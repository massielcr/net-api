# WebAPIClient

A .NET console application that demonstrates HTTP client usage with the GitHub REST API.

## Overview

WebAPIClient is a simple console application that:
- Connects to the GitHub REST API
- Retrieves public repositories from the `.NET Foundation` organization
- Displays repository information including name, description, URL, watchers count, and last push date

## Requirements

- .NET 10.0 or later
- Internet connection (to access GitHub API)

## Project Structure

```
WebAPIClient/
├── Program.cs              # Main application entry point
├── WebAPIClient.csproj     # Project file
└── Dtos/
    └── Repository.cs       # Data transfer object for GitHub repository
```

## Dtos

### Repository
A record class representing a GitHub repository with the following properties:
- `Name` - Repository name
- `Description` - Repository description
- `GitHubHomeUrl` - URL to the repository on GitHub
- `Homepage` - Project homepage URL
- `Watchers` - Number of watchers/stars
- `LastPush` - Last push date and time (converted to local time)

## Usage

1. Build the project:
   ```bash
   dotnet build
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. The application will fetch and display information about repositories from the `.NET Foundation` organization.

## Output Example

```
Name: runtime
Homepage: https://dotnet.microsoft.com
GitHub: https://github.com/dotnet/runtime
Description: .NET is a cross-platform runtime for cloud, mobile, desktop, and IoT applications.
Watchers: 14,500
Last push: 6/13/2026 12:30:45 PM
```

## Technical Details

- Uses `HttpClient` with GitHub REST API v3
- Serializes/deserializes JSON using `System.Net.Http.Json`
- Implements async/await pattern for API calls
- Uses JSON property mapping for GitHub API field names (e.g., `html_url` → `GitHubHomeUrl`)

## API Endpoint

- **Base URL:** `https://api.github.com`
- **Endpoint:** `GET /orgs/dotnet/repos`

## Future Enhancements

- Add filtering/sorting capabilities
- Support for pagination
- Command-line parameters for organization selection
- Error handling and retry logic
- Configuration file for customization
