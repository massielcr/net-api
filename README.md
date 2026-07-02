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

## Getting Started

### Prerequisites
- .NET 10.0 SDK or later
- Visual Studio 2022 / VS Code with C# extension (optional)

### Running the Projects

#### HttpClientMethods API
```bash
cd HttpClientMethods
dotnet run
```
The API will be available at `https://localhost:5217` (or `http://localhost:5216`)

#### PluralsightKDStreams API
```bash
cd PluralsightKDStreams
dotnet run
```

#### Running Tests
```bash
dotnet test PluralsightKDStreams.Tests
```

### Environment Variables
Some endpoints (e.g., GitHub API integration) may require authentication:
```bash
export GITHUB_TOKEN=your_github_token_here
```

---

## Key Concepts Demonstrated

### 1. HttpClient Configuration
- Named clients with dependency injection
- Base address configuration
- Default request headers management
- Custom message handlers

### 2. HTTP Methods
- **GET** - Retrieving data with variants (GetAsync, GetStreamAsync, GetByteArrayAsync, GetStringAsync)
- **POST** - Creating resources with request bodies
- **PUT** - Updating entire resources
- **PATCH** - Partial resource updates
- **DELETE** - Removing resources
- **SendAsync** - Low-level request control

### 3. Resilience Patterns
- Retry policies using Polly
- Custom delegating handlers for retry logic
- Automatic decompression support
- Error handling and recovery

### 4. Advanced Patterns
- Streaming large responses efficiently
- Cancellation token propagation
- Global exception handling
- AOT-friendly JSON serialization
- External API integration (GitHub API)
- File operations with HTTP streams

### 5. Best Practices
- Singleton HttpClient instances
- Proper disposal and resource management
- Structured logging
- OpenAPI/Swagger documentation
- Request/response DTOs
- Service layer abstraction

---

## API Endpoints

### HttpClientMethods
- **GET Endpoints**: `/api/getasync/*`, `/api/getstream/*`, `/api/getbytearray/*`, `/api/getstring/*`
- **POST Endpoints**: `/api/postasync/*`
- **PUT Endpoints**: `/api/putasync/*`
- **PATCH Endpoints**: `/api/patchasync/*`
- **DELETE Endpoints**: `/api/deleteasync/*`
- **Advanced**: `/api/sendasync/*`
- **Cancellation**: `/api/cancellation/*`

For full OpenAPI documentation, visit `/openapi/v1.json` or use the Swagger UI in development.

### PluralsightKDStreams
- **Streaming Endpoints**: `/api/streamer/*`
- **Cancellation Endpoints**: `/api/cancellation/*`

---

## Architecture Notes

### Dependency Injection
Both projects use ASP.NET Core's built-in DI container:
```csharp
// Named HttpClient configuration
builder.Services.AddHttpClient("GitHub", client =>
{
    client.BaseAddress = new Uri("https://api.github.com");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
});

// Scoped services for endpoint handling
builder.Services.AddScoped<IGetAsyncEndpointsService, GetAsyncEndpointsService>();
```

### Exception Handling
Global exception middleware ensures consistent error responses:
```csharp
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        // Log and return safe error response
        await context.Response.WriteAsJsonAsync(
            new ErrorResponse(500, "An unexpected server error occurred.")
        );
    });
});
```

### JSON Serialization
AOT-friendly JSON serialization context for performance and trimming support:
```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.WriteIndented = true;
});
```

---

## Learning Resources

### Official Microsoft Documentation
- [System.Net.Http API](https://learn.microsoft.com/en-us/dotnet/api/system.net.http?view=net-10.0)
- [HttpClient Class](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-10.0)
- [HttpMessageHandler](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpmessagehandler?view=net-10.0)
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/api/system.text.json?view=net-10.0)
- [ASP.NET Core Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)

### Related Libraries
- [Polly - Resilience Policies](https://github.com/App-vNext/Polly)
- [GitHub API Documentation](https://docs.github.com/en/rest)

---

## Project Organization

The solution uses a clean architecture approach:
- **Endpoints** - Route handlers and request/response mapping
- **Services** - Business logic and external API calls
- **Interfaces** - Abstraction layer for dependency injection
- **Models/DTOs** - Data structures
- **Handlers** - Custom HttpMessageHandlers for cross-cutting concerns
- **Http** - HTTP utilities and configurations

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