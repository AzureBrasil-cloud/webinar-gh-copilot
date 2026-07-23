---
type: overview
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/overview
  - area/project
  - component/domain
  - component/application
  - component/web
  - component/tests
---

# Repository Structure

Top-level layout of the BookStore .NET 10 MVC solution and how the four projects relate.

## Context

The codebase lives under `Src/` and is built as a single .NET 10 solution. The repository also contains training modules, deployment scripts, and this Obsidian vault.

## Root files and folders

| Path | Purpose |
|------|---------|
| `Src/BookStore.slnx` | Solution file referencing all projects. |
| `Src/BookStore.Domain/` | Domain layer: entities, `DbContext`, seed data, domain exception. |
| `Src/BookStore.Application/` | Application layer: services and `PagedResult<T>`. |
| `Src/BookStore.Web/` | ASP.NET Core 10 MVC web application (composition root). |
| `Src/BookStore.Tests/` | xUnit tests for the domain layer. |
| `deploy.sh` | Manual Azure App Service deployment script. |
| `README.md` | Webinar series overview and quick-start. |
| `full-doc.md` | Working source-of-truth document used to reconcile this vault. |
| `Docs/Vault/BookStore/` | This living-documentation vault. |
| `Modules/` | Per-module training content for the webinar series. |

## Project dependency graph

```
BookStore.Web
├── BookStore.Application
│   └── BookStore.Domain
└── BookStore.Domain

BookStore.Tests
└── BookStore.Domain
```

## Key files for new contributors

- Composition root: `Src/BookStore.Web/Program.cs`
- Domain context: `Src/BookStore.Domain/Data/BookStoreContext.cs`
- Seed data: `Src/BookStore.Domain/Seed/DataSeeder.cs`
- Domain exception: `Src/BookStore.Domain/Exceptions/DomainException.cs`

## Related

- [[Dependencies]]
- [[Architecture-Overview]]
- [[Operations-Overview]]
