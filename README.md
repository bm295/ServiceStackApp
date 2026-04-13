# KafkaFlow sample app (.NET 10)

This repository was rewritten to use [KafkaFlow](https://farfetch.github.io/kafkaflow/) instead of ServiceStack.

## Projects

- `ServiceStackApp/ServiceStackApp`: KafkaFlow **consumer** console app
- `Client`: KafkaFlow **producer** console app
- `ServiceStackApp/ServiceStackApp.ServiceModel`: shared message contract (`HelloMessage`)
- `ServiceStackApp/ServiceStackApp.Tests`: NUnit tests for local formatting logic used by the consumer

## Prerequisites

- .NET 10 SDK
- Kafka broker on `localhost:9092`

## Run

```bash
dotnet restore

# terminal 1: run consumer
dotnet run --project ServiceStackApp/ServiceStackApp/ServiceStackApp.csproj

# terminal 2: send one message
dotnet run --project Client/Client.csproj
```

The consumer subscribes to `sample-topic` with group id `sample-group`, and the producer sends a single `HelloMessage`.
