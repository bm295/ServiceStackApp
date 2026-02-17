using ServiceStack;
using ServiceStackApp;
using ServiceStackApp.ServiceInterface;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServiceStack(typeof(MyServices).Assembly);

var app = builder.Build();
app.UseServiceStack(new AppHost());

app.Run();
