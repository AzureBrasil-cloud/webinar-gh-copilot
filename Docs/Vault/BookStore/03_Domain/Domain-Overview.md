---
type: overview
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/overview
  - area/domain
  - entity/author
  - entity/book
---

# Domain Overview

Domain layer for BookStore: entities, relationships, invariants, errors, and seed data.

## Context

All business rules are owned by `BookStore.Domain`. Services and controllers may only call domain methods; they do not mutate entity state directly.

## Entities

- [[Author]] — author aggregate with name, bio, nationality, birth date, age, and book collection.
- [[Book]] — book aggregate with title, ISBN, genre, price, stock, availability, page count, and author reference.

## Relationship

- One author has many books (`Author.Books`).
- Each book belongs to one author (`Book.AuthorId` → `Author`).
- EF Core mapping uses `OnDelete(DeleteBehavior.Restrict)` in `BookStoreContext.OnModelCreating`.
- Domain-level guard: `Author.EnsureCanBeDeleted()` throws `DomainException` when the author still has books.

## Error model

- All invariant violations throw `DomainException` (`Src/BookStore.Domain/Exceptions/DomainException.cs`).
- `DomainException` is a plain `Exception` subclass with a message constructor.
- Domain entities never throw `InvalidOperationException`.

## Seed data

- `DataSeeder.Seed(db)` (`Src/BookStore.Domain/Seed/DataSeeder.cs`) is a no-op if any author or book already exists.
- Seeds 10 authors and 27 books across genres, some with `Stock = 0` to exercise the `IsAvailable` invariant.
- Persistence is EF Core In-Memory; data resets on every process restart.

## Source files

- `Src/BookStore.Domain/Entities/Author.cs`
- `Src/BookStore.Domain/Entities/Book.cs`
- `Src/BookStore.Domain/Data/BookStoreContext.cs`
- `Src/BookStore.Domain/Seed/DataSeeder.cs`
- `Src/BookStore.Domain/Exceptions/DomainException.cs`

## Related

- [[Author]]
- [[Book]]
- [[Architecture-Overview]]
- [[Operations-Overview]]
