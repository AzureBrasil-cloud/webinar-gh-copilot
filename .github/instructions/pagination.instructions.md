---
applyTo: "**/*.cshtml,Src/BookStore.Application/Common/PagedResult.cs,Src/BookStore.Application/Services/*.cs"
---

# Pagination conventions for BookStore.Web

When implementing or modifying a list/index page:

## Application layer
- Add `Application/Common/PagedResult.cs` with a generic `PagedResult<T>`:
  - Properties: `Items` (IReadOnlyList<T>), `PageNumber` (int, 1-based), `PageSize` (int), `TotalItems` (int).
  - Computed: `TotalPages`, `HasPrevious`, `HasNext`.
- Service index methods expose a paged variant: `Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize)`.
  - Default page size: **10**.
  - Clamp `pageNumber >= 1`, `pageSize` between 1 and 100.
  - Use `Skip((pageNumber - 1) * pageSize).Take(pageSize)` before materializing.
  - Count via `CountAsync()` over the same base query.
- Do **not** load the full table into memory to paginate.

## Controllers
- `Index` actions accept `int page = 1` and pass it to the service.

## Razor views
- Index views typed `@model PagedResult<Entity>`.
- Render table rows from `Model.Items`.
- Bootstrap 5 pagination below the table:
  - "Previous" disabled when `!Model.HasPrevious`.
  - "Next" disabled when `!Model.HasNext`.
  - "Page X of Y - N items" indicator.
  - Links use `asp-action="Index"` + `asp-route-page="@i"`.

## What not to do
- Do not add a NuGet pagination package.
- Do not paginate on the client side.
- Only change Index actions - leave CRUD actions untouched.