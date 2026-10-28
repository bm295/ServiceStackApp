using System.Text.Json;
using System.Text.Json.Serialization;
using WolverinePlayground.Storage;

namespace WolverinePlayground.Features.VisitSummaries;

public sealed class VisitSummaryWorkflow(VisitSummaryStore store, IVisitSummaryGenerator generator, VisitSummaryValidator validator)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    public async Task<VisitSummary> RunAsync(Visit visit, string requestId, string scenario, CancellationToken cancellationToken)
    {
        var summary = new VisitSummary(Guid.NewGuid().ToString("N"), requestId, visit, SummaryStatus.Generating, null, []);
        if (!store.Insert(summary))
        {
            var existing = store.Get(requestId, true)!;
            if (existing.Snapshot.Id != visit.Id) throw new InvalidOperationException("Idempotency key belongs to another visit.");
            return existing;
        }
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            string[] errors;
            string outcome;
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(20));
                var raw = await generator.GenerateAsync(visit, scenario, attempt, timeout.Token).WaitAsync(timeout.Token);
                if (raw.Length > 100_000) throw new JsonException("Response too large.");
                var content = JsonSerializer.Deserialize<SummaryContent>(raw, JsonOptions);
                var validation = validator.Validate(content, visit);
                errors = validation.Errors;
                outcome = !validation.SchemaValid ? "InvalidSchema" : errors.Length > 0 ? "InvalidSources" : "Validated";
                if (validation.SchemaValid)
                {
                    summary = summary with { Content = errors.Length == 0 ? content : null,
                        Status = errors.Length == 0 ? SummaryStatus.PendingReview : SummaryStatus.NeedsManualReview };
                }
            }
            catch (JsonException) { errors = ["Generator returned invalid JSON."]; outcome = "InvalidJson"; }
            catch (Exception ex) when (ex is TimeoutException or OperationCanceledException or HttpRequestException)
            { errors = [cancellationToken.IsCancellationRequested ? "Request cancelled." : "Provider timeout or transport failure."]; outcome = "ProviderFailure"; }
            catch (Exception)
            { errors = ["Unexpected generator failure; check provider configuration and server diagnostics."]; outcome = "GeneratorFailure"; summary = summary with { Status = SummaryStatus.Failed }; }

            if (summary.Status == SummaryStatus.Generating && (attempt == 2 || cancellationToken.IsCancellationRequested))
                summary = summary with { Status = SummaryStatus.Failed };
            var previousRevision = summary.Revision;
            summary = summary with { Revision = previousRevision + 1,
                History = [.. summary.History, new(DateTimeOffset.UtcNow, attempt, outcome, errors)] };
            if (!store.Save(summary, previousRevision)) throw new InvalidOperationException("Concurrent workflow update.");
            if (summary.Status != SummaryStatus.Generating) return summary;
        }
        return summary;
    }
}
