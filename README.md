# PrivateSocial

A modern distributed social platform built with .NET Aspire, showcasing cloud-native patterns and best practices.

## Overview

PrivateSocial is a full-stack application demonstrating:
- Microservices architecture with .NET Aspire orchestration
- React + TypeScript frontend with Docker containerization
- ASP.NET Core Web API with JWT authentication
- SQL Server database with Entity Framework Core
- Redis caching
- Production-ready nginx configuration

## Technology Stack

- **.NET 9.0** - Backend framework
- **React + TypeScript** - Frontend framework
- **Vite** - Frontend build tool
- **Docker** - Container platform
- **nginx** - Web server for React app
- **SQL Server** - Primary database
- **Redis** - Caching layer
- **.NET Aspire** - Cloud-native orchestration

## Prerequisites

- .NET 9.0 SDK
- Docker Desktop
- Node.js 20+ (for local development)

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
4. **SQL Server** - Database container
5. **Redis** - Cache container

### Frontend Container

The React application runs in a production-optimized Docker container:
- Multi-stage build with Node.js and nginx
- Automatic API proxy configuration
- Static asset caching
- Security headers

## Development

### Backend Development

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

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
```

## Features

- **User Authentication**: JWT-based authentication system
- **User Registration**: Create new accounts with email verification
- **Protected Routes**: Secure pages requiring authentication
- **Responsive Design**: Mobile-friendly UI
- **API Documentation**: OpenAPI/Swagger integration
- **Health Checks**: Service health monitoring
- **Distributed Tracing**: OpenTelemetry integration

## API Endpoints

- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/auth/me` - Get current user info
- `GET /api/users` - List users (protected)
- `GET /api/weatherforecast` - Sample endpoint

## Configuration

The application uses Aspire's configuration system with automatic service discovery. No manual configuration is required for inter-service communication.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.