# Module 2 - GitHub Copilot: Inline Suggestions & Inline Chat

**Goal:** use GitHub Copilot's lightweight editing features - inline suggestions and inline chat - alongside Ask and Agent modes to fix three design problems in a .NET 10 MVC application, choosing the right Copilot tool for each scope of change.

**Deliverable:** `BookStore.Web` refactored so that (1) the site title and branding read "Book Store" throughout, (2) domain rules live inside the entities, and (3) index pages have server-side pagination.

---

## Prerequisites

- VS Code (latest)
- GitHub Copilot + GitHub Copilot Chat extensions, signed in
- .NET 10 SDK

```bash
code --version
dotnet --version
```

**Repository checked out - `initial` branch**

```bash
git checkout initial
```

> If you completed Module 1 and are continuing from `feature/Module1`, only Problem 1 remains. Skip Steps 3 and 4 and use the wrap-up branch name `feature/Module2`.

---

## Problems to solve in this module

### Problem 1 - Unfriendly site title and branding

The browser tab and navbar show **"BookStore.Web"** - the internal project name. The title appears in three places inside `Src/BookStore.Web/Views/Shared/_Layout.cshtml`:

- `<title>@ViewData["Title"] - BookStore.Web</title>` (line 5)
- `<a class="navbar-brand" ...>BookStore.Web</a>` (line 16)
- `&copy; 2026 - BookStore.Web - ...` in the footer (line 55)

This matters because the title is what users see in the browser tab, in bookmarks, and in screen readers. A human-readable name ("Book Store") improves usability and branding.

**Fix:** replace every "BookStore.Web" occurrence in `_Layout.cshtml` with "Book Store".

### Problem 2 - Domain logic leaking into the application layer

`AuthorService` and `BookService` enforce rules that belong to the domain:

- Field validation: `Title required`, `Price >= 0`, `Stock >= 0`, `BirthDate` not in the future, etc.
- Derived state: `Book.IsAvailable = Stock > 0`
- Business invariant: an author with books cannot be deleted

The entities (`Author`, `Book`) are **anemic** - they hold only data via public setters. The services own all the logic. Any future caller (a REST API, a background job) would have to duplicate those checks.

**Fix:** move all invariants into the entities (rich domain model). Services should only load, call domain methods, and save.

### Problem 3 - No pagination on index pages

`BooksController.Index` and `AuthorsController.Index` call `GetAllAsync()` and pass the entire table to the view. With 27 books today, and growth ahead, this will degrade performance and UX.

**Fix:** add server-side pagination with a default page size of 10 items. The service queries only the current page via `Skip`/`Take`; the view renders a Bootstrap pager.

---

## GitHub Copilot features used in this module

### Ask mode

Ask is a **read-only conversation**. Copilot answers questions and proposes snippets but does not edit any file. Use it to locate exactly which files and lines need to change before touching anything.

> Reference: [Asking GitHub Copilot questions in your IDE](https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide)

### Inline suggestions (autocomplete / ghost text)

As you type, Copilot continuously predicts what you are about to write and shows the suggestion as dimmed "ghost text". Press `Tab` to accept the full suggestion or `Alt+]` / `Option+]` to cycle alternatives. This is the fastest way to apply small, predictable edits - like renaming a string that appears multiple times in one file.

> References: [Getting code suggestions in your IDE](https://docs.github.com/copilot/using-github-copilot/getting-code-suggestions-in-your-ide-with-github-copilot) - [Code suggestions concept](https://docs.github.com/en/copilot/concepts/completions/code-suggestions) - [AI-powered suggestions in VS Code](https://code.visualstudio.com/docs/editing/ai-powered-suggestions)

### Next edit suggestions

When you change one occurrence of a string or symbol, Copilot detects the pattern and shows ghost-text suggestions for the **next related edit** in the file - even before you navigate there. Accept with `Tab`. This is especially useful when the same value appears in multiple places within one file.

> Reference: [Configuring GitHub Copilot in your environment](https://docs.github.com/copilot/configuring-github-copilot/configuring-github-copilot-in-your-environment?tool=visualstudio)

### Inline chat

Inline chat opens a small chat input directly in the editor (`Ctrl+I` / `Cmd+I`). You describe the change in natural language and Copilot shows a diff preview before applying it. Use it for targeted, single-file edits where you want to describe the intent rather than type character-by-character.

> Reference: [Inline chat in VS Code](https://code.visualstudio.com/docs/chat/inline-chat)

### Plan mode

Plan is a **review-before-apply mode**. Copilot generates a step-by-step plan for the requested change and shows it for approval before making any edits. Use it for multi-file refactors where you want to validate the approach first.

> Reference: [Asking GitHub Copilot questions in your IDE](https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide)

### Agent mode

Agent is an **autonomous execution mode**. Copilot reads files, makes edits, runs terminal commands, reads the output, and iterates until the task is complete. Use it for end-to-end implementation tasks that span multiple files.

> Reference: [Agent mode in VS Code](https://code.visualstudio.com/docs/copilot/agents/agents-tutorial)

---

## Solution layout (what is on the `initial` branch)

```
Src/
├── BookStore.slnx
├── BookStore.Domain/
│   ├── Entities/   Author.cs, Book.cs        (anemic - only properties)
│   ├── Data/       BookStoreContext.cs        (EF Core In-Memory)
│   └── Seed/       DataSeeder.cs             (10 authors + 27 books)
├── BookStore.Application/
│   └── Services/   AuthorService, BookService  (domain logic lives here today)
└── BookStore.Web/
    ├── Controllers/  BooksController, AuthorsController, HomeController
    ├── Views/        Books/*, Authors/*, Home/*  (Bootstrap 5)
    │   └── Shared/   _Layout.cshtml             (title "BookStore.Web" - three places)
    └── Program.cs
```

**Run the app:**

```bash
cd Src/BookStore.Web
dotnet run
```

Or press **F5** in VS Code (launch config: *BookStore.Web (http)*). Open <http://localhost:5045>.

---

## Step 1 - Explore with Ask mode

Switch the Copilot Chat dropdown to **Ask**. No file changes yet - just discovering where the problems live.

### 1.1) Find all occurrences of "BookStore.Web" in the UI

> **Why:** pinpoint every file and line that shows the unfriendly name before touching anything.

**Prompt:**

```text
Search the workspace for every place where the string "BookStore.Web" is shown to users in the browser UI (title tag, navbar, footer, page headings).
List each occurrence with file path and line number.
Do not modify anything.
```

Expected: Copilot should identify three occurrences - all in `Src/BookStore.Web/Views/Shared/_Layout.cshtml`: the `<title>` tag (line 5), the `.navbar-brand` anchor (line 16), and the footer copyright line (line 55).

### 1.2) Identify domain logic leaking into the application layer

> **Why:** let Copilot map the problematic methods before we plan the refactor.

**Prompt:**

```text
#codebase Give me a short architectural overview of the solution under Src/:
- The role of each project (BookStore.Domain, BookStore.Application, BookStore.Web).
- Which classes hold business rules today and which only orchestrate.
- List every place where domain rules are leaking into the application layer - include file path and line number.
Be concise.
```

Copilot should point at `AuthorService.CreateAsync`, `BookService.CreateAsync`, `BookService.UpdateAsync`, and `AuthorService.DeleteAsync` as the problematic methods.

### 1.3) Confirm pagination is missing

**Prompt:**

```text
#file:Src/BookStore.Web/Controllers/BooksController.cs
#file:Src/BookStore.Web/Controllers/AuthorsController.cs
#file:Src/BookStore.Application/Services/BookService.cs
#file:Src/BookStore.Application/Services/AuthorService.cs
Confirm whether the Index actions support pagination.
If not, describe what server-side pagination with page size 10 would look like end-to-end (service -> controller -> view).
Do not modify anything.
```

---

## Step 2 - Fix Problem 1: site title and branding (inline suggestions)

This is a small, contained change in a single file - the ideal scenario for inline suggestions and next edit suggestions rather than Agent mode.

### 2.1) Open `_Layout.cshtml` and make the first change by hand

Open `Src/BookStore.Web/Views/Shared/_Layout.cshtml`.

Navigate to line 5:

```html
<title>@ViewData["Title"] - BookStore.Web</title>
```

Place the cursor after the `-` and **delete** "BookStore.Web". Start typing "Book Store". Copilot will show a ghost-text completion - press **Tab** to accept.

### 2.2) Accept next edit suggestions for the remaining occurrences

After accepting the first suggestion, Copilot's **next edit suggestions** feature detects the pattern and highlights the next occurrence of "BookStore.Web" in the file (the `.navbar-brand` text on line 16).

- Press **Tab** to accept the suggestion and jump to the next occurrence.
- Repeat for the footer copyright line (line 55).

All three occurrences are replaced without opening Chat or writing a prompt.

**Expected outcome:** every "BookStore.Web" in `_Layout.cshtml` reads "Book Store". The browser tab and navbar now show the friendly name.

### 2.3) (Alternative) Use inline chat for the same change

If next edit suggestions do not appear, use inline chat instead:

1. Select all the text in `_Layout.cshtml` (`Cmd+A` / `Ctrl+A`).
2. Open inline chat: `Cmd+I` / `Ctrl+I`.
3. Type the prompt and press Enter:

```text
Replace every occurrence of "BookStore.Web" with "Book Store".
```

Copilot shows a diff preview. Review it and click **Accept**.

**Verify:**

```bash
dotnet run --project Src/BookStore.Web
```

Open <http://localhost:5045>. The browser tab should read "Home Page - Book Store" and the navbar brand should say "Book Store".

---

## Step 3 - Fix Problem 2: rich domain model (Plan mode + Agent mode)

### 3.1) Create `.github/copilot-instructions.md`

This file teaches Copilot the architecture of this repo for every future prompt.

Path: `.github/copilot-instructions.md`

```markdown
# BookStore - Copilot repository instructions

## Solution layout
- .NET 10 multi-project solution under `Src/`.
  - `BookStore.Domain` - entities, `BookStoreContext` (EF Core In-Memory), seed data.
  - `BookStore.Application` - application services (orchestration only).
  - `BookStore.Web` - ASP.NET Core 10 MVC (controllers, Razor views, Bootstrap 5).
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
- **Controllers** are thin: model binding -> service call -> view/redirect. No business logic.

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

### 3.2) Plan mode - review the approach first

Switch to **Plan** mode. Paste the prompt, review the plan, then switch to **Agent** to execute.

**Plan mode prompt:**

```text
Following the architecture in .github/copilot-instructions.md, refactor the project so the domain layer owns its business rules.

Plan the following changes - do not make any edits yet:
1. Add `BookStore.Domain/Exceptions/DomainException.cs`.
2. Enrich `Author` and `Book` with private setters and domain methods that enforce invariants.
3. Strip all validation logic from `AuthorService` and `BookService` - services only load, call domain methods, and save.
4. Update controllers to catch `DomainException` (not `InvalidOperationException`) when displaying errors.
5. Verify `dotnet build Src/BookStore.slnx` stays green.
```

Review the plan. Once satisfied, switch to **Agent** and send the same prompt.

**Agent mode prompt:**

```text
Following the architecture in .github/copilot-instructions.md, refactor the project so the domain layer owns its business rules.

Concretely:
1. Add `BookStore.Domain/Exceptions/DomainException.cs`.
2. Make `Author` and `Book` rich entities:
   - Private setters where appropriate.
   - Static factory `Create(...)` and instance methods (`Rename`, `Restock`, `ChangePrice`, `AssignAuthor`, ...) that enforce the invariants listed in .github/copilot-instructions.md.
   - `Book.IsAvailable` becomes a computed property derived from `Stock` (no setter needed).
3. Remove every validation from `AuthorService` and `BookService`. Services only: load -> call domain method -> save.
4. Update controllers to catch `DomainException` instead of `InvalidOperationException`.
5. Keep all public service method signatures unchanged.
6. Run `dotnet build Src/BookStore.slnx` and fix any errors before finishing.
```

**Expected outcome:**

- `BookStore.Domain/Exceptions/DomainException.cs` created.
- `Author.cs` and `Book.cs` have private setters and domain methods.
- `AuthorService` and `BookService` are significantly shorter - no validation logic.
- `dotnet build Src/BookStore.slnx` returns 0 errors.

---

## Step 4 - Fix Problem 3: server-side pagination (Agent mode + file-specific instructions)

### 4.1) Create `.github/instructions/pagination.instructions.md`

Path: `.github/instructions/pagination.instructions.md`

````markdown
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
- `Index` actions accept `int page = 1` and pass it to the service.

## Razor views
- Index views typed `@model PagedResult<Entity>`.
- Render table rows from `Model.Items`.
- Bootstrap 5 pagination below the table:
  - "Previous" disabled when `!Model.HasPrevious`.
  - "Next" disabled when `!Model.HasNext`.
  - "Page X of Y - N items" indicator.
  - Links use `asp-action="Index"` + `asp-route-page="@i"`.

## What not to do
- Do not add a NuGet pagination package.
- Do not paginate on the client side.
- Only change Index actions - leave CRUD actions untouched.
````

### 4.2) Apply with Agent mode

**Prompt:**

```text
Add server-side pagination to the Books and Authors index pages, following .github/instructions/pagination.instructions.md.

Steps:
1. Create `BookStore.Application/Common/PagedResult.cs`.
2. Add `GetPagedAsync(int pageNumber, int pageSize)` to `IBookService` / `BookService` and `IAuthorService` / `AuthorService`. Default page size 10.
3. Update `BooksController.Index` and `AuthorsController.Index` to accept `int page = 1` and use the paged method.
4. Create a reusable pagination component in `Views/Shared/_Pagination.cshtml` (renders Previous/Next buttons, page indicator, disabled states per PagedResult properties). Update `Views/Books/Index.cshtml` and `Views/Authors/Index.cshtml`: type `@model PagedResult<...>`, render `Model.Items` in the table, and include the pagination component at the bottom.
5. Run `dotnet build Src/BookStore.slnx` and fix any errors.

Do not touch the Domain layer.
```

**Expected outcome:**

- `BookStore.Application/Common/PagedResult.cs` exists.
- Both services expose `GetPagedAsync`.
- Both controllers' `Index` actions accept `page`.
- Both index views render only the current page and show a Bootstrap pager.
- 27 books across 3 pages (10 + 10 + 7).

---

## Step 5 - Verify everything

```bash
dotnet run --project Src/BookStore.Web
```

| Check | Expected |
|---|---|
| Browser tab on Home | "Home Page - Book Store" |
| Navbar brand | "Book Store" |
| Footer copyright | "&copy; 2026 - Book Store - ..." |
| `/Books` | Table has <= 10 rows. Pager shows *Page 1 of 3 - 27 items*. Next/Prev work. |
| `/Authors` | Pager shows *Page 1 of 1*. Previous and Next are disabled. |
| `/Books/Create` with `Price = -1` | Validation error shown. Rule enforced in `Book`, not in `BookService`. |
| `/Authors/Delete/1` (author with books) | Error: "Cannot delete an author who still has books." Now from entity. |

---

## Recap

| Step | Copilot feature | Purpose |
|---|---|---|
| 1 - Explore codebase | **Ask mode** | Read-only discovery - locate all occurrences and confirm problems |
| 2 - Fix title | **Inline suggestions** + **next edit suggestions** | Accept ghost-text completions for quick single-file rename |
| 2 (alt) - Fix title | **Inline chat** | Describe the change in natural language, review diff, accept |
| 3 - Domain model refactor | **Plan mode** + **Agent mode** + `copilot-instructions.md` | Multi-file edits with review step and build verification |
| 4 - Pagination | **Agent mode** + `.instructions.md` (`applyTo`) | Scoped conventions for web layer + autonomous implementation |

---

## Wrap-up

Commit the result to `feature/Module2`:

```bash
git checkout -b feature/Module2
git add .
git commit -m "feature/Module2: friendly site title + rich domain model + server-side pagination"
```

---

## References

- GitHub Copilot overview - <https://docs.github.com/en/copilot/get-started/what-is-github-copilot>
- Asking GitHub Copilot questions in your IDE - <https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide>
- Getting code suggestions in your IDE - <https://docs.github.com/copilot/using-github-copilot/getting-code-suggestions-in-your-ide-with-github-copilot>
- Code suggestions concept - <https://docs.github.com/en/copilot/concepts/completions/code-suggestions>
- AI-powered suggestions in VS Code - <https://code.visualstudio.com/docs/editing/ai-powered-suggestions>
- Configuring GitHub Copilot in your environment (next edit suggestions) - <https://docs.github.com/copilot/configuring-github-copilot/configuring-github-copilot-in-your-environment?tool=visualstudio>
- Inline chat in VS Code - <https://code.visualstudio.com/docs/chat/inline-chat>
- Copilot Chat in VS Code - <https://code.visualstudio.com/docs/chat/copilot-chat>
- Agent mode in VS Code - <https://code.visualstudio.com/docs/copilot/agents/agents-tutorial>
- Repository custom instructions - <https://docs.github.com/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot>
- Custom instructions in VS Code - <https://code.visualstudio.com/docs/copilot/customization/custom-instructions>
