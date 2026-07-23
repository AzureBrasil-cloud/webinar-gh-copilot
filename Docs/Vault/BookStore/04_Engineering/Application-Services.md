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

# Application Services

The orchestration pattern used by `AuthorService`, `BookService`, and `CustomerService` in `BookStore.Application`.

## Context

Application services sit between controllers and the domain. They load aggregates, delegate to domain methods, and persist changes. They do not contain business rules.

## Responsibilities

1. Load aggregates from `BookStoreContext` (usually with `.Include(...)`).
2. Call domain factory or update methods (`Author.Create`, `Book.Update`, etc.).
3. Persist with `_db.SaveChangesAsync()`.
4. Return the result or `null`/`false` when an entity is not found.

## Loading related data

List methods eager-load and order results:

```csharp
// Src/BookStore.Application/Services/AuthorService.cs
return await _db.Authors
    .Include(a => a.Books)
    .OrderBy(a => a.Name)
    .ToListAsync();
```

```csharp
// Src/BookStore.Application/Services/BookService.cs
return await _db.Books
    .Include(b => b.Author)
    .OrderBy(b => b.Title)
    .ToListAsync();
```

`Customer` has no navigation properties, so `CustomerService` skips `.Include(...)`:

```csharp
// Src/BookStore.Application/Services/CustomerService.cs
return await _db.Customers
    .OrderBy(c => c.FullName)
    .ToListAsync();
```

## Create pattern

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

## Update pattern

```csharp
// Src/BookStore.Application/Services/AuthorService.cs
var existing = await _db.Authors.FirstOrDefaultAsync(a => a.Id == id);
if (existing is null)
    return null;

existing.Update(author.Name, author.Bio, author.Nationality, author.BirthDate);
await _db.SaveChangesAsync();
return existing;
```

## Delete pattern

```csharp
// Src/BookStore.Application/Services/AuthorService.cs
if (existing is null)
    return false;

existing.EnsureCanBeDeleted();
_db.Authors.Remove(existing);
await _db.SaveChangesAsync();
return true;
```

`CustomerService.DeleteAsync` removes directly - `Customer` has no `EnsureCanBeDeleted()` guard:

```csharp
// Src/BookStore.Application/Services/CustomerService.cs
if (existing is null)
    return false;

_db.Customers.Remove(existing);
await _db.SaveChangesAsync();
return true;
```

## What not to do

- Do not set entity properties directly from the service.
- Do not duplicate domain validation logic.
- Do not materialize the full table to paginate (see [[Pagination]]).

## Related

- [[Architecture-Overview]]
- [[Pagination]]
- [[Controllers]]
- [[Domain-Overview]]
