namespace WolverinePlayground.Features.VisitSummaries;

public sealed record ValidationResult(bool SchemaValid, string[] Errors);
public sealed class VisitSummaryValidator
{
    public ValidationResult Validate(SummaryContent? content, Visit visit)
    {
        if (content is null || content.ReasonForVisit is not { Length: > 0 } ||
            content.RecordedInformation is not { Length: > 0 } || content.MissingData is not { Length: > 0 })
            return new(false, ["All three sections must contain at least one sourced item."]);
        var items = content.ReasonForVisit.Concat(content.RecordedInformation).Concat(content.MissingData).ToArray();
        if (items.Length > 100 || items.Any(x => x is null || string.IsNullOrWhiteSpace(x.Text) || x.Text.Length > 4000 ||
            x.Sources is not { Length: > 0 } || x.Sources.Any(s => s is null || string.IsNullOrWhiteSpace(s.SourceId))))
            return new(false, ["Items require bounded text and nonempty citations."]);
        var errors = items.SelectMany(x => x.Sources).Where(c => !visit.Sources.Any(s =>
            s.Id == c.SourceId && s.PatientId == visit.PatientId && s.Version == c.Version))
            .Select(c => $"Invalid source, patient or version: {c.SourceId}@{c.Version}").Distinct().ToArray();
        return new(true, errors);
    }
}
