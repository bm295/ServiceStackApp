using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackApp;

const string helloTopicName = "hello-topic";
const string ordersTopicName = "orders-topic";

var services = new ServiceCollection();

services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .CreateTopicIfNotExists(helloTopicName, 4, 1)
        .CreateTopicIfNotExists(ordersTopicName, 4, 1)
        .AddConsumer(consumer => consumer
            .Topic(helloTopicName)
            .WithGroupId("hello-group")
            .WithBufferSize(100)
            .WithWorkersCount(4)
            .WithWorkDistributionStrategy<PartitionKeyDistributionStrategy>()
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<HelloMessageHandler>())))
        .AddConsumer(consumer => consumer
            .Topic(ordersTopicName)
            .WithGroupId("orders-group")
            .WithBufferSize(100)
            .WithWorkersCount(4)
            .WithWorkDistributionStrategy<PartitionKeyDistributionStrategy>()
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<OrderCreatedMessageHandler>())))));

var serviceProvider = services.BuildServiceProvider();
var bus = serviceProvider.CreateKafkaBus();

await bus.StartAsync();

Console.WriteLine("KafkaFlow consumers are running. Press ENTER to stop.");
Console.ReadLine();

await bus.StopAsync();
