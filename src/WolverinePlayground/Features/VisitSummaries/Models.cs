using System.Text.Json.Serialization;

namespace WolverinePlayground.Features.VisitSummaries;

public sealed record SourceRecord(string Id, string PatientId, int Version, string Text);
public sealed record Visit(string Id, string PatientId, int Version, SourceRecord[] Sources);
public sealed record Citation([property: JsonRequired] string SourceId, [property: JsonRequired] int Version);
public sealed record SummaryItem([property: JsonRequired] string Text, [property: JsonRequired] Citation[] Sources);
public sealed record SummaryContent([property: JsonRequired] SummaryItem[] ReasonForVisit,
    [property: JsonRequired] SummaryItem[] RecordedInformation, [property: JsonRequired] SummaryItem[] MissingData);
public enum SummaryStatus { Generating, PendingReview, NeedsManualReview, Failed, Approved }
public sealed record ExecutionEntry(DateTimeOffset At, int Attempt, string Outcome, string[] Reasons);
public sealed record VisitSummary(string Id, string RequestId, Visit Snapshot, SummaryStatus Status,
    SummaryContent? Content, ExecutionEntry[] History, int Revision = 0, string? ApprovedBy = null);
public sealed record ApproveVisitSummary(int ExpectedRevision, SummaryContent Content);

// Synthetic, immutable record versions. Never send identifying demographics to the generator.
public sealed class DemoVisitCatalog
{
    public Visit[] Visits { get; } = [
        new("visit-1", "patient-1", 1, [new("note-1", "patient-1", 1, "Reason: intermittent headache. Duration: three days. Allergies not recorded.")]),
        new("visit-2", "patient-2", 1, [new("note-2", "patient-2", 1, "Reason: follow-up for knee pain. Medication list not recorded.")])
    ];
    public Visit? Find(string id) => Visits.SingleOrDefault(x => x.Id == id);
}
