# net-api

A small collection of .NET example projects demonstrating HttpClient patterns, streaming HTTP scenarios, and lightweight Web APIs.

## Description

This README concentrates on the three projects that make up the REST solution. Each section below includes a one-line summary and a high-level tree of that project's main components.

---

### HttpClientMethods
A minimal ASP.NET Web API showing common HttpClient usage patterns, endpoint handlers, and service-layer abstractions.

HttpClientMethods/
- HttpClientMethods.csproj
- Program.cs
- appsettings.json, appsettings.Development.json
- Properties/launchSettings.json
- Http/  (example HTTP requests for tools like VS Code REST Client)
  - GetAsync.http, GetStreamAsync.http, GetByteArrayAsync.http, GetStringAsync.http, PostAsync.http, PutAsync.http, PatchAsync.http, DeleteAsync.http, SendAsync.http
- Endpoints/  (minimal API handlers organized by scenario)
  - GetAsyncEndpoints.cs, GetStreamAsyncEndpoints.cs, GetByteArrayAsyncEndpoints.cs, GetStringAsyncEndpoints.cs, PostAsyncEndpoints.cs, PutAsyncEndpoints.cs, PatchAsyncEndpoints.cs, DeleteAsyncEndpoints.cs, SendAsyncEndpoints.cs, CancellationEndpoints.cs
- Services/  (service-layer implementations used by endpoints)
  - GetAsyncEndpointsService.cs, GetStreamAsyncEndpointsService.cs, GetByteArrayAsyncEndpointsService.cs, GetStringAsyncEndpointsService.cs, PostAsyncEndpointsService.cs, PutAsyncEndpointsService.cs, PatchAsyncEndpoinsService.cs  (filename includes existing typo), SendAsyncEndpointsService.cs, DeleteAsyncEndpointsService.cs, FileService.cs, CancellationService.cs
- Interfaces/  (service contracts)
  - IGetAsyncEndpointsService.cs, IGetStreamAsyncEndpointsService.cs, IGetByteArrayAsyncEndpointsService.cs, IGetStringAsyncEndpointsService.cs, IPostAsyncEndpointsService.cs, IPutAsyncEndpointsService.cs, IPatchAsyncEndpoinsService.cs, ISendAsyncEndpointsService.cs, IDeleteAsyncEndpointsService.cs, ICancellationService.cs
- Models/  (domain models)
  - GitHubIssue.cs, GitHubAvatar.cs
- Dtos/  (request/response DTOs)
  - UpdateRepoRequestDto.cs, UpdateRepoTopicsRequestDto.cs, etc.

---

### PluralsightKDStreams
Examples and tests demonstrating streaming HTTP scenarios and cancellation/retry handling.

PluralsightKDStreams/
- PluralsightKDStreams.csproj
- Program.cs
- appsettings.json, appsettings.Development.json
- AppJsonSerializerContext.cs
- Handlers/
  - RetryPolicyDelegatingHandler.cs
- Services/
  - StreamerService.cs, CancellationService.cs
- Endpoints/
- Dtos/
  - PosterDto.cs
- Http/
  - Streams.http

PluralsightKDStreams.Tests/
- PluralsightKDStreams.Tests.csproj
- StreamerServiceTests.cs
- CancellationServiceTests.cs

---

### WebAPIClient
Reusable HTTP client implementations and small example consumers.

WebAPIClient/
- WebAPIClient.csproj
- Program.cs
- Dtos/
  - Repository.cs
- README.md (project-specific examples)

---

## Notes
- This README focuses only on the three projects above and their high-level structure.
- Filenames reflect the repository state (including a small typo in the Patch service filename).
- For runnable examples and detailed usage, consult each project's README.

## Author
- massielcr — https://github.com/massielcr

**Last Updated:** July 02, 2026

