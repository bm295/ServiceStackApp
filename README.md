# ServiceStackApp (.NET 10 / C# 14-ready)

This repository has been modernized to SDK-style projects targeting **.NET 10** with **C# preview language features** (for C# 14 compatibility while .NET 10 is in preview).

## Projects

- `ServiceStackApp/ServiceStackApp`: ASP.NET Core + ServiceStack host
- `ServiceStackApp/ServiceStackApp.ServiceModel`: DTO contracts
- `ServiceStackApp/ServiceStackApp.ServiceInterface`: service implementations
- `ServiceStackApp/ServiceStackApp.Tests`: NUnit tests
- `Client`: example console client

## Run

```bash
dotnet restore
dotnet test ServiceStackApp/ServiceStackApp.Tests/ServiceStackApp.Tests.csproj
dotnet run --project ServiceStackApp/ServiceStackApp/ServiceStackApp.csproj
```
