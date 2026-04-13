using KafkaFlow;

namespace ServiceStackApp.ServiceModel;

public class ProducerLoggingMiddleware(MiddlewareInstanceTracker tracker) : IMessageMiddleware
{
    private readonly string _instanceId = tracker.GetOrCreateId<ProducerLoggingMiddleware>();

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        Console.WriteLine(
            $"[ProducerMiddleware:{_instanceId}] Topic={context.ProducerContext.Topic} Key={context.Message.Key}");

        await next(context);
    }
}
