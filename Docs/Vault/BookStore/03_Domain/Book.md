---
type: reference
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/reference
  - area/domain
  - entity/book
---

# Book

The `Book` domain entity: properties, computed members, factory/update methods, and invariants.

## Source

`Src/BookStore.Domain/Entities/Book.cs`

## Properties

| Property | Type | Notes |
|----------|------|-------|
| `Id` | `int` | Database-generated identity. |
| `Title` | `string` | Required, max 250 characters, trimmed. |
| `Isbn` | `string` | Max 20 characters, trimmed. |
| `Description` | `string` | Max 2000 characters, trimmed. |
| `Genre` | `string` | Max 60 characters, trimmed. |
| `Price` | `decimal` | Must be >= 0. |
| `Stock` | `int` | Must be >= 0. |
| `PublishedDate` | `DateTime` | Cannot be in the future. |
| `NumberOfPages` | `int` | Private setter, must be >= 1. |
| `IsAvailable` | `bool` | Computed as `Stock > 0`. |
| `AuthorId` | `int` | Foreign key to `Author`. |
| `Author` | `Author?` | Navigation property. |

## Factory and mutators

- `Book.Create(title, isbn, description, genre, price, stock, publishedDate, authorId, numberOfPages)` — validates then returns a new instance.
- `Update(...)` — validates then mutates the existing instance.

## Invariants (exact `DomainException` messages)

| Condition | Message |
|-----------|---------|
| `Title` is null/empty/whitespace | `"Title is required."` |
| `Price` < 0 | `"Price cannot be negative."` |
| `Stock` < 0 | `"Stock cannot be negative."` |
| `PublishedDate` is in the future | `"Published date cannot be in the future."` |
| `NumberOfPages` < 1 | `"NumberOfPages must be at least 1."` |

## EF Core mapping

In `Src/BookStore.Domain/Data/BookStoreContext.cs`:

```csharp
modelBuilder.Entity<Book>(entity =>
{
    entity.HasKey(b => b.Id);
    entity.Property(b => b.Title).IsRequired().HasMaxLength(250);
    entity.Property(b => b.Isbn).HasMaxLength(20);
    entity.Property(b => b.Description).HasMaxLength(2000);
    entity.Property(b => b.Genre).HasMaxLength(60);
    entity.Property(b => b.Price).HasPrecision(18, 2);

    entity.HasOne(b => b.Author)
        .WithMany(a => a.Books)
        .HasForeignKey(b => b.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

## Related

- [[Author]]
- [[Domain-Overview]]
- [[Architecture-Overview]]
