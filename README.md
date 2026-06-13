# net-api - HttpClient Methods Practice Project

## Overview

This project demonstrates and practices core concepts from the [System.Net.Http API](https://learn.microsoft.com/en-us/dotnet/api/system.net.http?view=net-10.0). It's a practical ASP.NET Web API application that showcases real-world usage patterns of HttpClient for making HTTP requests and handling responses.

## Purpose

The examples in this project are designed to help developers understand and practice:

- HttpClient initialization and configuration
- Typed HttpClient patterns
- Request headers and default headers
- HTTP method implementations (GET, POST, PUT, DELETE, etc.)
- Request/response handling
- Error handling and resilience
- JSON serialization with System.Text.Json
- Integration with external APIs (e.g., GitHub API)

## Technology Stack

- **.NET** - Latest version (net-10.0)
- **ASP.NET Web API** - Slim builder pattern
- **HttpClient** - System.Net.Http
- **JSON Serialization** - System.Text.Json with AOT support

## Project Structure

- `HttpClientMethods/` - Main Web API project
  - `Endpoints/` - API endpoint handlers
  - `Services/` - Business logic and HTTP service implementations
  - `Http/` - HTTP-related utilities and configurations
  - `Models/` - Domain models
  - `Dtos/` - Data transfer objects
  - `Interfaces/` - Service contracts
  - `Program.cs` - Application startup and configuration

## References

For detailed information about the System.Net.Http namespace and its APIs, refer to:
- [System.Net.Http API Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.net.http?view=net-10.0)
- [HttpClient Class Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-10.0)