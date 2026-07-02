## net-api

A small collection of .NET example projects that illustrate HttpClient patterns, streaming HTTP scenarios, and reusable client code.

---

### HttpClientMethods
A minimal ASP.NET Web API demonstrating HttpClient usage patterns and service-layer abstractions.

| Component | Path | Description |
|---|---|---|
| Project file | `HttpClientMethods/HttpClientMethods.csproj` | Project configuration and target framework |
| Entrypoint | `HttpClientMethods/Program.cs` | Application startup and endpoint registration |
| Configuration | `HttpClientMethods/appsettings*.json` | Runtime settings used by examples |
| HTTP examples | `HttpClientMethods/Http/` | .http requests for manual testing (REST Client / curl) |
| Endpoints | `HttpClientMethods/Endpoints/` | Minimal API handlers organized by scenario |
| Services | `HttpClientMethods/Services/` | Service-layer implementations that use HttpClient |
| Interfaces | `HttpClientMethods/Interfaces/` | Contracts used for DI and testing |
| DTOs | `HttpClientMethods/Dtos/` | Request/response data shapes |
| Models | `HttpClientMethods/Models/` | Domain types (e.g. GitHubIssue) |

---

### PluralsightKDStreams
Streaming-focused examples showcasing cancellation and resilience (retry) patterns.

| Component | Path | Description |
|---|---|---|
| Project file | `PluralsightKDStreams/PluralsightKDStreams.csproj` | Project configuration |
| Entrypoint | `PluralsightKDStreams/Program.cs` | App startup and demo endpoints |
| Configuration | `PluralsightKDStreams/appsettings*.json` | Settings for streaming demos |
| Handlers | `PluralsightKDStreams/Handlers/` | Delegating handlers (retry, policies) |
| Services | `PluralsightKDStreams/Services/` | Streaming and cancellation logic |
| Endpoints | `PluralsightKDStreams/Endpoints/` | Demo endpoints exposing streams |
| DTOs | `PluralsightKDStreams/Dtos/` | Data shapes for stream payloads |
| HTTP examples | `PluralsightKDStreams/Http/` | .http examples for streaming scenarios |
| Tests | `PluralsightKDStreams.Tests/` | Unit tests for streamer and cancellation behavior |

---

### WebAPIClient
Reusable HTTP client helpers and small example consumers.

| Component | Path | Description |
|---|---|---|
| Project file | `WebAPIClient/WebAPIClient.csproj` | Project configuration |
| Entrypoint | `WebAPIClient/Program.cs` | Example client runners |
| DTOs | `WebAPIClient/Dtos/` | DTOs used by client code |
| Examples | `WebAPIClient/README.md` | Usage examples and quick-run commands |

---


