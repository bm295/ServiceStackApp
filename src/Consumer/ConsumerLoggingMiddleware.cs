using KafkaFlow;

namespace ServiceStackApp;

public class ConsumerLoggingMiddleware(MiddlewareInstanceTracker tracker) : IMessageMiddleware
{
    private readonly string _instanceId = tracker.GetOrCreateId<ConsumerLoggingMiddleware>();

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        Console.WriteLine(
            $"[ConsumerMiddleware:{_instanceId}] Topic={context.ConsumerContext.Topic} Partition={context.ConsumerContext.Partition} Offset={context.ConsumerContext.Offset}");

        await next(context);
    }
}
