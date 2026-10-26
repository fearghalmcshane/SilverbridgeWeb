# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Website for Silverbridge Harps GFC (GAA club): .NET 10 modular monolith API + Blazor Interactive Server WebUI (MudBlazor), PostgreSQL, Redis, Azure Blob storage, Clerk auth, orchestrated and deployed (Azure Container Apps) with .NET Aspire. `README.md` is the authoritative long-form reference, including the detailed Team Tally webhook / Matches / News match-report contract.

## Commands

```bash
dotnet build                                   # warnings are errors (see below)
dotnet run --project src/Aspire/SilverbridgeWeb.AppHost   # full local stack (needs Docker); pgAdmin on :5050
dotnet test --filter-not-namespace "SilverbridgeWeb.WebUI.BrowserTests*" --ignore-exit-code 8   # what CI runs first
dotnet test test/SilverbridgeWeb.ArchitectureTests        # global layer/module-isolation rules
dotnet test src/Backend/Modules/<Module>/test/SilverbridgeWeb.Modules.<Module>.<Unit|Integration|Architecture>Tests
dotnet test <project> --filter-class "*SomeTestClass"   # single class (--filter-method for a test)
```

- `global.json` opts `dotnet test` into Microsoft Testing Platform mode (required by xunit.v3 4.x on the .NET 10 SDK), so use xUnit's MTP filter options (`--filter-class`, `--filter-not-namespace`, ...) rather than VSTest `--filter` expressions. Exit code 8 means a project ran zero tests.

- Integration tests (Matches, News) use Testcontainers PostgreSQL — Docker must be running.
- Browser tests (`src/Frontend/test/SilverbridgeWeb.WebUI.BrowserTests`) use Playwright; install Chromium once via `bin/<Config>/net10.0/playwright.ps1 install chromium` after building.
- Component tests use bUnit (`src/Frontend/test/SilverbridgeWeb.WebUI.ComponentTests`).
- The Aspire CLI is also available (`aspire start`, `aspire describe`, `aspire logs <resource>`, `aspire otel logs`); `.mcp.json` registers the Aspire MCP server and `.github/skills/aspire/SKILL.md` documents the workflow.
- Local secrets are AppHost parameters (user secrets as `Parameters:<name>`), e.g. `clerk-authority`, `clerk-api-key`, `clerk-webhook-signing-secret`, `team-tally-webhook-signing-secret`, `clerk-client-id/secret`, `foireann-primary/secondary-api-key`. Never put secrets in `modules.*.json`.

## Architecture

### Runtime topology (AppHost `src/Aspire/SilverbridgeWeb.AppHost/Program.cs`)
- `silverbridgeweb-migrator` (`src/Backend/SilverbridgeWeb.Migrator`) runs first: applies every module's EF migrations (one `MigrateAsync<TDbContext>` per module in `MigratorWorker`) then does a Clerk user backfill, and exits. The API `WaitForCompletion`s it — **the API does not migrate on startup**.
- `silverbridgeweb-api` is private; `silverbridgeweb-webui` is the only external endpoint. The WebUI calls the API through typed clients in `WebUI/Services/ApiClients` using service discovery (`https+http://silverbridgeweb-api/`), and relays external webhooks (e.g. Team Tally) to the API (`WebUI/Services/Webhooks`).
- Auth: API validates Clerk JWTs; WebUI uses Clerk OIDC with cookie session.

### Modules (`src/Backend/Modules/<Name>/`)
Attendance, Bookings, Events, Foireann, Matches, News, Ticketing, Users. Each is Clean Architecture: `Domain` ← `Application` ← `Infrastructure` / `Presentation`, plus `IntegrationEvents` (the only project other modules may reference). Foireann is an external-API integration with no Domain/database. Layer and isolation rules are enforced by NetArchTest (global + per-module ArchitectureTests).

- **CQRS**: custom `ISender`/`MessageDispatcher` in `Common.Application` (not MediatR). Commands/queries + handlers + FluentValidation validators live together per feature folder in `Application`. Everything returns `Result`/`Result<T>` (`Common.Domain`); endpoints end with `result.Match(Results.Ok, ApiResults.Problem)`.
- **Endpoints**: one `internal sealed` class per endpoint implementing `IEndpoint` in `Presentation/<Feature>/`, discovered and mapped by `app.MapEndpoints()`.
- **Persistence**: one `<Module>DbContext` per module with its own PostgreSQL schema (`Schemas.cs`, `HasDefaultSchema`), all in the single `silverbridgeDb` database. Migrations live in `Infrastructure/Database/Migrations/`.
- **Cross-module communication** is only via integration events: aggregate raises a domain event → module outbox (`outbox_messages`) → background job → domain event handler publishes integration event on `IEventBus` → consuming module's inbox (`inbox_messages`) → idempotent `IIntegrationEventHandler<T>`. Delivery is at-least-once; consumers must be idempotent. Each module has its own `Outbox/` and `Inbox/` in Infrastructure (Matches has extended retry/backoff settings in `modules.matches.json`).
- **Config**: each module has `modules.<name>.json` / `modules.<name>.Development.json` in the API project, loaded by `AddModuleConfiguration`.

### Adding a new module touches several composition roots
`SilverbridgeWeb.Api/Program.cs` (`AddApplication` assembly list, `AddModuleConfiguration` list, `Add<Name>Module`), the module's `modules.<name>*.json` files, `MigratorWorker` (if it has a DbContext) plus the Migrator's registrations, `SilverbridgeWeb.slnx`, the global architecture tests, and a WebUI API client if the UI needs it.

### Frontend (`src/Frontend/SilverbridgeWeb.WebUI`)
Custom MudBlazor theme (blue `#263C9F`, yellow `#FCCF02`) in `Theme/`; light/dark follows system preference with a persisted override. New pages should use the shared `PageHeader`, `SectionHeader`, and `StatePanel` components (`Components/Shared/`), have exactly one `h1`, keep visible keyboard focus, and work at 320px wide without horizontal scroll. Page-specific styles go in scoped `.razor.css`; globals in `wwwroot/css/app.css`.

## Conventions

- `TreatWarningsAsErrors`, `AnalysisMode=All`, SonarAnalyzer and `EnforceCodeStyleInBuild` apply to every project (`Directory.Build.props`), so analyzer/style issues break the build. `.editorconfig` makes `this.` qualification and many expression preferences errors; CRLF, 4-space indent.
- Central Package Management: versions only in `Directory.Packages.props`; never put `Version=` on a `PackageReference`.
- Entity IDs are ULIDs stored as `Guid`: `Ulid.NewUlid().ToGuid()`.
- Namespaces mirror folders: `SilverbridgeWeb.Modules.<Module>.<Layer>.<Feature>`.
- Naming: `{Verb}{Noun}Command/Query`, `...CommandHandler`, `{Entity}{Action}DomainEvent`, `{Entity}{Action}IntegrationEvent`.
- `var` is an error except where the type is apparent (`.editorconfig`); use explicit types. CA2007 (ConfigureAwait) is disabled.
- CI (`.github/workflows/ci.yml`) runs on PRs to `main`: Release build, tests excluding browser tests, then Playwright browser tests. Pushes to `main` deploy to production via `aspire deploy` (`deploy-production.yml`).
