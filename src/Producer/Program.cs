using KafkaFlow;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackApp.ServiceModel;

const string helloTopicName = "hello-topic";
const string ordersTopicName = "orders-topic";
const string producerName = "sample-producer";

var services = new ServiceCollection();
services.AddSingleton<MiddlewareInstanceTracker>();

services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .CreateTopicIfNotExists(helloTopicName, 4, 1)
        .CreateTopicIfNotExists(ordersTopicName, 4, 1)
        .AddProducer(
            producerName,
            producer => producer
                .DefaultTopic(helloTopicName)
                .AddMiddlewares(m => m
                    .Add<ProducerLoggingMiddleware>(MiddlewareLifetime.Singleton)
                    .AddSerializer<JsonCoreSerializer>()))));

var serviceProvider = services.BuildServiceProvider();

var producer = serviceProvider
    .GetRequiredService<IProducerAccessor>()
    .GetProducer(producerName);

await producer.ProduceAsync(
    helloTopicName,
    Guid.NewGuid().ToString(),
    new HelloMessage { Text = "Hello from KafkaFlow producer" });

await producer.ProduceAsync(
    ordersTopicName,
    Guid.NewGuid().ToString(),
    new OrderCreatedMessage
    {
        OrderId = Guid.NewGuid().ToString("N"),
        Total = 149.99m
    });

Console.WriteLine("Messages sent to hello-topic and orders-topic.");
