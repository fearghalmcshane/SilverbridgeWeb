# Silverbridge Harps GFC - Copilot Instructions

## Project Overview

This is a **Modular Monolith** club management system built with **.NET 10** and **Clean Architecture** principles. Each business module (Events, Ticketing, Attendance, Users, Teams, News) is self-contained with its own Domain, Application, Infrastructure, and Presentation layers.

## Build & Run Commands

### Development with .NET Aspire
```bash
# Run the entire application stack (API, WebUI, PostgreSQL, Redis, Keycloak)
dotnet run --project src/Aspire/SilverbridgeWeb.AppHost

# Restore dependencies
dotnet restore

# Build solution
dotnet build
```

**Aspire Dashboard**: http://localhost:15000  
**WebUI**: http://localhost:5000  
**API**: http://localhost:5002  
**API Docs (Scalar)**: http://localhost:5002/scalar (development only)

### Testing
```bash
# Run all tests
dotnet test

# Run architecture tests only
dotnet test test/SilverbridgeWeb.ArchitectureTests

# Run tests for a specific module (if they exist)
dotnet test src/Backend/Modules/Events/test/
```

### Database Migrations
Migrations are applied automatically in development mode via Aspire. For manual migration management, navigate to the specific module's Infrastructure project.

## Architecture Patterns

### Module Structure
Each module follows this consistent structure:
- **Domain**: Entities, value objects, domain events, repository interfaces
- **Application**: CQRS commands/queries, handlers, domain event handlers
- **Infrastructure**: EF Core repositories, database configuration, outbox/inbox patterns
- **Presentation**: Minimal API endpoints implementing `IEndpoint`
- **IntegrationEvents**: Cross-module event contracts (published via outbox pattern)
- **PublicApi**: Abstractions for cross-module communication

### Module Independence
**Critical**: Modules MUST NOT reference other modules directly. Cross-module communication happens exclusively through:
- **Integration Events** (async): Published to outbox, consumed via inbox pattern
- **PublicApi interfaces** (sync): Exposed via Infrastructure registration, consumed through DI

Architecture tests enforce these boundaries. If you add dependencies between modules, tests will fail.

### CQRS Pattern
All application logic uses CQRS with MediatR:
- **Commands**: Implement `ICommand` or `ICommand<TResponse>`, handled by `ICommandHandler`
- **Queries**: Implement `IQuery<TResponse>`, handled by `IQueryHandler`
- Both return `Result<T>` from `SilverbridgeWeb.Common.Domain`

Example structure:
```
Application/
  FeatureName/
    CommandName/
      CommandNameCommand.cs
      CommandNameCommandHandler.cs
      CommandNameCommandValidator.cs  (FluentValidation)
```

### Result Pattern
All commands and queries return `Result` or `Result<T>`:
```csharp
// Success
return Result.Success();
return Result.Success(value);

// Failure
return Result.Failure(ErrorType.Specific);
```

Error types are defined in module-specific `*Errors.cs` files (e.g., `EventErrors`, `TicketingErrors`).

### API Endpoints
Endpoints use Minimal API style, implementing `IEndpoint`:
```csharp
internal sealed class GetEvent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("events/{id}", async (Guid id, ISender sender) =>
        {
            Result<EventResponse> result = await sender.Send(new GetEventQuery(id));
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.GetEvents)
        .WithTags(Tags.Events);
    }
}
```

All endpoints are auto-discovered and registered via `app.MapEndpoints()` in Program.cs.

### Domain Events vs Integration Events

**Domain Events** (internal to module):
- Inherit from `DomainEvent` (Common.Domain)
- Handled by domain event handlers within the same module
- Processed synchronously during the same transaction
- Example: `EventCreatedDomainEvent` → triggers business logic within Events module

**Integration Events** (cross-module):
- Inherit from `IntegrationEvent` (Common.Application.EventBus)
- Published to outbox table via `InsertOutboxMessagesInterceptor`
- Processed by background job (`OutboxMessageConsumer`)
- Delivered to other modules' inbox tables via MassTransit
- Each module processes its inbox with `ProcessInboxJob`
- Example: `EventPublishedIntegrationEvent` → notifies Ticketing and Attendance modules

To add an integration event:
1. Define event in `ModuleName.IntegrationEvents` project
2. Publish via domain event handler using `IEventBus.PublishAsync<T>`
3. Consuming module implements `IntegrationEventHandler<T>` in its Application layer

### Naming Conventions

**Projects**: `SilverbridgeWeb.Modules.{ModuleName}.{Layer}` (e.g., `SilverbridgeWeb.Modules.Events.Application`)

**Files**: Feature-based folders with descriptive names matching the operation (e.g., `CreateEvent/CreateEventCommand.cs`)

**Entities/Aggregates**: Singular, Pascal case (e.g., `Event`, `Order`, `Ticket`)

**Value Objects**: Descriptive names (e.g., `TicketCode`, `Money`, `EventStatus`)

**Domain Events**: `{EntityName}{Action}DomainEvent` (e.g., `EventPublishedDomainEvent`)

**Integration Events**: `{EntityName}{Action}IntegrationEvent` (e.g., `EventPublishedIntegrationEvent`)

**Commands**: `{Action}{EntityName}Command` (e.g., `CreateEventCommand`, `RescheduleEventCommand`)

**Queries**: `Get{EntityName}(s)Query` (e.g., `GetEventQuery`, `GetEventsQuery`)

**Endpoints**: Match command/query name without suffix (e.g., `GetEvent.cs` for `GetEventQuery`)

### Dependency Injection
- **Module registration**: Each module exposes `Add{ModuleName}Module(IServiceCollection, IConfiguration)` in its Infrastructure layer
- **Scoped services**: Repositories, handlers (auto-registered by MediatR/FluentValidation)
- **Singleton services**: Caching, message bus configuration

### Central Package Management
This solution uses **Directory.Packages.props** for centralized package version management. When adding NuGet packages:
1. Add `<PackageReference Include="PackageName" />` to the project file (no version)
2. Add `<PackageVersion Include="PackageName" Version="x.y.z" />` to `Directory.Packages.props`

## Key Technologies

- **Entity Framework Core**: ORM with PostgreSQL, snake_case naming via `EFCore.NamingConventions`
- **MassTransit**: Message bus for async module communication
- **Keycloak**: OAuth2/OIDC authentication (realm: `silverbridge`, clients: `silverbridgeweb-webui`, `silverbridge-confidential-client`)
- **Redis**: Distributed caching and session storage
- **MudBlazor**: Blazor Server UI components (WebUI project)
- **Scalar**: API documentation (development only)
- **Quartz.NET**: Background job scheduling (outbox/inbox processing)

## Code Quality Rules

- **Nullable Reference Types**: Enabled project-wide, all reference types must be explicitly nullable or non-nullable
- **Warnings as Errors**: Enabled in `Directory.Build.props`, all warnings must be resolved
- **SonarAnalyzer**: Active for all projects, follow recommendations
- **.editorconfig**: Enforces style rules (4-space indentation, `this.` not required, system directives first)
- **Architecture Tests**: Verify module independence and layer dependencies

## Authentication & Authorization

- **WebUI**: Cookie-based authentication with Keycloak via OpenID Connect
- **API**: JWT Bearer authentication from Keycloak
- **Permissions**: Defined in each module's `Permissions.cs` (Presentation layer)
- **Authorization**: Use `.RequireAuthorization(Permissions.SpecificPermission)` on endpoints

## Common Gotchas

1. **Module isolation**: Never add direct project references between modules (Domain, Application, Infrastructure, Presentation). Use IntegrationEvents or PublicApi.
2. **Outbox pattern**: Domain event handlers that need cross-module communication should publish integration events, not call other modules directly.
3. **Configuration**: Each module has separate `modules.{modulename}.json` files in the API project for module-specific settings.
4. **Database contexts**: Each module has its own `DbContext`. Do not share entities across contexts.
5. **ULIDs**: Primary keys use ULID (not Guid) for sortable, time-based unique identifiers.
6. **Result handling**: Always use `result.Match()` or check `result.IsSuccess` before accessing `result.Value`.

## Azure Deployment

This project uses **Azure Developer CLI (azd)** for deployment:
```bash
# Login to Azure
azd auth login

# Provision and deploy
azd up

# Deploy only (infrastructure already provisioned)
azd deploy
```

Infrastructure is generated from the Aspire AppHost. To customize:
```bash
azd infra gen  # Generates Bicep files to infra/ directory
```
