namespace WolverinePlayground.Features.Todos;

public record ActivityEntry(Guid TodoId, string Message, DateTimeOffset RecordedAt);
