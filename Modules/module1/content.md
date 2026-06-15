# Module 1 - GitHub Copilot: Ask, Plan, Agent modes & custom instructions

**Goal:** use the three GitHub Copilot interaction modes, inline suggestions, inline chat, and custom instructions to fix three design problems in a .NET 10 MVC application - without writing a single line of code by hand.

**Deliverable:** `BookStore.Web` refactored so that (1) the site title and branding read "Book Store", (2) domain rules live inside the domain entities, and (3) the index pages have server-side pagination - all driven by Copilot with the right context.

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

✅ **Repository checked out - `initial` branch**

```bash
git checkout initial
```

---

## Problems to solve in this module

### Problem 1 - Unfriendly site title and branding

The browser tab and navbar show **"BookStore.Web"** - the internal project name. 

**Fix:** replace every "BookStore.Web" occurrence in the UI with "Book Store" using inline suggestions or inline chat.

### Problem 2 - Domain logic leaking into the application layer

`AuthorService` and `BookService` enforce rules that belong to the domain:

- Field validation: `Title required`, `Price >= 0`, `Stock >= 0`, `BirthDate` not in the future, etc.
- Derived state: `Book.IsAvailable = Stock > 0`
- Business invariant: an author with books cannot be deleted

The entities (`Author`, `Book`) are **anemic** - they hold only data via public setters. The services own all the logic. Any future caller (a REST API, a background job) would have to duplicate those checks.

**Fix:** move all invariants into the entities (rich domain model). Services should only load, call, save.

### Problem 3 - No pagination on index pages

`BooksController.Index` and `AuthorsController.Index` call `GetAllAsync()` and pass the entire table to the view. With 27 books today, and growth ahead, this will degrade performance and UX.

**Fix:** add server-side pagination with a default page size of 10 items. The service queries only the current page via `Skip`/`Take`; the view renders a Bootstrap pager.

---

## GitHub Copilot features used in this module

### Ask mode

Ask is a **read-only conversation**. Copilot answers questions and proposes code snippets but **does not edit any file**. Use it to explore an unfamiliar codebase, verify assumptions, and plan a refactor before touching code.

> Reference: [Asking GitHub Copilot questions in your IDE](https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide)

### Inline suggestions (autocomplete / ghost text)

As you type, Copilot continuously predicts what you are about to write and shows the suggestion as dimmed "ghost text". Press `Tab` to accept the full suggestion or `Option+]` / `Alt+]` to cycle alternatives. This is the fastest way to apply small, predictable edits - like renaming a string that appears multiple times in one file.

> References: [Getting code suggestions in your IDE](https://docs.github.com/copilot/using-github-copilot/getting-code-suggestions-in-your-ide-with-github-copilot) - [Code suggestions concept](https://docs.github.com/en/copilot/concepts/completions/code-suggestions) - [AI-powered suggestions in VS Code](https://code.visualstudio.com/docs/editing/ai-powered-suggestions)

### Inline chat

Inline chat opens a small chat input directly in the editor (`Cmd+I` / `Ctrl+I`). You describe the change in natural language and Copilot shows a diff preview before applying it. Use it for targeted, single-file edits where you want to describe the intent rather than type character-by-character.

> Reference: [Inline chat in VS Code](https://code.visualstudio.com/docs/chat/inline-chat)

### Plan mode

Plan is a **review-before-apply mode**. Copilot generates a step-by-step plan for the requested change and shows it for approval before making any edits. Use it when you want to inspect the approach before committing to a multi-file refactor.

> Reference: [Ask / Plan / Agent modes](https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide)

### Agent mode

Agent is an **autonomous execution mode**. Copilot reads files, makes edits, runs terminal commands (like `dotnet build`), reads the output, and iterates - all in a loop until the task is done. Use it for end-to-end implementation tasks.

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
│   ├── Entities/   Author.cs, Book.cs        (anemic - only properties)
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

## Step 0 - Understand the project (Ask mode only)

Before fixing any problems, let's use Ask mode to explore and understand the codebase. This discovery phase ensures everyone on the team has the same mental model of the project structure, data flow, and current design.

### 0.1) Project structure and .NET version

Switch the Copilot Chat dropdown to **Ask**. This is read-only - no edits will be made.

**Prompt:**

```text
Look at the workspace. This is a .NET multi-project solution under Src/.
Tell me:
1. Which .NET version each project targets.
2. The purpose of each project (BookStore.Domain, BookStore.Application, BookStore.Web).
3. How to build the solution and run the web app from the terminal (macOS / zsh).
4. What URL the web app listens on by default.
Quote the exact files you used to answer each question.
```

**Expected outcome:** Copilot confirms .NET 10 for all projects, the three-layer architecture (Domain/Application/Web), build and run commands, and the default URL.

### 0.2) Database and seed data

**Prompt:**

```text
Describe the database setup in BookStore.Domain:
1. What database provider is used (SQL Server, SQLite, In-Memory, etc.)?
2. How many authors and books are seeded on startup?
3. Which files handle the data context and seeding?
```

**Expected outcome:** Copilot identifies EF Core In-Memory provider, 10 authors + 27 books, and the seeding logic in `DataSeeder.cs`.

### 0.3) Entity models and relationships

**Prompt:**

```text
Describe the domain entities in BookStore.Domain.Entities:
1. What are all the properties of Author and Book?
2. What is the relationship between Author and Book (one-to-many, many-to-many, etc.)?
3. Do these entities have any domain methods (other than simple getters/setters) - list their names.
4. Which properties have public setters, and which are read-only?
```

**Expected outcome:** Copilot maps the entities, shows the one-to-many relationship (Author has many Books), lists all properties, confirms no domain methods exist today, and notes that all properties have public setters (anemic model).

### 0.4) Application services

**Prompt:**

```text
Describe the application services in BookStore.Application.Services:
1. What are all the public methods in BookService and AuthorService (create, read, update, delete, list)?
2. Where is validation logic performed (inside CreateAsync, UpdateAsync, DeleteAsync)? List the exact business rules being enforced.
3. Does either service support pagination?
```

**Expected outcome:** Copilot maps both services, shows validation logic embedded in create/update/delete methods (e.g., "Price >= 0", "Stock >= 0", "Author has books"), confirms no pagination exists.

### 0.5) Controllers and views

**Prompt:**

```text
Describe the web layer in BookStore.Web:
1. What are the Index action signatures in BooksController and AuthorsController?
2. Which service method do they call, and what do they pass to the view?
3. In the shared layout file, where does the internal project name "BookStore.Web" appear (title, navbar, footer, etc.)?
```

**Expected outcome:** Both Index actions call `GetAllAsync()` and pass the full dataset to the view. No pagination. The project name appears in title, navbar brand, and footer of `_Layout.cshtml`.

### 0.6) Application entry point

**Prompt:**

```text
Describe how the BookStore.Web application is configured:
1. How is the dependency injection container set up, and which service interfaces are registered?
2. What is the default launch profile name, and what port does the app listen on?
3. Are there any configuration files (appsettings, launchSettings) - what are the key settings?
```

**Expected outcome:** Copilot confirms DI setup, service registrations, launch profile name ("http"), and port 5045.

### 0.7) Solution file structure

**Prompt:**

```text
What is #file:BookStore.slnx  and what is its purpose in a .NET project?
List all the project files it references and the folder hierarchy of the solution.
```

**Expected outcome:** Copilot explains that `.slnx` is the modern Visual Studio solution format (used in .NET 10), and lists the three projects it contains: BookStore.Domain, BookStore.Application, and BookStore.Web under the `Src/` folder.

---

## Step 1 - Problem 1: Unfriendly site title and branding

### 1.1) Discover where "BookStore.Web" appears (Ask mode)

Switch the Copilot Chat dropdown to **Ask**. No file changes yet.

**Prompt:**

```text
Search the workspace for every place where the string "BookStore.Web" or other internal project names, like "BookStore", is shown to users in the browser UI (title tag, navbar, footer, page headings).
List each occurrence with file path and line number.
Do not modify anything.
```

**Expected outcome:** Copilot identifies the locations in solution files where the internal project name appears.

### 1.2) Fix using inline suggestions

This is a small, contained change in a single file - the ideal scenario for inline suggestions.

Open `Src/BookStore.Web/Views/Shared/_Layout.cshtml` and navigate to line 5:

```html
<title>@ViewData["Title"] - BookStore.Web</title>
```

Place the cursor after the `-` and **delete** "BookStore.Web". Start typing "Book Store". Copilot will show a ghost-text completion - press **Tab** to accept.

Other suggestions is use inline chat instead:
1. Select all the text in `_Layout.cshtml` (`Cmd+A` / `Ctrl+A`).
2. Open inline chat: `Cmd+I` / `Ctrl+I`.
3. Paste and press Enter:

```text
Replace every occurrence of label "BookStore.Web" with "Book Store".
```

Review the diff and click **Accept**.

**Verify:**

```bash
dotnet run --project Src/BookStore.Web
```

Open <http://localhost:5045>. The browser tab should read "Home Page - Book Store" and the navbar brand should say "Book Store".

---

## Step 2 - Problem 2: Domain logic leaking into the application layer

### 2.1) Discover the problem (Ask mode)

Switch to **Ask** mode.

**Prompt:**

```text
#codebase Give me a short architectural overview of the solution under Src/:
- The role of each project (BookStore.Domain, BookStore.Application, BookStore.Web).
- Which classes hold business rules today and which only orchestrate.
- List every place where domain rules are leaking into the application layer - include file path and line number.
Be concise.
```

**Expected outcome:** Copilot points at `AuthorService.CreateAsync`, `BookService.CreateAsync`, `BookService.UpdateAsync`, and `AuthorService.DeleteAsync` as the problematic methods where validation logic belongs to the domain layer.

### 2.2) Fix using Plan mode + Agent mode

#### Create `.github/copilot-instructions.md`

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
```

#### Review with Plan mode

Switch to **Plan** mode to review the refactoring approach before applying.

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

Review the plan. Once satisfied, proceed to Agent mode.

#### Execute with Agent mode

Switch to **Agent** and send the same prompt to execute:

**Agent mode prompt:**

```text
Following the architecture in .github/copilot-instructions.md, refactor the project so the domain layer owns its business rules.

Concretely:
1. Add `BookStore.Domain/Exceptions/DomainException.cs`.
2. Make `Author` and `Book` rich entities:
   - Private setters where appropriate.
   - Static factory `Create(...)` and instance methods (`Rename`, `Restock`, `ChangePrice`, `AssignAuthor`, ...) that enforce the invariants listed in .github/copilot-instructions.md.
   - `Book.IsAvailable` becomes a computed property derived from `Stock` (no setter needed).
3. Remove every validation from `AuthorService` and `BookService`. Services only: load - call domain method - save.
4. Update controllers to catch `DomainException` instead of `InvalidOperationException`.
5. Keep all public service method signatures unchanged.
6. Run `dotnet build Src/BookStore.slnx` and fix any errors before finishing.
```

**Expected outcome:**

- `BookStore.Domain/Exceptions/DomainException.cs` created.
- `Author.cs` and `Book.cs` have private setters and domain methods.
- `AuthorService` and `BookService` are significantly shorter - no validation logic.
- `dotnet build Src/BookStore.slnx` returns 0 errors.

**Verify:**

```bash
dotnet run --project Src/BookStore.Web
```

Try creating a book with `Price = -1` at `/Books/Create`. The error should still appear - but now the rule is enforced inside `Book.Create(...)`, not inside `BookService`.

---

## Step 3 - Problem 3: No pagination on index pages

### 3.1) Discover the problem (Ask mode)

Switch to **Ask** mode.

**Prompt:**

```text
Look at the Index actions in the controllers and the service methods they call.
Confirm whether pagination is currently supported.
If not, describe what server-side pagination with a page size of 10 would look like end-to-end (service - controller - view).
Do not modify anything.
```

**Expected outcome:** Copilot confirms that both `Index` actions call `GetAllAsync()` and pass the entire dataset to the view. No pagination support exists.

### 3.2) Fix using Agent mode + file-specific instructions

#### Create `.github/instructions/pagination.instructions.md`

This file applies only to controllers, views, and application services - not the whole repo.

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

#### Execute with Agent mode

Switch to **Agent** and run this prompt:

**Agent mode prompt:**

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
- Both views render only the current page and show a Bootstrap pager.
- 27 books across 3 pages (10 + 10 + 7).

---

## Step 4 - Verify everything

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

| Step | Purpose | Copilot Mode | Outcome |
|---|---|---|---|
| **0** - Explore codebase | Understand project structure, entities, services, seed data, build/run commands | **Ask mode** (6 prompts) | Team alignment on architecture, no files changed |
| **1** - Site title | Locate and replace "BookStore.Web" with "Book Store" | **Ask mode** + **Inline suggestions** + **Inline chat** | User-friendly branding in browser tab, navbar, footer |
| **2** - Domain model | Move validation from services to entity methods | **Ask mode** + **Plan mode** + **Agent mode** + `copilot-instructions.md` | Rich entities, anemic services, `DomainException` introduced |
| **3** - Pagination | Add server-side paging to index pages | **Ask mode** + **Agent mode** + `.instructions.md` | Books and Authors paginate with page size 10 |
| **4** - Verify | Manual testing of all three fixes | Manual testing | All fixes verified working end-to-end |

---

## Wrap-up

Commit the result to `feature/Module1`:

```bash
git checkout -b feature/Module1
git add .
git commit -m "feature/Module1: site branding + rich domain model + server-side pagination"
```

---

## References

- What is GitHub Copilot? - <https://docs.github.com/en/copilot/get-started/what-is-github-copilot>
- GitHub Copilot plans - <https://docs.github.com/en/copilot/get-started/plans>
- Models and pricing - <https://docs.github.com/copilot/reference/copilot-billing/models-and-pricing>
- GitHub Copilot in VS Code - <https://code.visualstudio.com/docs/copilot/overview>
- Ask / Plan / Agent modes - <https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide>
- Agent mode in VS Code - <https://code.visualstudio.com/docs/copilot/agents/agents-tutorial>
- Plan mode in Copilot CLI - <https://docs.github.com/copilot/how-tos/copilot-cli/cli-best-practices#plan-mode>
- Getting code suggestions in your IDE - <https://docs.github.com/copilot/using-github-copilot/getting-code-suggestions-in-your-ide-with-github-copilot>
- Code suggestions concept - <https://docs.github.com/en/copilot/concepts/completions/code-suggestions>
- AI-powered suggestions in VS Code - <https://code.visualstudio.com/docs/editing/ai-powered-suggestions>
- Inline chat in VS Code - <https://code.visualstudio.com/docs/chat/inline-chat>
- Repository custom instructions - <https://docs.github.com/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot>
- Custom instructions in VS Code - <https://code.visualstudio.com/docs/copilot/customization/custom-instructions>
- Custom instructions support matrix - <https://docs.github.com/en/copilot/reference/custom-instructions-support>
- Prompt files - <https://code.visualstudio.com/docs/copilot/customization/prompt-files>
- Agent Skills - <https://code.visualstudio.com/docs/copilot/customization/agent-skills>
- Copilot cloud agent - <https://code.visualstudio.com/docs/copilot/copilot-cloud-agent>
