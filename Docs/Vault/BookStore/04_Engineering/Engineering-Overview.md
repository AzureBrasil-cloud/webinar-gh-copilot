---
type: overview
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/overview
  - area/engineering
---

# Engineering Overview

Code-style and engineering conventions used across the BookStore solution.

## Context

These conventions keep the .NET 10 codebase consistent across Domain, Application, Web, and Tests projects.

## Code style

- **Language**: C# 12.
- **Nullable reference types**: enabled in all projects.
- **Namespaces**: file-scoped namespaces.
- **`var`**: used when the type is obvious from the right-hand side.
- **Async/await**: end-to-end for all database calls.
- **Domain method naming**: verbs such as `Create`, `Update`, `EnsureCanBeDeleted`.

## Patterns

- [[Application-Services]] — load aggregate, call domain method, save.
- [[Pagination]] — server-side paging with `PagedResult<T>`.
- [[Controllers]] — thin MVC controllers.
- [[Testing]] — xUnit + FluentAssertions for domain entities.

## Error handling

- Domain invariants throw `DomainException`.
- Controllers catch `DomainException` and add the message to `ModelState` for form re-display.
- `DeleteAsync` returns `false` for missing entities; controllers return `NotFound()`.

## Related

- [[Architecture-Overview]]
- [[Application-Services]]
- [[Pagination]]
- [[Controllers]]
- [[Testing]]
- [[Sidebar-Logo-Icon]]
