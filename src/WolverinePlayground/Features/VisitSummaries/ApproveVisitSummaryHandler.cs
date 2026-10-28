using WolverinePlayground.Storage;

namespace WolverinePlayground.Features.VisitSummaries;

public sealed class ApproveVisitSummaryHandler(VisitSummaryStore store, VisitSummaryValidator validator, DemoVisitCatalog catalog)
{
    public VisitSummary Handle(string id, ApproveVisitSummary command, string doctor)
    {
        var summary = store.Get(id) ?? throw new KeyNotFoundException();
        if (summary.Status != SummaryStatus.PendingReview || summary.Revision != command.ExpectedRevision)
            throw new InvalidOperationException("Only the current pending draft can be approved.");
        var current = catalog.Find(summary.Snapshot.Id);
        if (current is null || current.PatientId != summary.Snapshot.PatientId || current.Version != summary.Snapshot.Version)
            throw new InvalidOperationException("The visit record changed; generate a new draft.");
        var validation = validator.Validate(command.Content, current);
        if (!validation.SchemaValid || validation.Errors.Length != 0)
        {
            var rejected = summary with { Revision = summary.Revision + 1,
                History = [.. summary.History, new(DateTimeOffset.UtcNow, 0, "ApprovalRejected", validation.Errors)] };
            if (!store.Save(rejected, summary.Revision)) throw new InvalidOperationException("Draft changed concurrently.");
            throw new ArgumentException(string.Join("; ", validation.Errors));
        }
        var approved = summary with { Content = command.Content, Status = SummaryStatus.Approved,
            ApprovedBy = doctor, Revision = summary.Revision + 1,
            History = [.. summary.History, new(DateTimeOffset.UtcNow, 0, "ApprovedByDoctor", [])] };
        if (!store.Save(approved, summary.Revision)) throw new InvalidOperationException("Draft changed concurrently.");
        return approved;
    }
}
