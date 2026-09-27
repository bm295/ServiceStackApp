using WolverinePlayground.Storage;

namespace WolverinePlayground.Features.Todos;

// The cascading event is handled later on the local "todo-events" queue.
public static class TodoCreatedHandler
{
    public static void Handle(TodoCreated created, TodoStore store)
    {
        var todo = store.Find(created.Id);
        if (todo is not null)
        {
            store.Record(new ActivityEntry(todo.Id, $"Created: {todo.Title}", DateTimeOffset.UtcNow));
        }
    }
}
