using KafkaFlow;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackApp.ServiceModel;

const string topicName = "sample-topic";
const string producerName = "say-hello";

var services = new ServiceCollection();

services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .CreateTopicIfNotExists(topicName, 4, 1)
        .AddProducer(
            producerName,
            producer => producer
                .DefaultTopic(topicName)
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>()))));

var serviceProvider = services.BuildServiceProvider();

var producer = serviceProvider
    .GetRequiredService<IProducerAccessor>()
    .GetProducer(producerName);

await producer.ProduceAsync(
    topicName,
    Guid.NewGuid().ToString(),
    new HelloMessage { Text = "Hello from KafkaFlow producer" });

Console.WriteLine("Message sent.");
