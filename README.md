# net-api - System.Net.Http API Practice

## Overview

This repository contains a comprehensive collection of projects designed to explore and practice core concepts from the [System.Net.Http API](https://learn.microsoft.com/en-us/dotnet/api/system.net.http?view=net-10.0). It demonstrates real-world patterns for making HTTP requests, handling responses, building resilient HTTP-based services, and integrating with external APIs in .NET.

## Solution Projects

### 1. **HttpClientMethods** 🌐
A comprehensive ASP.NET Web API project showcasing practical implementations and patterns for using HttpClient. This is the primary reference for HttpClient usage patterns.

**Key Features:**
- HTTP method implementations (GET, POST, PUT, PATCH, DELETE)
- `GetAsync` and `GetStreamAsync` patterns for efficient data retrieval
- `GetByteArrayAsync` for binary content handling
- `GetStringAsync` for simple text responses
- `SendAsync` for advanced request customization
- Request/response handling and HTTP headers management
- GitHub API integration with authentication
- Error handling with global exception middleware
- JSON serialization with AOT-friendly System.Text.Json
- Cancellation token support
- File operations and streaming capabilities

**Technology Stack:**
- .NET 10.0
- ASP.NET Core Web API (Slim Builder Pattern)
- HttpClient with named client configuration
- System.Text.Json
- OpenAPI/Swagger support

**Project Structure:**
```
HttpClientMethods/
├── Program.cs                    # Application startup and DI configuration
├── Endpoints/                    # API endpoint handlers (organized by HTTP method)
├── Services/                     # Business logic and HTTP service implementations
├── Http/                         # HTTP utilities and configurations
├── Models/                       # Domain models
├── Dtos/                         # Data transfer objects
├── Interfaces/                   # Service contracts
├── AppJsonSerializerContext.cs   # AOT-friendly JSON serialization context
├── appsettings.json              # Application configuration
├── appsettings.Development.json  # Development-specific settings
├── wwwroot/                      # Static web assets
└── HttpClientMethods.csproj      # Project file
```

---

### 2. **PluralsightKDStreams** 📡
A project demonstrating advanced HTTP client usage patterns, particularly focused on streaming, content negotiation, and resilience patterns.

**Key Features:**
- Streaming HTTP responses with `GetStreamAsync`
- Custom resilience policies and retry mechanisms
- Polly integration for advanced policy handling
- Custom delegating handlers for cross-cutting concerns
- Request decompression handling
- Local service integration patterns
- Streamer service for efficient data transfer

**Technology Stack:**
- .NET 10.0
- ASP.NET Core Web API
- HttpClient with custom message handlers
- Polly for resilience policies
- System.Text.Json

**Project Structure:**
```
PluralsightKDStreams/
├── Program.cs                    # Application startup with resilience policies
├── Endpoints/                    # Streaming and specialized endpoints
├── Services/                     # Streamer and HTTP-related services
├── Handlers/                     # Custom message handlers (RetryPolicyDelegatingHandler)
├── Dtos/                         # Data transfer objects
├── Interfaces/                   # Service contracts
├── AppJsonSerializerContext.cs   # AOT-friendly JSON serialization context
├── appsettings.json              # Application configuration
└── PluralsightKDStreams.csproj   # Project file
```

---

### 3. **PluralsightKDStreams.Tests** 🧪
Unit tests for the PluralsightKDStreams project using xUnit framework.

**Test Coverage:**
- `StreamerServiceTests.cs` - Tests for streaming service functionality
- `CancellationServiceTests.cs` - Tests for cancellation token handling

---

### 4. **WebAPIClient** 🔧
A utility library providing reusable HttpClient implementations and helpers for consuming web APIs. This serves as a foundation for other projects.

**Purpose:**
- Provides common HTTP client patterns
- Reusable configurations and handlers
- Base implementations for typed clients

---

## Notes

- All projects use modern .NET 10.0 with the latest language features
- The slim builder pattern in HttpClientMethods provides a minimal hosting model for lightweight APIs
- Both APIs include comprehensive error handling and logging
- Tests use xUnit for unit testing
- Solutions support AOT compilation through proper JSON serialization context setup

---

## License

This repository is for educational purposes.

---

## Contributing

This is a practice/learning repository. Feel free to explore, modify, and extend the projects to deepen your understanding of the System.Net.Http API and ASP.NET Core patterns.