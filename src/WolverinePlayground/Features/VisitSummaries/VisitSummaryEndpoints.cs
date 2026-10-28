using WolverinePlayground.Storage;

namespace WolverinePlayground.Features.VisitSummaries;

public sealed record GenerateVisitSummary(string RequestId, string Scenario = "valid");
public static class VisitSummaryEndpoints
{
    public static void MapVisitSummaries(this WebApplication app)
    {
        var group = app.MapGroup("/visit-summaries");
        // Demo-only access control: each configured secret identifies a doctor. Fail closed.
        group.AddEndpointFilter(async (context, next) =>
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var key = context.HttpContext.Request.Headers["X-Doctor-Key"].ToString();
            var doctor = config.GetSection("VisitSummaries:Doctors").GetChildren()
                .FirstOrDefault(x => !string.IsNullOrEmpty(x.Value) && x.Value == key)?.Key;
            if (doctor is null) return Results.Unauthorized();
            context.HttpContext.Items["Doctor"] = doctor;
            try { return await next(context); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
        });
        group.MapGet("/visits", (DemoVisitCatalog catalog) => catalog.Visits);
        group.MapGet("/{id}", (string id, VisitSummaryStore store) => store.Get(id) is { } summary ? Results.Ok(summary) : Results.NotFound());
        group.MapPost("/visits/{visitId}", async (string visitId, GenerateVisitSummary request, DemoVisitCatalog catalog,
            VisitSummaryWorkflow workflow, IConfiguration config, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.RequestId) || request.RequestId.Length > 100) return Results.BadRequest("RequestId required (max 100).");
            if (request.Scenario is not ("valid" or "foreign-source" or "timeout" or "invalid-json" or "recover")) return Results.BadRequest("Unknown scenario.");
            if (config["VisitSummaries:Provider"] == "Http" && request.Scenario != "valid") return Results.BadRequest("Scenarios require Demo provider.");
            var visit = catalog.Find(visitId);
            return visit is null ? Results.NotFound() : Results.Ok(await workflow.RunAsync(visit, request.RequestId, request.Scenario, ct));
        });
        group.MapPost("/{id}/approve", (string id, ApproveVisitSummary request, ApproveVisitSummaryHandler handler, HttpContext context) =>
            Results.Ok(handler.Handle(id, request, (string)context.Items["Doctor"]!)));
    }
}
