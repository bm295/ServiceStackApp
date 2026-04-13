using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackApp;

const string topicName = "sample-topic";

var services = new ServiceCollection();

services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .CreateTopicIfNotExists(topicName, 1, 1)
        .AddConsumer(consumer => consumer
            .Topic(topicName)
            .WithGroupId("sample-group")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<HelloMessageHandler>())))));

var serviceProvider = services.BuildServiceProvider();
var bus = serviceProvider.CreateKafkaBus();

await bus.StartAsync();

Console.WriteLine("KafkaFlow consumer is running. Press ENTER to stop.");
Console.ReadLine();

await bus.StopAsync();
