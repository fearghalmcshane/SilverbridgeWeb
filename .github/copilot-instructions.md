# Silverbridge Harps GFC Website - Development Guide

## Project Overview

ASP.NET Core modular monolith application using .NET 10, deployed to Azure Container Apps via .NET Aspire. The backend API uses a vertical slice architecture within modules, with a Blazor WebUI frontend.

## Build, Test, and Lint Commands

```bash
# Build entire solution
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Run all tests
dotnet test

# Run tests in specific module
dotnet test src/Backend/Modules/Users/test/SilverbridgeWeb.Modules.Users.ArchitectureTests

# Run architecture tests only
dotnet test test/SilverbridgeWeb.ArchitectureTests

# Run with Aspire (local development)
dotnet run --project src/Aspire/SilverbridgeWeb.AppHost

# Restore dependencies
dotnet restore
```

## Architecture

### Modular Monolith Structure

The application is organized into independent **modules**, each with its own bounded context:

- **Users** - User management and authentication
- **Events** - Event creation and management
- **Ticketing** - Ticket sales and management
- **Attendance** - Attendance tracking
- **Foireann** - Team/staff management

### Module Layer Structure

Each module follows **Clean Architecture** with strict layer dependencies enforced by architecture tests:

```
Module.Domain/          # Entities, value objects, domain events, repository interfaces
  └─ {Feature}/

Module.Application/     # Use cases, commands, queries, handlers
  └─ {Feature}/
      ├─ {Feature}Command.cs
      ├─ {Feature}CommandHandler.cs
      └─ {Feature}CommandValidator.cs

Module.Infrastructure/  # EF Core DbContext, repositories, external integrations
  ├─ Database/
  │   ├─ {Module}DbContext.cs
  │   ├─ Schemas.cs
  │   └─ Migrations/
  ├─ Outbox/
  └─ Inbox/

Module.Presentation/    # Minimal API endpoints
  └─ {Feature}/
      └─ {Endpoint}.cs (implements IEndpoint)

Module.IntegrationEvents/  # Events published to other modules

Module.ArchitectureTests/  # NetArchTest rules
```

**Layer dependency rules** (enforced by tests):
- Domain → None
- Application → Domain
- Infrastructure → Application, Domain
- Presentation → Application, Domain

### CQRS Pattern

All operations use **CQRS with MediatR**:

- **Commands** modify state, return `Result<T>`
- **Queries** read state, return data or `Result<T>`
- Command/Query handlers are in the `Application` layer
- Validators use `FluentValidation`

**Naming convention**: `{Action}{Entity}Command/Query` (e.g., `RegisterUserCommand`, `GetUserQuery`)

### Endpoint Structure

Endpoints use **Minimal APIs** and implement `IEndpoint`:

```csharp
internal sealed class RegisterUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new RegisterUserCommand(...));
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Users);
    }

    internal sealed class Request { ... }
}
```

### Result Pattern

All operations return `Result` or `Result<T>` from `Common.Domain`:

```csharp
// Success
Result<Guid> success = Result.Success(userId);

// Failure
Result<Guid> failure = Result.Failure<Guid>(UserErrors.EmailNotUnique);

// Pattern matching
return result.Match(Results.Ok, ApiResults.Problem);
```

### Module Communication

Modules communicate via **integration events** using MassTransit:

1. Domain events are raised within aggregates
2. Domain event handlers publish integration events to the message bus
3. Other modules consume integration events via inbox/outbox pattern

**Outbox pattern**: Domain events → OutboxMessages → Background processing → Message bus
**Inbox pattern**: Integration events → InboxMessages → Idempotent processing

### Database Schema Isolation

Each module has its own PostgreSQL schema defined in `{Module}DbContext`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema(Schemas.Users);
}
```

Migrations are per-module in `Module.Infrastructure/Database/Migrations/`.

### Common Layer

Shared abstractions in `Common.*` projects:
- `Common.Domain` - Result, Error, Entity, DomainEvent base classes
- `Common.Application` - ICommand, IQuery, ICommandHandler, IQueryHandler, behaviors
- `Common.Infrastructure` - Authentication, authorization, caching, inbox/outbox
- `Common.Presentation` - IEndpoint, endpoint extensions

## Key Conventions

### Project References

- Modules do **NOT** reference other modules directly
- Use `IntegrationEvents` projects for cross-module communication
- All modules reference `Common.*` projects

### Identifier Strategy

Use **ULID** for entity IDs (Universally Unique Lexicographically Sortable Identifier):

```csharp
public Guid Id { get; private set; } = Ulid.NewUlid().ToGuid();
```

### Naming Conventions

- **Namespaces**: Match folder structure (`SilverbridgeWeb.Modules.{Module}.{Layer}.{Feature}`)
- **Endpoints**: Grouped by feature folder, one file per endpoint
- **Commands/Queries**: `{Verb}{Noun}Command/Query` (e.g., `UpdateUserCommand`)
- **Handlers**: `{CommandOrQuery}Handler` (e.g., `RegisterUserCommandHandler`)
- **Domain Events**: `{Entity}{Action}DomainEvent` (e.g., `UserRegisteredDomainEvent`)
- **Integration Events**: `{Entity}{Action}IntegrationEvent`

### Configuration Files

Modules use separate JSON config files:
- `modules.{module}.json` - Production settings
- `modules.{module}.Development.json` - Development overrides

Loaded via:
```csharp
builder.Configuration.AddModuleConfiguration(["events", "users", "ticketing", "attendance", "foireann"]);
```

### Code Style

Enforced via `.editorconfig`:
- **No** `this.` qualification (error level)
- Required accessibility modifiers for non-interface members
- Prefer `string` over `String`, `int` over `Int32`
- Expression-level preferences enforced as errors
- CRLF line endings, 4-space indentation

### Code Analysis

- **TreatWarningsAsErrors**: `true` (all warnings block compilation)
- **SonarAnalyzer.CSharp** included in all projects
- **AnalysisLevel**: `latest`, **AnalysisMode**: `All`

### Authentication

Uses **Keycloak** for authentication:
- AppHost configures Keycloak container with realm import
- API validates JWT tokens from Keycloak authority
- Frontend uses OpenID Connect with Keycloak

## Aspire Configuration

AppHost (`SilverbridgeWeb.AppHost`) orchestrates:
- PostgreSQL (with pgAdmin on port 5050)
- Redis
- Keycloak (port 8085 in run mode, persistent container with data volume)
- API project
- Blazor WebUI project

Service discovery and configuration automatically wired between projects.

## Testing Strategy

### Architecture Tests

Use **NetArchTest.Rules** to enforce architectural constraints:
- Layer dependencies
- Domain purity (no infrastructure references)
- Naming conventions

Run architecture tests to validate changes:
```bash
dotnet test test/SilverbridgeWeb.ArchitectureTests
```

### Test Project Structure

Each module can have:
- `{Module}.ArchitectureTests` - Layer and naming rules
- Integration tests (to be added)
- Unit tests (to be added)

## Package Management

Uses **Central Package Management** (`Directory.Packages.props`):
- All package versions defined centrally
- Projects reference packages without version attributes
- Update versions in one place for all projects

Key packages:
- **Aspire** 13.2.4
- **MediatR** 14.1.0
- **MassTransit** 9.1.0
- **FluentValidation** 12.1.1
- **EF Core** 10.0.7
- **MudBlazor** 9.4.0 (frontend)

## Development Workflow

1. **Start Aspire AppHost** for local development with all dependencies
2. **Create new features** in appropriate module following vertical slice structure
3. **Run architecture tests** to validate layer boundaries
4. **Add migrations** per-module if database changes needed
5. **Use integration events** for cross-module communication
6. **CI runs on PR** to main branch (build + test)
