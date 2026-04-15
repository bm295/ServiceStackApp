# KafkaFlow sample app (.NET 10)

This repository uses [KafkaFlow](https://farfetch.github.io/kafkaflow/) with a clearer producer/consumer-first folder layout.

## Project structure

- `src/Producer`: KafkaFlow **producer** console app
- `src/Consumer`: KafkaFlow **consumer** console app
- `src/Contracts`: shared message contracts (`HelloMessage`, `OrderCreatedMessage`)
- `tests/Consumer.Tests`: NUnit tests for consumer formatting logic

## Prerequisites

- .NET 10 SDK
- Kafka broker on `localhost:9092`

## Run

```bash
dotnet restore

# terminal 1: run consumers
dotnet run --project src/Consumer/Consumer.csproj

# terminal 2: send messages
dotnet run --project src/Producer/Producer.csproj
```

The app demonstrates support for topics with different message types:

- `hello-topic` consumes `HelloMessage` with `HelloMessageHandler`
- `orders-topic` consumes `OrderCreatedMessage` with `OrderCreatedMessageHandler`

## Middleware pipeline, DI, and lifetime control

Both producer and consumers define middleware pipelines with explicit order:

- Producer pipeline: `ProducerLoggingMiddleware` -> `GzipMessageCompressor` -> `JsonCoreSerializer`
- Consumer pipeline: `RetrySimple` -> `ConsumerLoggingMiddleware` -> `GzipMessageDecompressor` -> `JsonCoreDeserializer` -> typed handlers

Custom middleware classes are created through `Microsoft.Extensions.DependencyInjection` and use constructor
injection (`MiddlewareInstanceTracker`) to demonstrate DI-driven middleware activation.

The middleware is registered using the lifetime overload:

- `.Add<ProducerLoggingMiddleware>(MiddlewareLifetime.Singleton)`
- `.Add<ConsumerLoggingMiddleware>(MiddlewareLifetime.Singleton)`

Consumer exception handling uses `KafkaFlow.Retry` middleware with `RetrySimple(...).HandleAnyException()`.

## Consumer concurrency and ordering

Each consumer uses KafkaFlow worker parallelism and `PartitionKeyDistributionStrategy` so messages from the
same Kafka partition are always routed to the same worker (preserving partition order), while different
partitions are processed in parallel.
