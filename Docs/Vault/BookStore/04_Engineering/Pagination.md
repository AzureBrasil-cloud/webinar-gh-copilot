---
type: guide
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/guide
  - area/engineering
  - component/application
---

# Pagination

Server-side pagination conventions for list/index pages in BookStore.Web.

## Context

List actions are paginated to avoid loading full tables into memory. The default page size is **10**.

## `PagedResult<T>`

`Src/BookStore.Application/Common/PagedResult.cs`:

```csharp
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
```

## Service implementation

`GetPagedAsync(pageNumber, pageSize)` clamps inputs, counts, and pages the same base query:

```csharp
// Src/BookStore.Application/Services/BookService.cs
pageNumber = Math.Max(1, pageNumber);
pageSize = Math.Clamp(pageSize, 1, 100);
var query = _db.Books.Include(b => b.Author).OrderBy(b => b.Title);
var total = await query.CountAsync();
var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
return new PagedResult<Book> { Items = items, PageNumber = pageNumber, PageSize = pageSize, TotalItems = total };
```

`CustomerService` follows the same clamp/count/page shape, ordered by `FullName` with no `.Include(...)` (no navigation properties):

```csharp
// Src/BookStore.Application/Services/CustomerService.cs
pageNumber = Math.Max(1, pageNumber);
pageSize = Math.Clamp(pageSize, 1, 100);
var query = _db.Customers.OrderBy(c => c.FullName);
var total = await query.CountAsync();
var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
return new PagedResult<Customer> { Items = items, PageNumber = pageNumber, PageSize = pageSize, TotalItems = total };
```

## Controller usage

```csharp
// Src/BookStore.Web/Controllers/BooksController.cs
public async Task<IActionResult> Index(int page = 1)
{
    var result = await _bookService.GetPagedAsync(page, pageSize: 10);
    return View(result);
}
```

## Razor views

- Index views are typed `@model PagedResult<Entity>`.
- Rows render from `Model.Items`.
- Bootstrap 5 pagination uses `asp-route-page` and respects `HasPrevious`/`HasNext`.

## Rules

- Default page size: **10**.
- `pageSize` clamped to `[1, 100]`.
- Use `CountAsync()` over the same query — never materialize the full table.
- Do not add a pagination NuGet package.
- Do not paginate on the client side.
- Only `Index` actions are paginated; leave other CRUD actions untouched.

## Source

- `Src/BookStore.Application/Common/PagedResult.cs`
- `Src/BookStore.Application/Services/AuthorService.cs`
- `Src/BookStore.Application/Services/BookService.cs`
- `Src/BookStore.Application/Services/CustomerService.cs`
- `.github/instructions/pagination.instructions.md`

## Related

- [[Application-Services]]
- [[Controllers]]
- [[Engineering-Overview]]
