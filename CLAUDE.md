# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PrivateSocial is a .NET Aspire-based distributed application using:
- **.NET 9.0** with C#
- **React + TypeScript** frontend with Vite bundler
- **ASP.NET Core Web API** backend with Controller architecture
- **SQL Server** database with Entity Framework Core (Code First)
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
# - SQL Server container with database
# - API Service
# - Web Frontend (Docker container with nginx)
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

### Frontend Development
```bash
# Install dependencies
cd PrivateSocial.Web.React && npm install

# Run development server (standalone)
cd PrivateSocial.Web.React && npm run dev

# Build for production
cd PrivateSocial.Web.React && npm run build

# Lint
cd PrivateSocial.Web.React && npm run lint
```

## Architecture

### Projects Structure
1. **PrivateSocial.AppHost** - Orchestrates the distributed application, manages service dependencies
2. **PrivateSocial.ServiceDefaults** - Shared configurations (OpenTelemetry, health checks, resilience)
3. **PrivateSocial.ApiService** - Backend API service with Entity Framework Core
4. **PrivateSocial.Web.React** - React + TypeScript frontend with Vite
5. **PrivateSocial.Tests** - Integration tests using Aspire testing framework

### Key Architectural Patterns
- **Service Discovery**: Services communicate via Aspire's built-in service discovery (e.g., "https+http://apiservice")
- **Health Checks**: All services expose `/health` and `/alive` endpoints
- **Resilience**: HTTP clients configured with retry policies and circuit breakers
- **Caching**: Redis output caching implemented in the Web project
- **Observability**: OpenTelemetry for distributed tracing, metrics, and logging

### Inter-Service Communication
- Web frontend calls API service via nginx proxy (/api/* routes)
- Services registered in AppHost:
  - API service: `.AddProject<T>()` pattern
  - Redis: `.AddRedis()` as container
  - SQL Server: `.AddSqlServer()` as container with database
  - Web frontend: `.AddDockerfile()` for containerized React app
- Service discovery handled by Aspire's environment variables

### Frontend Architecture
- React + TypeScript with Vite bundler
- React Router for navigation with protected routes
- Component structure: App.tsx → AuthProvider → MainLayout → Pages
- Pages located in src/pages/
- Shared components in src/components/
- Services for API communication in src/services/
- Authentication context in src/contexts/AuthContext.tsx
- Styles organized in src/styles/
- JWT token stored in localStorage
- Axios for HTTP requests with auth interceptor
- **Containerized with Docker**:
  - Multi-stage build (Node.js for building, nginx for serving)
  - Nginx configured to proxy /api/* requests to backend service
  - Dynamic service discovery via Aspire environment variables
  - Production-optimized with caching and compression

### API Architecture
- Controller-based architecture with BaseApiController
- JWT Bearer authentication configured
- OpenAPI/Swagger enabled in development with XML documentation
- Problem Details for standardized error responses
- Controllers in Controllers/ folder
  - AuthController - Registration, login, and user info
  - UsersController - User CRUD operations (protected)
  - PostsController - Social posts CRUD operations (protected)
- Models in Models/ folder
- Services in Services/ folder
  - IAuthService/AuthService - Authentication logic
- Data layer with Entity Framework Core:
  - Data/ApplicationDbContext.cs - Main DbContext
  - Data/Entities/ - Entity models (User, Post)
  - Data/Configurations/ - EF Core entity configurations

## Development Guidelines

### Adding New Services
1. Create new project in solution
2. Reference PrivateSocial.ServiceDefaults
3. Register in AppHost.cs using `builder.AddProject<ProjectName>()`
4. Configure service discovery if inter-service communication needed

### Adding New Pages
1. Create .tsx file in PrivateSocial.Web.React/src/pages/
2. Add route in App.tsx using React Router
3. Update NavMenu.tsx if navigation link needed
4. Use React hooks and context for state management

### Adding New API Controllers
1. Create controller in PrivateSocial.ApiService/Controllers/
2. Inherit from BaseApiController for common functionality
3. Add XML documentation for OpenAPI
4. Use attribute routing with [Route("your-route")]
5. Add [ProducesResponseType] attributes for OpenAPI documentation
6. Inject ApplicationDbContext for database access

### Database Development
1. Add new entities in Data/Entities/
2. Create configurations in Data/Configurations/
3. Add DbSet properties to ApplicationDbContext
4. Database is auto-created on startup in development
5. Use async methods for all database operations

### Testing Approach
- Use Aspire testing framework for integration tests
- Test distributed scenarios with `DistributedApplicationTestingBuilder`
- HttpClient available for API testing in test context
- **Test Structure**:
  - Controllers/ - Controller tests with mocked dependencies
    - AuthControllerTests.cs - Tests for authentication endpoints
    - UsersControllerTests.cs - Tests for user management endpoints
    - PostsControllerTests.cs - Tests for social posts endpoints
    - ControllerTestBase.cs - Base class with common test setup
  - Services/ - Service tests with in-memory database
    - AuthServiceTests.cs - Tests for authentication service logic
  - Helpers/ - Test utilities
    - TestDbContextFactory.cs - Creates in-memory database contexts
    - TestConfigurationBuilder.cs - Provides test configuration
    - TestDataBuilder.cs - Creates test data objects
- **Testing Tools**:
  - xUnit v3 - Test framework
  - FluentAssertions - Readable assertions
  - Moq - Mocking framework
  - Entity Framework InMemory - In-memory database for tests

## Docker Configuration

### React Frontend Container
The React application is containerized for production deployment:

**Dockerfile Features:**
- Multi-stage build for optimized image size
- Node.js Alpine for building, nginx Alpine for serving
- Static assets served by nginx with caching headers
- API proxy configuration for backend communication

**Key Files:**
- `PrivateSocial.Web.React/Dockerfile` - Multi-stage Docker build
- `PrivateSocial.Web.React/nginx.conf` - Nginx configuration with API proxy
- `PrivateSocial.Web.React/docker-entrypoint.sh` - Dynamic service discovery setup
- `PrivateSocial.Web.React/.dockerignore` - Excludes unnecessary files

**API Proxy Configuration:**
- All `/api/*` requests are proxied to the backend service
- Service URL is dynamically configured via Aspire environment variables
- Proper headers for WebSocket support and forwarding client information