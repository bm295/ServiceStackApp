using Funq;
using ServiceStack;
using ServiceStackApp.ServiceInterface;

namespace ServiceStackApp;

public class AppHost : AppHostBase
{
    public AppHost() : base("ServiceStackApp", typeof(MyServices).Assembly)
    {
    }

    public override void Configure(Container container)
    {
    }
}
