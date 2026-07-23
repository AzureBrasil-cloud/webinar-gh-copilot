---
type: guide
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/guide
  - area/engineering
  - component/web
  - entity/customer
---

# Controllers

Thin MVC controller pattern used by `BooksController`, `AuthorsController`, `CustomersController`, and `HomeController`.

## Context

Controllers handle model binding, invoke application services, and return views or redirects. They contain no business logic.

## Common shape

Most CRUD controllers follow this structure:

- `Index(int page = 1)` — paged list via `GetPagedAsync`.
- `Details(int id)` — fetch by id, return `NotFound()` if missing.
- `Create` GET/POST — show form, validate `ModelState`, call service, catch `DomainException`.
- `Edit` GET/POST — same pattern, plus id mismatch check.
- `Delete` GET/POST — confirm view, then delete.

## POST action pattern

```csharp
// Src/BookStore.Web/Controllers/BooksController.cs
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Book book, int numberOfPages)
{
    if (!ModelState.IsValid)
    {
        await PopulateAuthorsAsync(book.AuthorId);
        return View(book);
    }

    try
    {
        await _bookService.CreateAsync(book, numberOfPages);
        return RedirectToAction(nameof(Index));
    }
    catch (DomainException ex)
    {
        ModelState.AddModelError(string.Empty, ex.Message);
        await PopulateAuthorsAsync(book.AuthorId);
        return View(book);
    }
}
```

## DomainException handling

Controllers catch `DomainException` from services and add the message to `ModelState` so the view can re-display the form with the error.

## Author dropdown

`BooksController` uses `ViewBag.Authors` with a `SelectList` populated by `PopulateAuthorsAsync` for Create/Edit forms.

## CustomersController

`CustomersController` follows the same shape with no navigation-property concerns (`Customer` has none). It binds directly to the `Customer` entity for Create/Edit (no separate view-model), same as `AuthorsController`:

```csharp
// Src/BookStore.Web/Controllers/CustomersController.cs
public async Task<IActionResult> Index(int page = 1)
{
    var result = await _customerService.GetPagedAsync(page, pageSize: 10);
    return View(result);
}
```

Views live in `Src/BookStore.Web/Views/Customers/` (`Index`, `Details`, `Create`, `Edit`, `Delete`), mirroring `Views/Authors/*.cshtml`, and `Index.cshtml` uses the shared `_Pagination` partial (see [[Pagination]]).

`ICustomerService`/`CustomerService` are registered in `Src/BookStore.Web/Program.cs` via `AddScoped<ICustomerService, CustomerService>()`.

## Anti-forgery

All mutating POST actions use `[ValidateAntiForgeryToken]`.

## Source

- `Src/BookStore.Web/Controllers/BooksController.cs`
- `Src/BookStore.Web/Controllers/AuthorsController.cs`
- `Src/BookStore.Web/Controllers/CustomersController.cs`
- `Src/BookStore.Web/Controllers/HomeController.cs`

## Related

- [[Application-Services]]
- [[Pagination]]
- [[Architecture-Overview]]
- [[Engineering-Overview]]
