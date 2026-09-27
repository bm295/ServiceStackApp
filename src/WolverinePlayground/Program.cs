using Wolverine;
using WolverinePlayground.Features.Todos;
using WolverinePlayground.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TodoStore>();
builder.Host.UseWolverine(options =>
{
    options.PublishMessage<TodoCreated>().ToLocalQueue("todo-events");
});

var app = builder.Build();

app.MapCreateTodo();
app.MapTodoQueries();

app.Run();
