# net-api

A small collection of .NET example projects that illustrate HttpClient patterns, streaming HTTP scenarios, and reusable client code.

## Summary

This README provides a concise, top-level summary of the projects contained in the REST solution. Each project section below includes a one-line description and a compact table mapping the main components to their purpose.

---

### HttpClientMethods
A minimal ASP.NET Web API demonstrating common HttpClient usage patterns, endpoint handlers, and service-layer abstractions.

| Component | Path | Purpose |
|---|---|---|
| Project file | HttpClientMethods/HttpClientMethods.csproj | Project configuration and target frameworks |
| Entrypoint | HttpClientMethods/Program.cs | Application startup and endpoint registration |
| Configuration | HttpClientMethods/appsettings*.json | Runtime settings for examples |
| HTTP examples | HttpClientMethods/Http/ | .http files for manual API testing (curl/REST Client) |
| Endpoints | HttpClientMethods/Endpoints/ | Minimal API handlers showcasing request/response patterns |
| Services | HttpClientMethods/Services/ | Service-layer logic and HttpClient usage examples |
| Interfaces | HttpClientMethods/Interfaces/ | Contracts for service implementations used by endpoints |
| DTOs | HttpClientMethods/Dtos/ | Request and response DTO classes |
| Models | HttpClientMethods/Models/ | Domain model types used across the project |

---

### PluralsightKDStreams
Examples and tests focused on streaming HTTP scenarios, cancellation tokens, and resilience handlers.

| Component | Path | Purpose |
|---|---|---|
| Project file | PluralsightKDStreams/PluralsightKDStreams.csproj | Project configuration |
| Entrypoint | PluralsightKDStreams/Program.cs | App startup and demo endpoints |
| Configuration | PluralsightKDStreams/appsettings*.json | Settings used by streaming examples |
| Handlers | PluralsightKDStreams/Handlers/ | Delegating handlers (e.g., retry, policies) |
| Services | PluralsightKDStreams/Services/ | Streaming and cancellation logic implementations |
| Endpoints | PluralsightKDStreams/Endpoints/ | Demo endpoints exposing stream scenarios |
| DTOs | PluralsightKDStreams/Dtos/ | DTOs used by stream producers/consumers |
| HTTP examples | PluralsightKDStreams/Http/ | .http files demonstrating streaming requests |
| Tests | PluralsightKDStreams.Tests/ | Unit tests for StreamerService and cancellation behavior |

---

### WebAPIClient
Lightweight project with reusable HTTP client helpers and small example consumers.

| Component | Path | Purpose |
|---|---|---|
| Project file | WebAPIClient/WebAPIClient.csproj | Project configuration |
| Entrypoint | WebAPIClient/Program.cs | Example client runners |
| DTOs | WebAPIClient/Dtos/ | Data transfer objects used by client code |
| Examples | WebAPIClient/README.md | Project-specific usage examples and quick-run commands |

---

## Notes
- This README offers a high-level overview and component map for the three projects only; it intentionally omits low-level file listings.
- Filenames reflect the repository state (including a small typo in the Patch service filename).
- For runnable examples and deeper guidance, open the README inside each project folder.

## Author
- massielcr — https://github.com/massielcr

**Last Updated:** July 02, 2026
