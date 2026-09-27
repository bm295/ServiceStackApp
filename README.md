# KafkaFlow sample app (.NET 10)

This repository uses [KafkaFlow](https://farfetch.github.io/kafkaflow/) with a clearer producer/consumer-first folder layout.

## Project structure

- `src/Producer`: KafkaFlow **producer** console app
- `src/Consumer`: KafkaFlow **consumer** console app
- `src/Contracts`: shared message contracts (`HelloMessage`, `OrderCreatedMessage`)
- `src/DbEncryption`: shared Microsoft Data Protection helpers for protecting patient identifiable information before persistence
- `tests/Consumer.Tests`: NUnit tests for consumer formatting logic
- `tests/ServiceStackApp.Tests`: NUnit tests for patient identifiable information encryption

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

> Note: this sample demonstrates the built-in `KafkaFlow.Retry` package. It does **not** include
> [KafkaFlow Retry Extensions](https://github.com/Farfetch/kafkaflow-retry-extensions) configuration
> such as delayed topics or dead-letter topic routing.

## Consumer concurrency and ordering

Each consumer uses KafkaFlow worker parallelism and `PartitionKeyDistributionStrategy` so messages from the
same Kafka partition are always routed to the same worker (preserving partition order), while different
partitions are processed in parallel.

## DB encryption helpers

The `DbEncryption` class library provides a patient identifiable information encryption service backed by the Microsoft Data Protection API. Store each protected payload and purpose together so protected patient fields can be unprotected after retrieval. Configure Data Protection key persistence for the deployment environment so application instances can share and rotate keys safely.

Its folders follow an inward dependency direction:

- `Domain` contains the plain and encrypted value models.
- `Application` defines encryption ports and patient-field protection policy; it does not reference the Data Protection adapter.
- `Infrastructure` implements the database encryption port with Microsoft Data Protection and depends on the application contract.

The stable database-purpose prefix is owned by the application boundary and shared with the infrastructure adapter. This keeps persisted purpose values compatible without making application policy depend on a concrete encryption technology.
