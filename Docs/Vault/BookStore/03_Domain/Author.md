---
type: reference
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/reference
  - area/domain
  - entity/author
---

# Author

The `Author` domain entity: properties, computed members, factory/update methods, and invariants.

## Source

`Src/BookStore.Domain/Entities/Author.cs`

## Properties

| Property | Type | Notes |
|----------|------|-------|
| `Id` | `int` | Database-generated identity. |
| `Name` | `string` | Required, max 150 characters, trimmed on create/update. |
| `Bio` | `string` | Max 2000 characters, trimmed. |
| `BirthDate` | `DateTime` | Cannot be in the future. |
| `Nationality` | `string` | Max 100 characters, trimmed. |
| `Books` | `ICollection<Book>` | One-to-many navigation collection. |

## Computed members

- `Age` — `(int)Math.Floor((DateTime.UtcNow - BirthDate).TotalDays / 365.25)`.

## Factory and mutators

- `Author.Create(name, bio, nationality, birthDate)` — validates then returns a new instance.
- `Update(name, bio, nationality, birthDate)` — validates then mutates the existing instance.

## Invariants (exact `DomainException` messages)

| Condition | Message |
|-----------|---------|
| `Name` is null/empty/whitespace | `"Author name is required."` |
| `Name` length > 150 | `"Author name must be at most 150 characters."` |
| `BirthDate` is in the future | `"Birth date cannot be in the future."` |

## Deletion rule

```csharp
public void EnsureCanBeDeleted()
{
    if (Books.Any())
        throw new DomainException("Cannot delete an author who still has books.");
}
```

## EF Core mapping

In `Src/BookStore.Domain/Data/BookStoreContext.cs`:

```csharp
modelBuilder.Entity<Author>(entity =>
{
    entity.HasKey(a => a.Id);
    entity.Property(a => a.Name).IsRequired().HasMaxLength(150);
    entity.Property(a => a.Bio).HasMaxLength(2000);
    entity.Property(a => a.Nationality).HasMaxLength(100);
});
```

## Related

- [[Book]]
- [[Domain-Overview]]
- [[Architecture-Overview]]
