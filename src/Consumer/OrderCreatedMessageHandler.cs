using KafkaFlow;
using ServiceStackApp.ServiceModel;

namespace ServiceStackApp;

public class OrderCreatedMessageHandler : IMessageHandler<OrderCreatedMessage>
{
    public Task Handle(IMessageContext context, OrderCreatedMessage message)
    {
        var output = MessageFormatter.Format(
            context.ConsumerContext.Partition,
            context.ConsumerContext.Offset,
            $"Order Created | Id: {message.OrderId} | Total: {message.Total}");

        Console.WriteLine(output);
        return Task.CompletedTask;
    }
}
