using NUnit.Framework;
using ServiceStackApp;

namespace ServiceStackApp.Tests;

public class UnitTest
{
    [Test]
    public void MessageFormatter_formats_consumer_log_output()
    {
        var formatted = MessageFormatter.Format(2, 42, "Hello from tests");

        Assert.That(formatted, Is.EqualTo("Partition: 2 | Offset: 42 | Message: Hello from tests"));
    }
}
