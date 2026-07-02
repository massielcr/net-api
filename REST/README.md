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

## Project structure

The REST folder contains the following projects and example consumers:

REST/
├─ HttpClientMethods/ — ASP.NET Web API project with example endpoints and service-layer HttpClient usage.
├─ PluralsightKDStreams/ — Examples of streaming and external API integration.
└─ WebAPIClient/ — Reusable HTTP client helpers and simple consumer examples.

See each project's README for in-depth details.

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