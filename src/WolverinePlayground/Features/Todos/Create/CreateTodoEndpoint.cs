using Wolverine;

namespace WolverinePlayground.Features.Todos;

public static class CreateTodoEndpoint
{
    public static void MapCreateTodo(this WebApplication app)
    {
        app.MapPost("/todos", async (NewTodo request, IMessageBus bus) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { error = "Title is required." });
            }

            var id = Guid.NewGuid();
            await bus.InvokeAsync(new CreateTodo(id, request.Title.Trim()));
            return Results.Created($"/todos/{id}", new { id });
        });
    }
}
