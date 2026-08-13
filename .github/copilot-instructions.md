# BookStore - Copilot repository instructions

## Solution layout
- .NET 10 multi-project solution under `Src/`.
  - `BookStore.Domain` - entities, `BookStoreContext` (EF Core In-Memory), seed data.
  - `BookStore.Application` - application services (orchestration only).
  - `BookStore.Web` - ASP.NET Core 10 MVC (controllers, Razor views) whose UI is rendered by Vue 3 + PrimeVue + Tailwind CSS v4 islands built from `ClientApp/`. Bootstrap has been removed; jQuery remains only for unobtrusive validation on the surviving Razor Customers forms.
- No authentication.

## Front-end
- See `.github/instructions/frontend.instructions.md` (ClientApp: Vue/PrimeVue/Tailwind conventions) and `.github/instructions/web.instructions.md` (how MVC serves the Vue bundle and the `/api/*` contract).
- `npm run build` in `Src/BookStore.Web/ClientApp` is required after any front-end change; it is not part of `dotnet build`.

## Architecture rules
- **Domain layer** (`BookStore.Domain`) owns all business rules.
  - Entities (`Author`, `Book`) are **rich**: invariants are enforced through domain methods, not in services.
  - Invariants include: `Title required`, `Price >= 0`, `Stock >= 0`, `PublishedDate` not in the future, `BirthDate` not in the future, `IsAvailable = Stock > 0`, "Author with books cannot be deleted".
  - Throw `DomainException` (in `BookStore.Domain/Exceptions/DomainException.cs`) on invariant violations. Never throw `InvalidOperationException` from entities.
- **Application layer** (`BookStore.Application`) only orchestrates:
  - Load aggregates via `BookStoreContext`.
  - Call domain methods on entities.
  - Persist via `SaveChangesAsync`.
  - Do not duplicate business rules.
- **Controllers** are thin: model binding - service call - view/redirect. No business logic.

## Code style
- C# 12, nullable enabled, file-scoped namespaces, `var` when type is obvious.
- `async`/`await` end-to-end for all DB calls.
- Domain methods use verbs: `Rename`, `Restock`, `ChangePrice`, `AssignAuthor`, etc.

## Build & run
- Build: `dotnet build Src/BookStore.slnx`
- Run: `dotnet run --project Src/BookStore.Web`
- Persistence: EF Core In-Memory only. Do not add migrations or change the provider.

## General guidance
- Read the file before editing it. Do not invent class names.
- Keep changes minimal and focused on the requested task.