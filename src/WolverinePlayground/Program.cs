using Wolverine;
using WolverinePlayground.Features.Todos;
using WolverinePlayground.Storage;
using DbEncryption;
using Microsoft.AspNetCore.DataProtection;
using WolverinePlayground.Features.VisitSummaries;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TodoStore>();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));
builder.Services.AddSingleton<IDbEncryptionService, DataProtectionDbEncryptionService>();
builder.Services.AddSingleton(sp => new VisitSummaryStore(builder.Configuration.GetConnectionString("VisitSummaries") ??
    $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "visit-summaries.db")}", sp.GetRequiredService<IDbEncryptionService>()));
builder.Services.AddSingleton<DemoVisitCatalog>();
builder.Services.AddSingleton<VisitSummaryValidator>();
builder.Services.AddTransient<VisitSummaryWorkflow>();
builder.Services.AddTransient<ApproveVisitSummaryHandler>();
if (builder.Configuration["VisitSummaries:Provider"] == "Http")
    builder.Services.AddHttpClient<IVisitSummaryGenerator, HttpVisitSummaryGenerator>(c => c.Timeout = TimeSpan.FromSeconds(20));
else
    builder.Services.AddSingleton<IVisitSummaryGenerator, DemoVisitSummaryGenerator>();
builder.Host.UseWolverine(options =>
{
    options.PublishMessage<TodoCreated>().ToLocalQueue("todo-events");
});

var app = builder.Build();

app.MapCreateTodo();
app.MapTodoQueries();
app.MapVisitSummaries();

app.Run();
