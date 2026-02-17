using Funq;
using NUnit.Framework;
using ServiceStack;
using ServiceStackApp.ServiceInterface;
using ServiceStackApp.ServiceModel;

namespace ServiceStackApp.Tests;

public class IntegrationTest
{
    private const string BaseUri = "http://localhost:2000/";
    private readonly ServiceStackHost appHost;

    private class AppHost : AppSelfHostBase
    {
        public AppHost() : base(nameof(IntegrationTest), typeof(MyServices).Assembly)
        {
        }

        public override void Configure(Container container)
        {
        }
    }

    public IntegrationTest()
    {
        appHost = new AppHost()
            .Init()
            .Start(BaseUri);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => appHost.Dispose();

    private static IServiceClient CreateClient() => new JsonServiceClient(BaseUri);

    [Test]
    public void Can_call_Hello_Service()
    {
        var client = CreateClient();

        var response = client.Get(new Hello { Name = "World" });

        Assert.That(response.Result, Is.EqualTo("Hello, World!"));
    }
}
