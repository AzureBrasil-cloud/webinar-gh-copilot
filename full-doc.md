# This is a  full documentation of the solution. This doc will be source of truth for the vault. It will be maintained by code agents after code changes.

## Tags

- Create more tags as needed
- Example of tags below:

type/*
- `type/overview` — high-level summary of a folder or area
- `type/index` — navigation/tag/glossary index note
- `type/guide` — how-to or engineering guide
- `type/runbook` — operational step-by-step procedure
- `type/reference` — external documentation or link summary
- `type/template` — note template

area/*
- `area/architecture` — solution layering and structure
- `area/domain` — domain entities and business rules
- `area/engineering` — code style and conventions
- `area/operations` — build, run, test procedures

component/*
- `component/domain` — `BookStore.Domain`
- `component/application` — `BookStore.Application`
- `component/web` — `BookStore.Web`
- `component/tests` — `BookStore.Tests`

entity/*
- `entity/author` — `Author` domain entity
- `entity/book` — `Book` domain entity

Base
- `note` — applied to every note in the vault

## Domain

Source: `Src/BookStore.Domain/Entities/`, `Src/BookStore.Domain/Data/BookStoreContext.cs`, `Src/BookStore.Domain/Seed/DataSeeder.cs`, `Src/BookStore.Domain/Exceptions/DomainException.cs`.

### Author (`entity/author`)
- Properties: `Id`, `Name`, `Bio`, `BirthDate`, `Nationality`, `Books` (collection), computed `Age` (`(DateTime.UtcNow - BirthDate).TotalDays / 365.25`).
- Created via `Author.Create(name, bio, nationality, birthDate)`; mutated via `Update(...)`.
- Invariants (`Author.cs`):
  - `Name` required, max 150 characters.
  - `BirthDate` cannot be in the future.
- `EnsureCanBeDeleted()` throws `DomainException` if the author still has books ("Cannot delete an author who still has books.").
- EF Core mapping (`BookStoreContext.OnModelCreating`): `Name` required/max 150, `Bio` max 2000, `Nationality` max 100.

```csharp
// Src/BookStore.Domain/Entities/Author.cs
public static Author Create(string name, string bio, string nationality, DateTime birthDate)
{
    Validate(name, birthDate);
    return new Author { Name = name.Trim(), /* ... */ BirthDate = birthDate };
}

public void EnsureCanBeDeleted()
{
    if (Books.Any())
        throw new DomainException("Cannot delete an author who still has books.");
}
```

### Book (`entity/book`)
- Properties: `Id`, `Title`, `Isbn`, `Description`, `Genre`, `Price`, `Stock`, `PublishedDate`, `NumberOfPages` (private setter), `AuthorId`/`Author`, computed `IsAvailable` (`Stock > 0`).
- Created via `Book.Create(title, isbn, description, genre, price, stock, publishedDate, authorId, numberOfPages)`; mutated via `Update(...)`.
- Invariants (`Book.cs`):
  - `Title` required.
  - `Price >= 0`.
  - `Stock >= 0`.
  - `PublishedDate` cannot be in the future.
  - `NumberOfPages >= 1`.
- EF Core mapping: `Title` required/max 250, `Isbn` max 20, `Description` max 2000, `Genre` max 60, `Price` has precision `(18, 2)`.
- Relationship: `Book.AuthorId` → `Author`, one-to-many (`Author.Books`), `OnDelete(DeleteBehavior.Restrict)` — DB will not cascade-delete an author with books (enforced at both DB and domain level via `EnsureCanBeDeleted`).

```csharp
// Src/BookStore.Domain/Entities/Book.cs
private static void Validate(string title, decimal price, int stock, DateTime publishedDate, int numberOfPages)
{
    if (string.IsNullOrWhiteSpace(title))
        throw new DomainException("Title is required.");
    if (price < 0)
        throw new DomainException("Price cannot be negative.");
    // ... Stock, PublishedDate, NumberOfPages checks
}
```

### Errors
- All invariant violations throw `DomainException` (`Src/BookStore.Domain/Exceptions/DomainException.cs`), a plain `Exception` subclass with a `string message` constructor. Domain entities never throw `InvalidOperationException`.
- `BookService`/`AuthorService` also throw `DomainException` for orchestration-level checks (e.g. "Author does not exist." in `BookService.CreateAsync`/`UpdateAsync` when `AuthorId` doesn't match an existing author).

### Seed data
- `DataSeeder.Seed(db)` (`Src/BookStore.Domain/Seed/DataSeeder.cs`) is a no-op if any `Author` or `Book` already exists.
- Seeds 10 authors (e.g. George Orwell, Jane Austen, J.R.R. Tolkien, Agatha Christie, Ernest Hemingway, Gabriel García Márquez, Haruki Murakami, Stephen King, Isaac Asimov, Virginia Woolf) and 27 books across genres (Dystopian, Romance, Fantasy, Mystery, Sci-Fi, Horror, Modernist, etc.), some with `Stock = 0` to exercise the `IsAvailable` invariant.
- Persistence is EF Core **In-Memory** (`BookStoreContext`) — data resets on every process restart; there is no database migration story.

## Architecture

Source: `Src/BookStore.slnx`, `Src/BookStore.Web/Program.cs`, `.github/copilot-instructions.md`.

### Solution layout (`area/architecture`)
.NET 10 multi-project solution under `Src/`:

- `BookStore.Domain` (`component/domain`) — entities (`Author`, `Book`), `BookStoreContext` (EF Core In-Memory `DbContext`), `DataSeeder`, `DomainException`. Owns all business rules.
- `BookStore.Application` (`component/application`) — application services (`AuthorService`, `IAuthorService`, `BookService`, `IBookService`) and `Common/PagedResult<T>`. Orchestration only: no business rules, just load → call domain method → `SaveChangesAsync`.
- `BookStore.Web` (`component/web`) — ASP.NET Core 10 MVC. `Controllers/` (`AuthorsController`, `BooksController`, `HomeController`), Razor `Views/` (Bootstrap 5), `Program.cs` composition root.
- `BookStore.Tests` (`component/tests`) — xUnit tests for `BookStore.Domain` entities (`AuthorTests`, `BookTests`).

### Project references
- `BookStore.Web` → `BookStore.Application`, `BookStore.Domain`.
- `BookStore.Application` → `BookStore.Domain`.
- `BookStore.Tests` → `BookStore.Domain`.
- `BookStore.Domain` has no project references; depends only on `Microsoft.EntityFrameworkCore.InMemory`.

### Composition root (`Program.cs`)
- Registers `AddControllersWithViews()`.
- Registers `BookStoreContext` with `UseInMemoryDatabase("BookStoreDb")`.
- Registers `IAuthorService`/`AuthorService` and `IBookService`/`BookService` as scoped.
- Seeds the in-memory database once at startup via `DataSeeder.Seed(db)` inside a startup scope.
- Uses the default MVC route: `{controller=Home}/{action=Index}/{id?}`.
- No authentication/authorization is configured (`app.UseAuthorization()` is present but there's no auth scheme registered — the solution has no login).

```csharp
// Src/BookStore.Web/Program.cs
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

### Layering rule
- **Domain** owns invariants (rich entities, `DomainException`).
- **Application** only orchestrates: load aggregate via `BookStoreContext` → call domain method → `SaveChangesAsync`. Never duplicates business rules.
- **Controllers** are thin: model binding → service call → view/redirect. No business logic in controllers.

## Engineering

Source: `.github/copilot-instructions.md`, `.github/instructions/pagination.instructions.md`, `Src/BookStore.Application/Services/*.cs`, `Src/BookStore.Web/Controllers/*.cs`.

### Code style (`area/engineering`)
- C# 12, nullable enabled, file-scoped namespaces, `var` when the type is obvious.
- `async`/`await` end-to-end for all DB calls (`Task<...>` methods throughout services and controllers).
- Domain methods use verbs: `Create`, `Update`, `EnsureCanBeDeleted` (`Author`/`Book`); future domain methods should follow the same verb convention (e.g. `Restock`, `ChangePrice`).

### Application services pattern
- `AuthorService`/`BookService` take a `BookStoreContext` via constructor injection.
- Every list method (`GetAllAsync`, `GetPagedAsync`) uses `.Include(...)` to eager-load the related entity (`Author.Books`, `Book.Author`) and orders results (`OrderBy(a => a.Name)` / `OrderBy(b => b.Title)`).
- `CreateAsync`/`UpdateAsync` call the domain factory/`Update` method — never set entity properties directly from the service.
- `DeleteAsync` returns `false` (not an exception) when the entity does not exist, letting the controller decide (`NotFound()`).

```csharp
// Src/BookStore.Application/Services/BookService.cs
public async Task<Book> CreateAsync(Book book, int numberOfPages)
{
    var authorExists = await _db.Authors.AnyAsync(a => a.Id == book.AuthorId);
    if (!authorExists)
        throw new DomainException("Author does not exist.");

    var entity = Book.Create(book.Title, book.Isbn, /* ... */ book.AuthorId, numberOfPages);
    _db.Books.Add(entity);
    await _db.SaveChangesAsync();
    return entity;
}
```

### Pagination (`.github/instructions/pagination.instructions.md`)
- `PagedResult<T>` (`Src/BookStore.Application/Common/PagedResult.cs`): `Items`, `PageNumber` (1-based), `PageSize`, `TotalItems`, computed `TotalPages`, `HasPrevious`, `HasNext`.
- Service `GetPagedAsync(pageNumber, pageSize)`: clamps `pageNumber >= 1` and `pageSize` to `[1, 100]`, uses `Skip`/`Take` + `CountAsync()` — never materializes the full table.
- Controllers: `Index(int page = 1)` passes `page` to `GetPagedAsync(page, pageSize: 10)` (default page size 10).
- Razor `Index` views are typed `@model PagedResult<Entity>`, render `Model.Items`, and use Bootstrap 5 pagination controls (`asp-route-page`).
- No client-side pagination and no pagination NuGet package — only CRUD list (`Index`) actions are paginated.

```csharp
// Src/BookStore.Application/Common/PagedResult.cs
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}
```

```csharp
// Src/BookStore.Application/Services/BookService.cs
public async Task<PagedResult<Book>> GetPagedAsync(int pageNumber, int pageSize)
{
    pageNumber = Math.Max(1, pageNumber);
    pageSize = Math.Clamp(pageSize, 1, 100);
    var query = _db.Books.Include(b => b.Author).OrderBy(b => b.Title);
    var total = await query.CountAsync();
    var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    return new PagedResult<Book> { Items = items, PageNumber = pageNumber, PageSize = pageSize, TotalItems = total };
}
```

### Controllers
- Thin CRUD controllers (`BooksController`, `AuthorsController`) follow the same shape: `Index` (paged), `Details`, `Create` (GET populates dropdowns via `PopulateAuthorsAsync`, POST validates `ModelState` then calls the service inside a `try/catch (DomainException)` that re-adds the error to `ModelState`), `Edit` (same pattern), `Delete`/`DeleteConfirmed`.
- `BooksController` uses `SelectList` (`ViewBag.Authors`) to populate the author dropdown for Create/Edit forms.
- All mutating actions (`Create`, `Edit`, `DeleteConfirmed`) use `[ValidateAntiForgeryToken]`.

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

### Testing
- `BookStore.Tests` (xUnit + FluentAssertions) covers `BookStore.Domain` entities only (`AuthorTests`, `BookTests`) — no integration/controller tests yet.

## Operations

Source: `README.md`, `Src/BookStore.Web/Properties/launchSettings.json`, `Src/BookStore.Web/appsettings.json`, `deploy.sh`.

### Local development (`area/operations`)
- Build: `dotnet build Src/BookStore.slnx`
- Run: `dotnet run --project Src/BookStore.Web` → <http://localhost:5045>
- Test: `dotnet test Src/BookStore.Tests/BookStore.Tests.csproj`
- Persistence: EF Core In-Memory (`BookStoreContext`, database name `BookStoreDb`) — no migrations, data resets on every restart, no other provider is configured.
- `appsettings.json`: default ASP.NET Core logging config only (`Logging.LogLevel`, `AllowedHosts: "*"`). No connection strings, no secrets, no feature flags today.

### Production environment
- Hosted on **Azure App Service**, running on a **B1** App Service Plan instance.
- Deployment is done via [`deploy.sh`](../deploy.sh) at the repo root, which:
  1. Publishes `Src/BookStore.Web/BookStore.Web.csproj` in `Release` configuration to `./publish/web`.
  2. Zips the publish output into `./publish/web.zip`.
  3. Deploys the zip with `az webapp deploy` to the fixed Azure resource names:
     - Resource group: `rg-book-store`
     - App Service Plan: `asp-book-store`
     - Web App: `app-book-store`
  4. Prints the resulting URL: `https://app-book-store.azurewebsites.net`.
- The script assumes an active `az login` session with the correct subscription already selected (no `subscription-id` parameter) and requires the `az` CLI and `zip` to be installed locally.

```bash
# deploy.sh
RG="rg-book-store"
APP_SERVICE_PLAN="asp-book-store"
APP="app-book-store"

dotnet publish "$WEB_PROJECT" -c Release -o "$PUB_DIR"
# ... zip PUB_DIR into ZIP_FILE
az webapp deploy -g "$RG" -n "$APP" --type zip --src-path "$ZIP_FILE"
```

- No CI/CD pipeline is defined yet — deployment is manual, run locally via `./deploy.sh`.
- Azure region: **East US 2**.
- No deployment slots — `./deploy.sh` deploys directly to the production Web App (`app-book-store`).
- No monitoring/observability tooling (Application Insights, log streaming) configured yet.

## References
[https://learn.microsoft.com/pt-br/dotnet/core/whats-new/dotnet-10/overview](https://learn.microsoft.com/pt-br/dotnet/core/whats-new/dotnet-10/overview)

[https://learn.microsoft.com/pt-br/aspnet/core/?view=aspnetcore-10.0](https://learn.microsoft.com/pt-br/aspnet/core/?view=aspnetcore-10.0)

[https://learn.microsoft.com/pt-br/aspnet/core/mvc/overview?view=aspnetcore-10.0](https://learn.microsoft.com/pt-br/aspnet/core/mvc/overview?view=aspnetcore-10.0)

[https://learn.microsoft.com/pt-br/aspnet/core/data/ef-mvc/?view=aspnetcore-10.0](https://learn.microsoft.com/pt-br/aspnet/core/data/ef-mvc/?view=aspnetcore-10.0)

[https://learn.microsoft.com/pt-br/ef/core/](https://learn.microsoft.com/pt-br/ef/core/)

[https://learn.microsoft.com/pt-br/ef/core/what-is-new/ef-core-10.0/whatsnew](https://learn.microsoft.com/pt-br/ef/core/what-is-new/ef-core-10.0/whatsnew)

[https://learn.microsoft.com/pt-br/azure/app-service/](https://learn.microsoft.com/pt-br/azure/app-service/)

[https://learn.microsoft.com/pt-br/azure/app-service/quickstart-dotnetcore](https://learn.microsoft.com/pt-br/azure/app-service/quickstart-dotnetcore)

[https://learn.microsoft.com/pt-br/aspnet/core/host-and-deploy/azure-apps/?view=aspnetcore-10.0](https://learn.microsoft.com/pt-br/aspnet/core/host-and-deploy/azure-apps/?view=aspnetcore-10.0)

[https://learn.microsoft.com/pt-br/azure/app-service/configure-language-dotnetcore](https://learn.microsoft.com/pt-br/azure/app-service/configure-language-dotnetcore)

[https://learn.microsoft.com/pt-br/azure/app-service/overview-hosting-plans](https://learn.microsoft.com/pt-br/azure/app-service/overview-hosting-plans)

[https://learn.microsoft.com/pt-br/azure/app-service/app-service-plan-manage](https://learn.microsoft.com/pt-br/azure/app-service/app-service-plan-manage)

[https://learn.microsoft.com/pt-br/azure/app-service/configure-common](https://learn.microsoft.com/pt-br/azure/app-service/configure-common)

[https://learn.microsoft.com/pt-br/azure/app-service/app-service-key-vault-references](https://learn.microsoft.com/pt-br/azure/app-service/app-service-key-vault-references)

[https://learn.microsoft.com/pt-br/azure/app-service/monitor-app-service](https://learn.microsoft.com/pt-br/azure/app-service/monitor-app-service)

[https://learn.microsoft.com/pt-br/azure/app-service/troubleshoot-diagnostic-logs](https://learn.microsoft.com/pt-br/azure/app-service/troubleshoot-diagnostic-logs)


