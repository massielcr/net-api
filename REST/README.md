# net-api

A small collection of .NET projects that demonstrate practical patterns for using System.Net.Http (HttpClient), streaming HTTP scenarios, and building lightweight Web APIs.

## Overview

This repository is intended for learning and experimentation. It includes example servers and clients that show how to:
- Make robust HTTP requests with HttpClient
- Handle streaming responses
- Compose retry and timeout policies
- Serialize/deserialize JSON with System.Text.Json

## Projects

- HttpClientMethods — ASP.NET Web API project with example endpoints and service-layer HttpClient usage.
- PluralsightKDStreams — Examples of streaming and external API integration.
- WebAPIClient — Reusable HTTP client helpers and simple consumer examples.

See each project's README for in-depth details.

## Projects

This README focuses on the three projects in the REST solution. Each project has its own section below with a short description and a tree-style listing of its main components.

### HttpClientMethods
A sample ASP.NET Web API demonstrating HttpClient usage patterns, endpoint handlers, and service-layer abstractions.

HttpClientMethods/
├─ HttpClientMethods.csproj
├─ Program.cs
├─ appsettings.json
├─ appsettings.Development.json
├─ Properties/
│  └─ launchSettings.json
├─ Http/                # HTTP request examples for VS Code REST Client / tools
│  ├─ GetAsync.http
│  ├─ GetStreamAsync.http
│  ├─ GetByteArrayAsync.http
│  ├─ GetStringAsync.http
│  ├─ PostAsync.http
│  ├─ PutAsync.http
│  ├─ PatchAsync.http
│  ├─ DeleteAsync.http
│  └─ SendAsync.http
├─ Endpoints/           # Minimal API endpoint handlers
│  ├─ GetAsyncEndpoints.cs
│  ├─ GetStreamAsyncEndpoints.cs
│  ├─ GetByteArrayAsyncEndpoints.cs
│  ├─ GetStringAsyncEndpoints.cs
│  ├─ PostAsyncEndpoints.cs
│  ├─ PutAsyncEndpoints.cs
│  ├─ PatchAsyncEndpoints.cs
│  ├─ DeleteAsyncEndpoints.cs
│  ├─ SendAsyncEndpoints.cs
│  └─ CancellationEndpoints.cs
├─ Services/            # Service implementations used by endpoints
│  ├─ GetAsyncEndpointsService.cs
│  ├─ GetStreamAsyncEndpointsService.cs
│  ├─ GetByteArrayAsyncEndpointsService.cs
│  ├─ GetStringAsyncEndpointsService.cs
│  ├─ PostAsyncEndpointsService.cs
│  ├─ PutAsyncEndpointsService.cs
│  ├─ PatchAsyncEndpoinsService.cs   # note: filename includes existing typo
│  ├─ SendAsyncEndpointsService.cs
│  ├─ DeleteAsyncEndpointsService.cs
│  ├─ FileService.cs
│  └─ CancellationService.cs
├─ Interfaces/          # Service and handler contracts
│  ├─ IGetAsyncEndpointsService.cs
│  ├─ IGetStreamAsyncEndpointsService.cs
│  ├─ IGetByteArrayAsyncEndpointsService.cs
│  ├─ IGetStringAsyncEndpointsService.cs
│  ├─ IPostAsyncEndpointsService.cs
│  ├─ IPutAsyncEndpointsService.cs
│  ├─ IPatchAsyncEndpoinsService.cs
│  ├─ ISendAsyncEndpointsService.cs
│  ├─ IDeleteAsyncEndpointsService.cs
│  └─ ICancellationService.cs
├─ Models/
│  ├─ GitHubIssue.cs
│  └─ GitHubAvatar.cs
└─ Dtos/                # DTOs used for requests/responses
   ├─ UpdateRepoRequestDto.cs
   ├─ UpdateRepoTopicsRequestDto.cs
   └─ (other DTO files)

---

### PluralsightKDStreams
Examples of streaming HTTP scenarios, handlers for retry/cancellation, and related test coverage.

PluralsightKDStreams/
├─ PluralsightKDStreams.csproj
├─ Program.cs
├─ appsettings.json
├─ appsettings.Development.json
├─ AppJsonSerializerContext.cs
├─ Handlers/
│  └─ RetryPolicyDelegatingHandler.cs
├─ Services/
│  ├─ StreamerService.cs
│  └─ CancellationService.cs
├─ Endpoints/
├─ Dtos/
│  └─ PosterDto.cs
└─ Http/
   └─ Streams.http

PluralsightKDStreams.Tests/
├─ PluralsightKDStreams.Tests.csproj
├─ StreamerServiceTests.cs
└─ CancellationServiceTests.cs

---

### WebAPIClient
Reusable HTTP client implementations and simple consumer examples.

WebAPIClient/
├─ WebAPIClient.csproj
├─ Program.cs
└─ Dtos/
   └─ Repository.cs

---

Notes:
- This file concentrates on the three projects above and omits other top-level REST folder details.
- For full usage, examples, and runnable instructions, see each project's README under its directory.

## Technology
## Technology

- .NET (net-8.0 / net-10.0)
- ASP.NET Web API
- System.Net.Http (HttpClient)
- System.Text.Json

## Contributing

Contributions and improvements are welcome — open an issue or a pull request.

## License

See LICENSE in the repository root (if present).

## References

- https://learn.microsoft.com/en-us/dotnet/api/system.net.http
- https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient