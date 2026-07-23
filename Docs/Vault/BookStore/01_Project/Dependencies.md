---
type: reference
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/reference
  - area/project
  - component/domain
  - component/application
  - component/web
  - component/tests
---

# Dependencies

NuGet package and project-reference inventory for each BookStore project.

## Context

Keeping this inventory current avoids silent drift when packages are added, removed, or upgraded.

## Project references

| Project | References |
|---------|------------|
| `BookStore.Web` | `BookStore.Application`, `BookStore.Domain` |
| `BookStore.Application` | `BookStore.Domain` |
| `BookStore.Tests` | `BookStore.Domain` |
| `BookStore.Domain` | (none) |

## NuGet packages

### BookStore.Domain

| Package | Version |
|---------|---------|
| `Microsoft.EntityFrameworkCore.InMemory` | `10.0.0` |

### BookStore.Tests

| Package | Version |
|---------|---------|
| `coverlet.collector` | `6.0.4` |
| `FluentAssertions` | `8.10.0` |
| `Microsoft.NET.Test.Sdk` | `17.14.1` |
| `xunit` | `2.9.3` |
| `xunit.runner.visualstudio` | `3.1.4` |

### BookStore.Web and BookStore.Application

No direct NuGet packages beyond the .NET 10 SDK and implicit project references.

## Source files

- `Src/BookStore.Domain/BookStore.Domain.csproj`
- `Src/BookStore.Application/BookStore.Application.csproj`
- `Src/BookStore.Web/BookStore.Web.csproj`
- `Src/BookStore.Tests/BookStore.Tests.csproj`

## Related

- [[Repository-Structure]]
- [[Dotnet-Aspnetcore-Ef-Docs]]
