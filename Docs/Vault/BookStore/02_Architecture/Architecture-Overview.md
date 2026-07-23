---
type: overview
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/overview
  - area/architecture
  - component/domain
  - component/application
  - component/web
---

# Architecture Overview

Three-layer architecture for the BookStore ASP.NET Core 10 MVC solution.

## Context

The solution is intentionally split into Domain, Application, and Web layers so that business rules live in exactly one place and controllers stay thin.

## Layers

### Domain (`BookStore.Domain`)

- Owns all business rules and invariants.
- Contains `Author`, `Book`, `BookStoreContext`, `DataSeeder`, and `DomainException`.
- Entities are rich: state changes happen through domain methods (`Create`, `Update`, `EnsureCanBeDeleted`).
- Source: `Src/BookStore.Domain/`.

### Application (`BookStore.Application`)

- Orchestrates only: load aggregates from `BookStoreContext`, call domain methods, persist with `SaveChangesAsync`.
- Contains `AuthorService`, `BookService`, their interfaces, and `PagedResult<T>`.
- Must not duplicate domain invariants.
- Source: `Src/BookStore.Application/Services/`.

### Web (`BookStore.Web`)

- ASP.NET Core 10 MVC project.
- Controllers handle model binding, call services, and return views/redirects.
- No business logic in controllers.
- Source: `Src/BookStore.Web/Controllers/`.

## Composition root

`Src/BookStore.Web/Program.cs` wires everything:

```csharp
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BookStoreContext>(options =>
    options.UseInMemoryDatabase("BookStoreDb"));

builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
// ...
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookStoreContext>();
    DataSeeder.Seed(db);
}
```

## Layering rules

1. **Domain owns invariants** — entities throw `DomainException` on violations.
2. **Application orchestrates** — it never re-implements validation rules.
3. **Controllers are thin** — model binding → service call → view/redirect.

## Authentication

No authentication or authorization scheme is registered. `app.UseAuthorization()` is present, but the solution has no login flow.

## Related

- [[Repository-Structure]]
- [[Domain-Overview]]
- [[Application-Services]]
- [[Controllers]]
- [[Operations-Overview]]
