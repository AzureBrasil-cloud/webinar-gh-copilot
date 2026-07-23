---
type: overview
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/overview
  - area/operations
---

# Operations Overview

Build, run, test, and persistence information for the BookStore solution.

## Context

The solution is a plain .NET 10 web application with no CI/CD pipeline. Local development and manual Azure deployment are the current workflows.

## Build

```bash
dotnet build Src/BookStore.slnx
```

## Run locally

```bash
dotnet run --project Src/BookStore.Web
```

Then open <http://localhost:5045> (from `Src/BookStore.Web/Properties/launchSettings.json`).

## Test

```bash
dotnet test Src/BookStore.Tests/BookStore.Tests.csproj
```

## Persistence

- EF Core In-Memory provider.
- Database name: `BookStoreDb` (configured in `Src/BookStore.Web/Program.cs`).
- Data is re-seeded on every process restart via `DataSeeder.Seed(db)`.
- No migrations and no other database provider is configured.

## Configuration

`Src/BookStore.Web/appsettings.json` contains only default ASP.NET Core logging and `AllowedHosts: "*"`. There are no connection strings, secrets, or feature flags today.

## Deployment

Production deployment is manual via `deploy.sh`. See [[Deployment-Runbook]] for the exact steps and Azure targets.

## Source

- `Src/BookStore.Web/Program.cs`
- `Src/BookStore.Web/Properties/launchSettings.json`
- `Src/BookStore.Web/appsettings.json`
- `Src/BookStore.Domain/Data/BookStoreContext.cs`
- `Src/BookStore.Domain/Seed/DataSeeder.cs`
- `README.md`

## Related

- [[Deployment-Runbook]]
- [[Testing]]
- [[Architecture-Overview]]
- [[Azure-App-Service-Docs]]
