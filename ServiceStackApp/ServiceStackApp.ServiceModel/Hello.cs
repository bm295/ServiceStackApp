using ServiceStack;

namespace ServiceStackApp.ServiceModel;

[Route("/hello/{Name}")]
public class Hello : IReturn<HelloResponse>
{
    public string Name { get; set; } = string.Empty;
}

public class HelloResponse
{
    public string Result { get; set; } = string.Empty;
}
