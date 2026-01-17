# Silverbridge Harps GFC Website

A comprehensive club management system for Silverbridge Harps Gaelic Football Club (GFC). This modular monolith application provides event management, ticketing, attendance tracking, and user management capabilities.

## 🏗️ Architecture

This project follows **Clean Architecture** principles organized as a **Modular Monolith**, where each business module is self-contained with its own domain, application, infrastructure, and presentation layers.

### Project Structure

```
src/
├── Aspire/                          # .NET Aspire orchestration
│   ├── SilverbridgeWeb.AppHost/     # Application host & service composition
│   └── SilverbridgeWeb.ServiceDefaults/  # Shared service defaults
├── Backend/
│   ├── API/                         # Main API entry point
│   │   └── SilverbridgeWeb.Api/
│   ├── Common/                      # Shared cross-cutting concerns
│   │   ├── Application/             # CQRS, behaviors, event bus
│   │   ├── Domain/                  # Domain primitives
│   │   ├── Infrastructure/          # Database, caching, outbox/inbox patterns
│   │   └── Presentation/            # Common endpoints & results
│   └── Modules/                     # Business modules
│       ├── Attendance/              # Attendance tracking
│       ├── Events/                  # Event management
│       ├── Ticketing/               # Ticket sales & orders
│       └── Users/                   # User management
└── Frontend/
    └── SilverbridgeWeb.WebUI/       # Blazor Server application (MudBlazor)

test/
└── SilverbridgeWeb.ArchitectureTests/  # Architecture compliance tests
```

Each module follows a consistent structure:
- **Domain**: Entities, value objects, domain events
- **Application**: Commands, queries, handlers (CQRS with MediatR)
- **Infrastructure**: Data persistence, external service integrations
- **Presentation**: API endpoints, DTOs
- **IntegrationEvents**: Cross-module event contracts

## 🚀 Tech Stack

- **.NET 10.0**
- **.NET Aspire** - Application orchestration and service discovery
- **ASP.NET Core Web API** - RESTful API
- **Blazor Server** - Interactive web UI with MudBlazor components
- **PostgreSQL** - Primary database (via Npgsql)
- **Redis** - Distributed caching and session storage
- **Keycloak** - Identity and access management
- **Entity Framework Core** - ORM with PostgreSQL
- **MassTransit** - Message bus for async communication
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Request validation
- **Scalar** - Interactive API documentation
- **OpenTelemetry** - Observability and distributed tracing

## 📋 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for local development with Aspire)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/) (optional, for Azure deployment)

## 🛠️ Getting Started

### Local Development with .NET Aspire

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SilverbridgeWeb
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the AppHost project**
   ```bash
   dotnet run --project src/Aspire/SilverbridgeWeb.AppHost
   ```
   
   This will:
   - Start PostgreSQL with pgAdmin (port 5050)
   - Start Redis
   - Start Keycloak (port 8085)
   - Start the API service
   - Start the WebUI application
   - Open the Aspire dashboard in your browser

4. **Access the services**
   - **Aspire Dashboard**: http://localhost:15000
   - **WebUI**: http://localhost:5000 (HTTP) or https://localhost:5001 (HTTPS)
   - **API**: http://localhost:5002 or https://localhost:5003
   - **Scalar API Docs**: http://localhost:5002/scalar (development only)
   - **pgAdmin**: http://localhost:5050

### Development Notes

- Database migrations are automatically applied in development mode
- Keycloak realm configuration is imported from `src/Aspire/SilverbridgeWeb.AppHost/Realms/`
- Default Keycloak admin password: `admin` (configurable via parameter)
- Service-to-service communication uses service discovery provided by Aspire

## 🏃 Running Tests

Run architecture tests to verify compliance with architectural constraints:

```bash
dotnet test test/SilverbridgeWeb.ArchitectureTests
```

## ☁️ Azure Deployment

This project is configured for deployment to Azure Container Apps using Azure Developer CLI (`azd`).

### Initial Setup

1. **Login to Azure**
   ```bash
   azd auth login
   ```

2. **Provision and deploy**
   ```bash
   azd up
   ```
   
   This command will:
   - Create the Azure infrastructure (Container Apps, PostgreSQL, Redis, Keycloak)
   - Build and deploy the containerized applications
   - Configure all service connections and environment variables

3. **Configure CI/CD** (optional)
   ```bash
   azd pipeline config
   ```

### Infrastructure

The infrastructure is defined in `azure.yaml` and uses .NET Aspire's Azure resource provisioning. The generated infrastructure includes:

- **Azure Container Apps Environment** - Hosts the API and WebUI
- **Azure PostgreSQL Flexible Server** - Database for application and Keycloak
- **Azure Redis Cache** - Distributed caching
- **Keycloak Container** - Authentication and authorization

For more details on infrastructure management, see [next-steps.md](next-steps.md).

## 📦 Modules

### Events Module
Manages club events, including event creation, scheduling, and event details.

### Ticketing Module
Handles ticket sales for events, including:
- Order creation and management
- Ticket type configuration
- Order fulfillment and ticket issuance

### Attendance Module
Tracks attendance at events, allowing the club to monitor participation and engagement.

### Users Module
Manages user accounts and profiles, integrated with Keycloak for authentication and authorization.

## 🔐 Authentication & Authorization

The application uses **Keycloak** for identity and access management:

- **Public Client**: `silverbridgeweb-webui` (for Blazor frontend)
- **Confidential Client**: `silverbridge-confidential-client` (for API-to-API communication)
- **Realm**: `silverbridge`

Users authenticate via OpenID Connect, and the frontend uses cookie-based authentication with token refresh.

## 📝 API Documentation

When running in development mode, interactive API documentation is available at `/scalar`. The API uses OpenAPI 3.0 specification.

## 🧪 Code Quality

- **Nullable Reference Types**: Enabled project-wide
- **Code Analysis**: Latest rules with warnings as errors
- **SonarAnalyzer**: Active static code analysis
- **Architecture Tests**: Automated compliance verification

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Follow the existing architecture patterns
2. Ensure all tests pass
3. Maintain code style consistency (enforced via .editorconfig)
4. Add appropriate documentation for new features

## 📚 Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Azure Developer CLI Documentation](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
