using Newtonsoft.Json;
using RestSharp;
using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RestClient("https://jsonplaceholder.typicode.com");
            var request = new RestRequest("todos/1");
            var response = client.Get(request);
            var todo = JsonConvert.DeserializeObject<Todo>(response.Content);
            Console.WriteLine($"UserId: {todo.UserId}");
            Console.WriteLine($"Id: {todo.Id}");
            Console.WriteLine($"Title: {todo.Title}");
            Console.WriteLine($"Completed: {todo.Completed}");
            Console.ReadKey();
        }
    }

    class Todo
    { 
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Completed { get; set; }
    }
}
