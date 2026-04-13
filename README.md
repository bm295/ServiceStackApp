# KafkaFlow sample app (.NET 10)

This repository uses [KafkaFlow](https://farfetch.github.io/kafkaflow/) with a clearer producer/consumer-first folder layout.

## Project structure

- `src/Producer`: KafkaFlow **producer** console app
- `src/Consumer`: KafkaFlow **consumer** console app
- `src/Contracts`: shared message contract (`HelloMessage`)
- `tests/Consumer.Tests`: NUnit tests for consumer formatting logic

## Prerequisites

- .NET 10 SDK
- Kafka broker on `localhost:9092`

## Run

```bash
dotnet restore

# terminal 1: run consumer
dotnet run --project src/Consumer/Consumer.csproj

# terminal 2: send one message
dotnet run --project src/Producer/Producer.csproj
```

The consumer subscribes to `sample-topic` with group id `sample-group`, and the producer sends a single `HelloMessage`.
