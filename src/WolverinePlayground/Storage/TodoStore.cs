using System.Collections.Concurrent;
using WolverinePlayground.Features.Todos;

namespace WolverinePlayground.Storage;

public sealed class TodoStore
{
    private readonly ConcurrentDictionary<Guid, Todo> _todos = new();
    private readonly ConcurrentQueue<ActivityEntry> _activity = new();

    public void Add(Todo todo) => _todos[todo.Id] = todo;
    public Todo? Find(Guid id) => _todos.GetValueOrDefault(id);
    public Todo[] All() => _todos.Values.OrderBy(todo => todo.CreatedAt).ToArray();
    public void Record(ActivityEntry entry) => _activity.Enqueue(entry);
    public ActivityEntry[] Activity() => _activity.ToArray();
}
