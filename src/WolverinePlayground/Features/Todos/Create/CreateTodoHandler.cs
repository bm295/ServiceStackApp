using WolverinePlayground.Storage;

namespace WolverinePlayground.Features.Todos;

// Wolverine discovers this handler by convention. Its return value is a cascading message.
public static class CreateTodoHandler
{
    public static TodoCreated Handle(CreateTodo command, TodoStore store)
    {
        store.Add(new Todo(command.Id, command.Title, DateTimeOffset.UtcNow));
        return new TodoCreated(command.Id);
    }
}
