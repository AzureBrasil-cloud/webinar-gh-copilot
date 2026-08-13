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
- MVC `Index` actions accept `int page = 1` and pass it to the service.
- The JSON API controllers (`Controllers/Api/*ApiController.cs`) expose `GET /api/<feature>?page=&pageSize=` and map `PagedResult<T>` to `PagedResultDto<T>` (`Items`, `PageNumber`, `PageSize`, `TotalItems`, `TotalPages`, `HasPrevious`, `HasNext`). They reuse the same `GetPagedAsync` - never re-implement paging there.

## Views and front-end
- Index views are Vue mount points (`<div id="<feature>-app" data-api-url="/api/<feature>">`); the list and its pager are rendered by PrimeVue, not Razor. Views may still declare `@model PagedResult<Entity>` as a server-side fallback.
- Paging happens through PrimeVue `DataTable` with `lazy` + `paginator`, fed by `composables/usePagedFetch.ts`, which calls the paged API endpoint on every page change.
- `Views/Shared/_Pagination.cshtml` is a Bootstrap-era leftover no longer referenced by any view. Do not wire it back in; if a new Razor list page needs paging, migrate it to the Vue + API pattern instead.

## What not to do
- Do not add a NuGet pagination package.
- Do not paginate on the client side (never fetch all rows and slice them in the browser).
- Do not duplicate paging logic in an API controller or a Vue component.
- Only change Index actions - leave CRUD actions untouched.