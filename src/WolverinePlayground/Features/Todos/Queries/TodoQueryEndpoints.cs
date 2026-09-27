using WolverinePlayground.Storage;

namespace WolverinePlayground.Features.Todos;

public static class TodoQueryEndpoints
{
    public static void MapTodoQueries(this WebApplication app)
    {
        app.MapGet("/todos", (TodoStore store) => Results.Ok(store.All()));
        app.MapGet("/todos/{id:guid}", (Guid id, TodoStore store) =>
            store.Find(id) is { } todo ? Results.Ok(todo) : Results.NotFound());
        app.MapGet("/activity", (TodoStore store) => Results.Ok(store.Activity()));
    }
}
