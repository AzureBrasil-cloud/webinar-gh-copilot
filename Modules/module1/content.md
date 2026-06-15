# Module 1 — GitHub Copilot: Ask, Plan, Agent modes & custom instructions

**Goal:** use the three GitHub Copilot interaction modes and custom instructions to fix two real design problems in a .NET 10 MVC application — without writing a single line of code by hand.

**Deliverable:** `BookStore.Web` refactored so that (1) domain rules live inside the domain entities and (2) the index pages have server-side pagination — all driven by Copilot with the right context.

---

## Prerequisites

✅ **Tools installed**

- VS Code (latest)
- GitHub Copilot + GitHub Copilot Chat extensions, signed in
- .NET 10 SDK

```bash
code --version
dotnet --version
```

✅ **Repository checked out — `initial` branch**

```bash
git checkout initial
```

---

## Problems to solve in this module

### Problem 1 — Domain logic leaking into the application layer

`AuthorService` and `BookService` enforce rules that belong to the domain:

- Field validation: `Title required`, `Price >= 0`, `Stock >= 0`, `BirthDate` not in the future, etc.
- Derived state: `Book.IsAvailable = Stock > 0`
- Business invariant: an author with books cannot be deleted

The entities (`Author`, `Book`) are **anemic** — they hold only data via public setters. The services own all the logic. Any future caller (a REST API, a background job) would have to duplicate those checks.

**Fix:** move all invariants into the entities (rich domain model). Services should only load, call, save.

### Problem 2 — No pagination on index pages

`BooksController.Index` and `AuthorsController.Index` call `GetAllAsync()` and pass the entire table to the view. With 27 books today, and growth ahead, this will degrade performance and UX.

**Fix:** add server-side pagination with a default page size of 10 items. The service queries only the current page via `Skip`/`Take`; the view renders a Bootstrap pager.

---

## GitHub Copilot features used in this module

### Ask mode

Ask is a **read-only conversation**. Copilot answers questions and proposes code snippets but **does not edit any file**. Use it to explore an unfamiliar codebase, verify assumptions, and plan a refactor before touching code.

> Reference: [Asking GitHub Copilot questions in your IDE](https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide)

### Plan mode

Plan is a **review-before-apply mode**. Copilot generates a step-by-step plan for the requested change and shows it for approval before making any edits. Use it when you want to inspect the approach before committing to a multi-file refactor.

> Reference: [Ask / Plan / Agent modes](https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide)

### Agent mode

Agent is an **autonomous execution mode**. Copilot reads files, makes edits, runs terminal commands (like `dotnet build`), reads the output, and iterates — all in a loop until the task is done. Use it for end-to-end implementation tasks.

> References: [Agent mode in VS Code](https://code.visualstudio.com/docs/copilot/agents/agents-tutorial) · [Plan mode in Copilot CLI](https://docs.github.com/copilot/how-tos/copilot-cli/cli-best-practices#plan-mode)

### Repository custom instructions (`copilot-instructions.md`)

A single Markdown file at `.github/copilot-instructions.md` that Copilot reads alongside **every prompt** in the repo. Use it to encode architecture conventions, naming rules, and build commands so you never have to repeat them.

> Reference: [Adding repository custom instructions for GitHub Copilot](https://docs.github.com/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot)

### File-specific instructions (`.instructions.md` with `applyTo`)

A Markdown file in `.github/instructions/` with a YAML front-matter `applyTo` glob. Copilot loads it **only when the matching files are in context**. Use it to scope pagination or styling conventions to controllers and views only.

> Reference: [Use custom instructions in VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-instructions)

---

## Solution layout (what is on the `initial` branch)

```
Src/
├── BookStore.slnx
├── BookStore.Domain/
│   ├── Entities/   Author.cs, Book.cs        (anemic — only properties)
│   ├── Data/       BookStoreContext.cs        (EF Core In-Memory)
│   └── Seed/       DataSeeder.cs             (10 authors + 27 books)
├── BookStore.Application/
│   └── Services/   AuthorService, BookService  (← domain logic lives here today)
└── BookStore.Web/
    ├── Controllers/  BooksController, AuthorsController, HomeController
    ├── Views/        Books/*, Authors/*, Home/*  (Bootstrap 5)
    └── Program.cs
```

**Run the app:**

```bash
cd Src/BookStore.Web
dotnet run
```

Or press **F5** in VS Code (launch config: *BookStore.Web (http)*). Open <http://localhost:5045>.

**Build the solution:**

```bash
cd Src
dotnet build BookStore.slnx
```

---

## Step 1 — Explore with Ask mode

Switch the Copilot Chat dropdown to **Ask**. We are not changing anything yet — just reading and validating our mental model.

### 1.1) Understand the project structure

> **Why:** confirm Copilot can read and summarize the repo before we ask it to change anything.

**Prompt:**

```text
Look at the workspace. This is a .NET 10 multi-project solution under Src/.
Tell me:
1. Which .NET version each project targets.
2. How to build the solution and run the web app from the terminal (macOS / zsh).
3. What URL the web app listens on by default.
Quote the exact files you used to answer.
```

Expected: .NET 10, `dotnet build Src/BookStore.slnx`, `dotnet run --project Src/BookStore.Web`, URL from `launchSettings.json`.

### 1.2) Identify domain logic leaking into the application layer

> **Why:** let Copilot find the evidence for Problem 1 so we know exactly what to move.

**Prompt:**

```text
#codebase Give me a short architectural overview of the solution under Src/:
- The role of each project (BookStore.Domain, BookStore.Application, BookStore.Web).
- Which classes hold business rules today and which only orchestrate.
- List every place where domain rules are leaking into the application layer — include file path and line number.
Be concise.
```

Copilot should point at `AuthorService.CreateAsync`, `BookService.CreateAsync`, `BookService.UpdateAsync`, and `AuthorService.DeleteAsync` as the problematic methods.

### 1.3) Confirm pagination is missing

> **Why:** validate Problem 2 before writing the fix instructions.

**Prompt:**

```text
#file:Src/BookStore.Web/Controllers/BooksController.cs
#file:Src/BookStore.Web/Controllers/AuthorsController.cs
#file:Src/BookStore.Application/Services/BookService.cs
#file:Src/BookStore.Application/Services/AuthorService.cs
Confirm whether the Index actions support pagination.
If not, describe what server-side pagination with page size 10 would look like end-to-end (service → controller → view).
Do not modify anything.
```

---

## Step 2 — Fix Problem 1: rich domain model (Agent mode + general instructions)

### 2.1) Create `.github/copilot-instructions.md`

This file teaches Copilot the architecture of this repo for every future prompt.

Path: `.github/copilot-instructions.md`

```markdown
# BookStore — Copilot repository instructions

## Solution layout
- .NET 10 multi-project solution under `Src/`.
  - `BookStore.Domain` — entities, `BookStoreContext` (EF Core In-Memory), seed data.
  - `BookStore.Application` — application services (orchestration only).
  - `BookStore.Web` — ASP.NET Core 10 MVC (controllers, Razor views, Bootstrap 5).
- No authentication.

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
- **Controllers** are thin: model binding → service call → view/redirect. No business logic.

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
```

### 2.2) Plan then apply with Agent mode

Switch to **Plan** mode first to review the approach, then switch to **Agent** to execute.

**Plan mode prompt** (review the plan, then switch to Agent):

```text
Following the architecture in .github/copilot-instructions.md, refactor the project so the domain layer owns its business rules.

Plan the following changes — do not make any edits yet:
1. Add `BookStore.Domain/Exceptions/DomainException.cs`.
2. Enrich `Author` and `Book` with private setters and domain methods that enforce invariants.
3. Strip all validation logic from `AuthorService` and `BookService` — services only load, call domain methods, and save.
4. Update controllers to catch `DomainException` (not `InvalidOperationException`) when displaying errors.
5. Verify `dotnet build Src/BookStore.slnx` stays green.
```

Review the plan. Once satisfied, switch to **Agent** and send the same prompt (or confirm with "Go ahead"):

**Agent mode prompt:**

```text
Following the architecture in .github/copilot-instructions.md, refactor the project so the domain layer owns its business rules.

Concretely:
1. Add `BookStore.Domain/Exceptions/DomainException.cs`.
2. Make `Author` and `Book` rich entities:
   - Private setters where appropriate.
   - Static factory `Create(...)` and instance methods (`Rename`, `Restock`, `ChangePrice`, `AssignAuthor`, ...) that enforce the invariants listed in .github/copilot-instructions.md.
   - `Book.IsAvailable` becomes a computed property derived from `Stock` (no setter needed).
3. Remove every validation from `AuthorService` and `BookService`. Services only: load → call domain method → save.
4. Update controllers to catch `DomainException` instead of `InvalidOperationException`.
5. Keep all public service method signatures unchanged.
6. Run `dotnet build Src/BookStore.slnx` and fix any errors before finishing.
```

**Expected outcome:**

- `BookStore.Domain/Exceptions/DomainException.cs` created.
- `Author.cs` and `Book.cs` have private setters and domain methods.
- `AuthorService` and `BookService` are significantly shorter — no validation logic.
- `dotnet build Src/BookStore.slnx` → 0 errors.

**Verify:**

```bash
dotnet run --project Src/BookStore.Web
```

Try creating a book with `Price = -1` at `/Books/Create`. The error should still appear — but now the rule is enforced inside `Book.Create(...)`, not inside `BookService`.

---

## Step 3 — Fix Problem 2: server-side pagination (Agent mode + file-specific instructions)

### 3.1) Create `.github/instructions/pagination.instructions.md`

This file applies only to controllers, views, and application services — not the whole repo.

Path: `.github/instructions/pagination.instructions.md`

````markdown
---
applyTo: "Src/BookStore.Web/Controllers/**/*.cs,Src/BookStore.Web/Views/**/*.cshtml,Src/BookStore.Application/Services/*.cs"
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
  - "Page X of Y · N items" indicator.
  - Links use `asp-action="Index"` + `asp-route-page="@i"`.

## What not to do
- Do not add a NuGet pagination package.
- Do not paginate on the client side.
- Only change Index actions — leave CRUD actions untouched.
````

### 3.2) Apply with Agent mode

**Prompt:**

```text
Add server-side pagination to the Books and Authors index pages, following .github/instructions/pagination.instructions.md.

Steps:
1. Create `BookStore.Application/Common/PagedResult.cs`.
2. Add `GetPagedAsync(int pageNumber, int pageSize)` to `IBookService` / `BookService` and `IAuthorService` / `AuthorService`. Default page size 10.
3. Update `BooksController.Index` and `AuthorsController.Index` to accept `int page = 1` and use the paged method.
4. Update `Views/Books/Index.cshtml` and `Views/Authors/Index.cshtml`: type `@model PagedResult<...>`, render `Model.Items`, add a Bootstrap 5 pager.
5. Run `dotnet build Src/BookStore.slnx` and fix any errors.

Do not touch the Domain layer.
```

**Expected outcome:**

- `BookStore.Application/Common/PagedResult.cs` exists.
- Both services expose `GetPagedAsync`.
- Both controllers' `Index` actions accept `page`.
- Both views render only the current page and show a Bootstrap pager.
- 27 books → 3 pages (10 + 10 + 7).

---

## Step 4 — Verify everything

```bash
dotnet run --project Src/BookStore.Web
```

| Check | Expected |
|---|---|
| `/Books` | Table has ≤ 10 rows. Pager shows *Page 1 of 3 · 27 items*. Next/Prev work. |
| `/Authors` | Pager shows *Page 1 of 1*. Previous and Next are disabled. |
| `/Books/Create` with `Price = -1` | Validation error shown. Rule enforced in `Book`, not in `BookService`. |
| `/Authors/Delete/1` (author with books) | Error: "Cannot delete an author who still has books." Now from entity. |

---

## Recap

| Step | Copilot feature | Purpose |
|---|---|---|
| Explore codebase | **Ask mode** | Read-only discovery, no file changes |
| Review refactor plan | **Plan mode** | Inspect approach before applying |
| Domain model refactor | **Agent mode** + `copilot-instructions.md` | Multi-file edits + build verification |
| Pagination implementation | **Agent mode** + `.instructions.md` (`applyTo`) | Scoped conventions for web layer |

---

## Wrap-up

Commit the result to `module/1`:

```bash
git checkout -b module/1
git add .
git commit -m "module/1: rich domain model + server-side pagination"
```

Module 2 will start from `module/1` and cover the next set of Copilot features.

---

## References

- What is GitHub Copilot? — <https://docs.github.com/en/copilot/get-started/what-is-github-copilot>
- GitHub Copilot plans — <https://docs.github.com/en/copilot/get-started/plans>
- Models and pricing — <https://docs.github.com/copilot/reference/copilot-billing/models-and-pricing>
- GitHub Copilot in VS Code — <https://code.visualstudio.com/docs/copilot/overview>
- Ask / Plan / Agent modes — <https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide>
- Agent mode in VS Code — <https://code.visualstudio.com/docs/copilot/agents/agents-tutorial>
- Plan mode in Copilot CLI — <https://docs.github.com/copilot/how-tos/copilot-cli/cli-best-practices#plan-mode>
- Repository custom instructions — <https://docs.github.com/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot>
- Custom instructions in VS Code — <https://code.visualstudio.com/docs/copilot/customization/custom-instructions>
- Custom instructions support matrix — <https://docs.github.com/en/copilot/reference/custom-instructions-support>
- Prompt files — <https://code.visualstudio.com/docs/copilot/customization/prompt-files>
- Agent Skills — <https://code.visualstudio.com/docs/copilot/customization/agent-skills>
- Copilot cloud agent — <https://code.visualstudio.com/docs/copilot/copilot-cloud-agent>
