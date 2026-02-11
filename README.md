# PrivateSocial

[![CI/CD Pipeline](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/ci.yml)
[![E2E Tests](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/e2e-tests.yml/badge.svg?branch=main)](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/e2e-tests.yml)
[![Frontend Validation](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/frontend-validation.yml/badge.svg?branch=main)](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/frontend-validation.yml)
[![Backend Validation](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/backend-validation.yml/badge.svg?branch=main)](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/backend-validation.yml)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Node](https://img.shields.io/badge/Node.js-20.x-339933?logo=node.js&logoColor=white)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-5.7-3178C6?logo=typescript&logoColor=white)
![Tests](https://img.shields.io/badge/Tests-119%2B-success)
![License](https://img.shields.io/badge/License-MIT-blue)

A modern distributed social platform built with .NET Aspire, showcasing cloud-native patterns and best practices.

> **Note:** This project is under active development. Core features are still being built out and deployment pipelines have not yet been implemented. Contributions and feedback are welcome!

## Overview

PrivateSocial is a full-stack social networking application demonstrating:
- Microservices architecture with .NET Aspire orchestration
- React + TypeScript frontend with Docker containerization
- ASP.NET Core Web API with JWT authentication
- SQL Server database with Entity Framework Core
- Asynchronous messaging with RabbitMQ and MassTransit
- Event-driven workflows with saga state machines
- Redis caching for performance optimization
- Comprehensive test coverage with xUnit v3
- Production-ready nginx configuration

## Technology Stack

### Backend
- **.NET 10.0** - Backend framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for SQL Server
- **MassTransit 8.x** - Messaging abstraction with consumers and sagas
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
- **RabbitMQ** - Message broker with management UI
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
   - **RabbitMQ Management**: Check dashboard for RabbitMQ management URL

## Architecture

### Services

1. **PrivateSocial.AppHost** - Orchestrates all services (API, Worker, RabbitMQ, SQL Server, Redis)
2. **PrivateSocial.ApiService** - RESTful API backend, publishes domain events
3. **PrivateSocial.Worker** - Background service for message consumers and saga orchestration
4. **PrivateSocial.Contracts** - Shared message contracts (events and commands)
5. **PrivateSocial.Web.React** - Containerized React frontend
6. **PrivateSocial.ServiceDefaults** - Shared service configurations
7. **PrivateSocial.Tests** - Comprehensive test suite

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
- **JWT Authentication**: Secure token-based authentication with HTTPS enforcement
- **User Registration**: Create accounts with email validation and strong password requirements
- **Strong Passwords**: 12+ character minimum with uppercase, lowercase, number, and special character
- **User Login**: Authenticate with username/password
- **Protected Routes**: Secure pages requiring authentication
- **Role-based Access**: Different permission levels

### Social Features
- **User Profiles**: Customizable user profiles with bio and avatar
- **Posts**: Create, read, update, and delete social posts
- **User Management**: Admin capabilities for user management

### Messaging & Event-Driven Architecture
- **RabbitMQ**: Durable message broker with management UI, persistent queues via data volumes
- **MassTransit 8.x**: Messaging abstraction for publish/subscribe and send/receive patterns
- **Domain Events**: `UserRegistered`, `PostCreated`, `PostUpdated`, `PostDeleted` published after database operations
- **Commands**: `SendWelcomeEmail`, `ModeratePostContent`, `CreateDefaultProfile` for point-to-point processing
- **User Onboarding Saga**: State machine orchestrating multi-step onboarding (welcome email + default profile), handles out-of-order completion
- **Fan-out Events**: Multiple consumers independently process the same event (e.g., logging + saga both handle `UserRegistered`)
- **Background Worker**: Dedicated worker service for asynchronous processing, decoupled from the API

### Technical Features
- **API Documentation**: Interactive Swagger/OpenAPI documentation
- **Health Checks**: Service health monitoring endpoints
- **Distributed Tracing**: OpenTelemetry integration (includes MassTransit traces)
- **Error Handling**: Consistent error responses with Problem Details
- **Caching**: Redis caching for improved performance
- **Responsive Design**: Mobile-friendly Tailwind CSS UI

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration (requires strong password: 12+ chars with uppercase, lowercase, number, special char)
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

![CI/CD Pipeline](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/ci.yml/badge.svg?branch=main)

The project includes comprehensive test coverage with **119+ tests** across backend and frontend, with automated testing on every push to main:

### Backend Tests (46 tests)
- **Controller Tests**: Testing API endpoints with mocked dependencies
- **Service Tests**: Testing business logic with in-memory database
- **Integration Tests**: Testing distributed scenarios

Run backend tests:
```bash
dotnet test
```

### Frontend Tests (73 tests)
- **Component Tests**: Testing React components with React Testing Library
- **Service Tests**: Testing API service layer
- **Hook Tests**: Testing custom React hooks
- **Page Tests**: Testing full page components with user interactions

Run frontend tests:
```bash
cd PrivateSocial.Web.React
npm test
```

### Backend Test Structure
```
PrivateSocial.Tests/
├── Controllers/
│   ├── AuthControllerTests.cs
│   ├── UsersControllerTests.cs
│   ├── PostsControllerTests.cs
│   └── ControllerTestBase.cs
├── Services/
│   ├── AuthServiceTests.cs
│   └── PostServiceIntegrationTests.cs
└── Helpers/
    ├── TestDbContextFactory.cs
    ├── TestConfigurationBuilder.cs
    └── TestDataBuilder.cs
```

### Frontend Test Structure
```
PrivateSocial.Web.React/src/
├── components/
│   ├── CreatePostForm.test.tsx
│   ├── LoadingSpinner.test.tsx
│   ├── PostCard.test.tsx
│   └── ProtectedRoute.test.tsx
├── contexts/
│   └── AuthContext.test.tsx
├── pages/
│   ├── Login.test.tsx
│   ├── Posts.test.tsx
│   └── Register.test.tsx
└── services/
    └── postsService.test.ts
```

## Configuration

The application uses Aspire's configuration system with automatic service discovery. No manual configuration is required for inter-service communication.

### Environment Variables
- Automatically configured by Aspire
- Service discovery handled through environment variables
- Connection strings managed by Aspire

## Continuous Integration

### Status Badges

The badges at the top of this README show real-time status of the main branch:

- **CI/CD Pipeline** [![CI/CD](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/ci.yml) - Full build, test, and deployment pipeline
- **E2E Tests** [![E2E Tests](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/e2e-tests.yml/badge.svg?branch=main)](https://github.com/JimboSchneider/PrivateSocial/actions/workflows/e2e-tests.yml) - End-to-end integration tests with real services
- **Frontend Validation** - Linting and unit testing for React code
- **Backend Validation** - Build and unit testing for .NET code

Click any badge to view detailed test results and logs.

### Automated Workflows

The project includes automated workflows that run on every push to main:

- **Main Branch Test Report**: Generates comprehensive test summary with coverage
- **Claude Code Review**: AI-powered code review on pull requests
- **CI/CD Pipeline**: Build, test, and artifact publication
- **Frontend Validation**: Linting, type checking, and unit tests
- **Backend Validation**: Build, unit tests, and code quality checks
- **E2E Testing**: Full integration tests with SQL Server and Redis
- **PR Validation**: Comprehensive validation before merge approval

### Test Results

After each push to main, you can view:
- **Test Summary**: Check the Actions tab for detailed test reports
- **Coverage Reports**: Download coverage artifacts from workflow runs
- **E2E Test Videos**: Playwright records videos of E2E test failures
- **Test Artifacts**: All test results retained for 90 days

## Security

### Authentication & Passwords
- **JWT Authentication**: Secure token-based authentication with HTTPS enforcement in production
- **Strong Password Requirements**: Minimum 12 characters with uppercase, lowercase, number, and special character
- **Password Hashing**: BCrypt hashing with salt for secure password storage
- **Environment-Based Security**: JWT HTTPS validation enabled in production, disabled in development

### Infrastructure Security
- **HTTPS Enforcement**: Required for JWT tokens in production environments
- **Security Headers**: Configured in nginx (X-Frame-Options, X-Content-Type-Options, etc.)
- **Input Validation**: Comprehensive validation with detailed error messages
- **SQL Injection Prevention**: Entity Framework Core parameterized queries
- **Dependency Security**: Regular updates to address vulnerabilities (0 known vulnerabilities)

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
- Write unit tests for new features (maintain test coverage)
- Update API documentation for new endpoints
- Ensure TypeScript types are properly defined
- Run linting before committing frontend changes
- Keep dependencies updated to address security vulnerabilities
- Follow security best practices (input validation, HTTPS, strong passwords)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with .NET Aspire for cloud-native orchestration
- Uses best practices for distributed systems
- Implements modern authentication and authorization patterns