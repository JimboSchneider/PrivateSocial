# PrivateSocial

A modern distributed social platform built with .NET Aspire, showcasing cloud-native patterns and best practices.

## Overview

PrivateSocial is a full-stack social networking application demonstrating:
- Microservices architecture with .NET Aspire orchestration
- React + TypeScript frontend with Docker containerization
- ASP.NET Core Web API with JWT authentication
- SQL Server database with Entity Framework Core
- Redis caching for performance optimization
- Comprehensive test coverage with xUnit v3
- Production-ready nginx configuration

## Technology Stack

### Backend
- **.NET 10.0** - Backend framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for SQL Server
- **JWT Authentication** - Secure token-based auth
- **xUnit v3** - Testing framework
- **FluentAssertions** - Test assertions
- **Moq** - Mocking framework

### Frontend
- **React 18** - UI framework
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool
- **React Router** - Client-side routing
- **Axios** - HTTP client
- **Tailwind CSS** - Utility-first CSS framework

### Infrastructure
- **Docker** - Container platform
- **nginx** - Web server for React app
- **SQL Server** - Primary database
- **Redis** - Caching layer
- **.NET Aspire** - Cloud-native orchestration

## Prerequisites

- .NET 10.0 SDK
- Docker Desktop
- Node.js 20+ (for local frontend development)

## Quick Start

1. Clone the repository:
```bash
git clone https://github.com/yourusername/PrivateSocial.git
cd PrivateSocial
```

2. Run the entire application stack:
```bash
dotnet run --project PrivateSocial.AppHost
```

3. Access the applications:
   - **Aspire Dashboard**: https://localhost:17253
   - **React Frontend**: Check dashboard for assigned port
   - **API Documentation**: Check dashboard for API service URL + `/swagger`

## Architecture

### Services

1. **PrivateSocial.AppHost** - Orchestrates all services
2. **PrivateSocial.ApiService** - RESTful API backend
3. **PrivateSocial.Web.React** - Containerized React frontend
4. **PrivateSocial.ServiceDefaults** - Shared service configurations
5. **PrivateSocial.Tests** - Comprehensive test suite

### Database Schema

- **Users** - User accounts with profiles
- **Posts** - Social media posts

### Frontend Container

The React application runs in a production-optimized Docker container:
- Multi-stage build with Node.js and nginx
- Automatic API proxy configuration
- Static asset caching with proper headers
- Security headers configured
- Dynamic service discovery via Aspire

## Development

### Backend Development

```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific project
dotnet run --project PrivateSocial.ApiService
```

### Frontend Development

```bash
cd PrivateSocial.Web.React

# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Run linting
npm run lint

# Run tests
npm test
```

## Features

### Authentication & Authorization
- **JWT Authentication**: Secure token-based authentication
- **User Registration**: Create accounts with email validation
- **User Login**: Authenticate with username/password
- **Protected Routes**: Secure pages requiring authentication
- **Role-based Access**: Different permission levels

### Social Features
- **User Profiles**: Customizable user profiles with bio and avatar
- **Posts**: Create, read, update, and delete social posts
- **User Management**: Admin capabilities for user management

### Technical Features
- **API Documentation**: Interactive Swagger/OpenAPI documentation
- **Health Checks**: Service health monitoring endpoints
- **Distributed Tracing**: OpenTelemetry integration
- **Error Handling**: Consistent error responses with Problem Details
- **Caching**: Redis caching for improved performance
- **Responsive Design**: Mobile-friendly Tailwind CSS UI

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/auth/me` - Get current user info (protected)

### Users
- `GET /api/users` - List all users (protected)
- `GET /api/users/{id}` - Get user by ID (protected)
- `POST /api/users` - Create new user (protected)

### Posts
- `GET /api/posts` - Get paginated posts (protected)
- `GET /api/posts/{id}` - Get post by ID (protected)
- `POST /api/posts` - Create new post (protected)
- `PUT /api/posts/{id}` - Update post (owner only)
- `DELETE /api/posts/{id}` - Delete post (owner only)

## Testing

The project includes comprehensive test coverage:

### Backend Tests (44+ tests)
- **Controller Tests**: Testing API endpoints with mocked dependencies
- **Service Tests**: Testing business logic with in-memory database
- **Integration Tests**: Testing distributed scenarios

Run tests with:
```bash
dotnet test
```

### Test Structure
```
PrivateSocial.Tests/
├── Controllers/
│   ├── AuthControllerTests.cs
│   ├── UsersControllerTests.cs
│   ├── PostsControllerTests.cs
│   └── ControllerTestBase.cs
├── Services/
│   └── AuthServiceTests.cs
└── Helpers/
    ├── TestDbContextFactory.cs
    ├── TestConfigurationBuilder.cs
    └── TestDataBuilder.cs
```

## Configuration

The application uses Aspire's configuration system with automatic service discovery. No manual configuration is required for inter-service communication.

### Environment Variables
- Automatically configured by Aspire
- Service discovery handled through environment variables
- Connection strings managed by Aspire

## Security

- JWT tokens for authentication
- Password hashing with BCrypt
- HTTPS enforcement in production
- Security headers configured in nginx
- Input validation and sanitization
- SQL injection prevention with Entity Framework Core

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Write tests for your changes
4. Ensure all tests pass (`dotnet test`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## Development Guidelines

- Follow C# coding conventions
- Write unit tests for new features
- Update API documentation for new endpoints
- Ensure TypeScript types are properly defined
- Run linting before committing frontend changes

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with .NET Aspire for cloud-native orchestration
- Uses best practices for distributed systems
- Implements modern authentication and authorization patterns