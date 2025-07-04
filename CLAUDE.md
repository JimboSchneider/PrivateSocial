# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PrivateSocial is a .NET Aspire-based distributed application using:
- **.NET 9.0** with C#
- **Blazor Server** frontend with Interactive Server Components
- **ASP.NET Core Web API** backend
- **Redis** for caching
- **xUnit v3** for testing

## Essential Commands

### Development
```bash
# Run the entire application (recommended)
dotnet run --project PrivateSocial.AppHost

# This launches:
# - Aspire Dashboard: https://localhost:17253
# - Redis container
# - API Service
# - Web Frontend
```

### Build
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build [ProjectName]/[ProjectName].csproj
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test PrivateSocial.Tests/PrivateSocial.Tests.csproj

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Architecture

### Projects Structure
1. **PrivateSocial.AppHost** - Orchestrates the distributed application, manages service dependencies
2. **PrivateSocial.ServiceDefaults** - Shared configurations (OpenTelemetry, health checks, resilience)
3. **PrivateSocial.ApiService** - Backend API service
4. **PrivateSocial.Web** - Blazor Server frontend
5. **PrivateSocial.Tests** - Integration tests using Aspire testing framework

### Key Architectural Patterns
- **Service Discovery**: Services communicate via Aspire's built-in service discovery (e.g., "https+http://apiservice")
- **Health Checks**: All services expose `/health` and `/alive` endpoints
- **Resilience**: HTTP clients configured with retry policies and circuit breakers
- **Caching**: Redis output caching implemented in the Web project
- **Observability**: OpenTelemetry for distributed tracing, metrics, and logging

### Inter-Service Communication
- Web frontend calls API service using `WeatherApiClient` with service discovery
- Services registered in AppHost using `.AddProject<T>()` pattern
- Redis registered as container using `.AddRedis()`

### Frontend Architecture
- Blazor Server with Interactive Server Components
- Bootstrap CSS for styling
- Component structure: App.razor → Routes.razor → Layout → Pages
- Pages located in Components/Pages/
- Shared components in Components/

### API Architecture
- Minimal API pattern
- OpenAPI/Swagger enabled in development
- Problem Details for standardized error responses
- Weather forecast endpoint as example implementation

## Development Guidelines

### Adding New Services
1. Create new project in solution
2. Reference PrivateSocial.ServiceDefaults
3. Register in AppHost.cs using `builder.AddProject<ProjectName>()`
4. Configure service discovery if inter-service communication needed

### Adding New Pages
1. Create .razor file in PrivateSocial.Web/Components/Pages/
2. Add @page directive with route
3. Update NavMenu.razor if navigation link needed
4. Use dependency injection for services

### Testing Approach
- Use Aspire testing framework for integration tests
- Test distributed scenarios with `DistributedApplicationTestingBuilder`
- HttpClient available for API testing in test context