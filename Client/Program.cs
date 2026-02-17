using System.Text.Json;
using RestSharp;

var client = new RestClient("https://jsonplaceholder.typicode.com");
var request = new RestRequest("todos/1");
var response = await client.GetAsync(request);

if (response?.Content is null)
{
    Console.WriteLine("No response content was returned.");
    return;
}

var todo = JsonSerializer.Deserialize<Todo>(response.Content);
if (todo is null)
{
    Console.WriteLine("Unable to deserialize response.");
    return;
}

Console.WriteLine($"UserId: {todo.UserId}");
Console.WriteLine($"Id: {todo.Id}");
Console.WriteLine($"Title: {todo.Title}");
Console.WriteLine($"Completed: {todo.Completed}");

internal sealed class Todo
{
    public int UserId { get; init; }
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool Completed { get; init; }
}
