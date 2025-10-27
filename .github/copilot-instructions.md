# Silverbridge Web - AI Coding Instructions

## Architecture Overview

This is a **modular monolith** built with .NET 9 and .NET Aspire orchestration. The backend is organized into three business modules (`Events`, `Ticketing`, `Users`), each isolated with its own database schema, domain logic, and presentation layer. Modules communicate via **integration events** (MassTransit), while internal domain events use MediatR.

### Key Structural Patterns

**Module Structure** (e.g., `src/Backend/Modules/Events/`):

- `Domain/` - Entities, domain events, repository interfaces
- `Application/` - MediatR commands/queries, validators, handlers
- `Infrastructure/` - EF Core DbContext, repositories, migrations (each module has its own PostgreSQL schema like `events`, `ticketing`, `users`)
- `Presentation/` - Minimal API endpoints via `IEndpoint` pattern
- `IntegrationEvents/` - Shared cross-module event contracts

**Common Layers** (`src/Backend/Common/`):

- `Common.Domain` - `Result<T>` monad, `Error` types, `IDomainEvent`, `Entity` base class
- `Common.Application` - MediatR pipeline behaviors (validation, logging, exception handling), `ICommand`/`IQuery` interfaces
- `Common.Infrastructure` - Shared services (caching, event bus, authentication, DB connection factory)
- `Common.Presentation` - `IEndpoint` registration, `ApiResults` helpers for Result→HTTP mapping

**Frontend**: Blazor WebAssembly (`src/Frontend/SilverbridgeWeb.WebUI`) using MudBlazor components.

## Critical Developer Workflows

### Running the Application

```powershell
# Start Aspire orchestration (API + Frontend + PostgreSQL + Redis)
dotnet run --project src/Aspire/SilverbridgeWeb.AppHost
```

The AppHost (`src/Aspire/SilverbridgeWeb.AppHost/Program.cs`) defines service dependencies and health checks.

### Database Migrations

Each module manages its own migrations. To add a migration for the Events module:

```powershell
# From repository root
dotnet ef migrations add <MigrationName> `
  --project src/Backend/Modules/Events/SilverbridgeWeb.Modules.Events.Infrastructure `
  --startup-project src/Backend/API/SilverbridgeWeb.Api `
  --context EventsDbContext
```

Migrations auto-apply in development via `ApplyMigrations()` in `Program.cs`.

### Module Configuration

Each module uses dedicated JSON config files (`modules.{moduleName}.json`, `modules.{moduleName}.Development.json`) registered in `Program.cs` via `AddModuleConfiguration()`.

## Code Conventions

### Result Pattern

**Always** return `Result` or `Result<T>` from application layer handlers. Never throw exceptions for expected failures:

```csharp
// Good
return Result.Failure(EventErrors.NotFound(eventId));

// Bad
throw new NotFoundException($"Event {eventId} not found");
```

### Endpoint Registration

Endpoints implement `IEndpoint` and are auto-discovered:

```csharp
internal sealed class GetCategories : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories", async (ISender sender) => { /* ... */ })
           .RequireAuthorization()
           .WithTags(Tags.Categories);
    }
}
```

Call `app.MapEndpoints()` in `Program.cs` **after** `app.UseAuthentication()/UseAuthorization()`.

### Validation

Create FluentValidation validators alongside commands. They run automatically via `ValidationPipelineBehavior<,>`:

```csharp
internal sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty();
        // ...
    }
}
```

### Domain Events vs Integration Events

- **Domain Events**: In-process events within a module (e.g., `UserRegisteredDomainEvent`). Raised via `Entity.Raise()`, published automatically after `SaveChangesAsync()` by `PublishDomainEventsInterceptor`.
- **Integration Events**: Cross-module async events (e.g., `UserRegisteredIntegrationEvent`). Published via `IEventBus.PublishAsync()`, consumed by MassTransit consumers.

### Naming Conventions

- Database tables/columns: `snake_case` (via EFCore.NamingConventions)
- C# code: PascalCase/camelCase per standard conventions
- Module schemas: lowercase (e.g., `events`, `ticketing`, `users`)

## Dependency Management

Uses **Central Package Management** (`Directory.Packages.props`). Add package versions there, not in individual `.csproj` files.

## Key Files to Reference

- **Middleware Pipeline**: `src/Backend/API/SilverbridgeWeb.Api/Program.cs`
- **Result Mapping**: `src/Backend/Common/SilverbridgeWeb.Common.Presentation/ApiResults/ApiResults.cs`
- **MediatR Setup**: `src/Backend/Common/SilverbridgeWeb.Common.Application/ApplicationConfiguration.cs`
- **Module Boundaries**: Each module's `Infrastructure/{ModuleName}Module.cs` (e.g., `EventsModule.cs`)
- **Error Definitions**: Per-module `Domain/*Errors.cs` files (e.g., `EventErrors.cs`)

## Common Pitfalls

1. **Middleware Order**: Authentication/Authorization must come **before** `MapEndpoints()` or you'll get 404s instead of 401s.
2. **Module Isolation**: Never reference another module's `Domain`, `Application`, or `Infrastructure` projects. Use `IntegrationEvents` projects for cross-module contracts.
3. **DbContext Access**: Use `IUnitOfWork` abstraction in application layer, not direct `DbContext`.
4. **Result Unwrapping**: Call `result.Match()` or check `result.IsSuccess` before accessing `result.Value` to avoid exceptions.
