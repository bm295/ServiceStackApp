using KafkaFlow;
using ServiceStackApp.ServiceModel;

namespace ServiceStackApp;

public class HelloMessageHandler : IMessageHandler<HelloMessage>
{
    public Task Handle(IMessageContext context, HelloMessage message)
    {
        var output = MessageFormatter.Format(
            context.ConsumerContext.Partition,
            context.ConsumerContext.Offset,
            message.Text);

        Console.WriteLine(output);
        return Task.CompletedTask;
    }
}
