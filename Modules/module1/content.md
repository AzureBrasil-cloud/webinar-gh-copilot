# Module 1 — GitHub Copilot fundamentals: Ask mode, custom instructions, Agent mode

**Goal of this module:** master the **fundamental ways of interacting with GitHub Copilot in VS Code** by applying them to a real .NET 10 MVC project. We will use **Ask mode**, **general repository instructions**, **file‑specific instructions**, and **Agent mode** to refactor a small BookStore application that has two known design issues.

**Deliverable:** the `BookStore.Web` MVC app refactored so that (1) domain rules live in the domain layer and (2) the Razor index pages use server‑side pagination — all done by guiding Copilot with the right context.

---

## Prerequisites

✅ **Tools installed:**

- VS Code (latest)
- GitHub Copilot + GitHub Copilot Chat extensions, signed in
- .NET 10 SDK

```bash
code --version
dotnet --version
```

✅ **Repository checked out and base branch loaded:**

```bash
git checkout initial
```

The `initial` branch contains the base BookStore.Web project. Each next branch (`module/1`, `module/2`, …) is built **on top of the previous one**.

---

## What we will learn

1. **What GitHub Copilot is** — completions, Chat, plans, models, pricing
2. **Copilot in VS Code** — Ask / Edit / Agent modes and how context is built
3. **Custom instructions** — repository‑wide and file‑scoped
4. **A practical workflow** — explore with Ask, codify rules with instructions, execute with Agent
5. **Two refactors driven by Copilot:**
   - Move domain rules from the application layer into the domain layer
   - Add server‑side pagination (10 items per page) to Razor index pages

---

## 1) GitHub Copilot in 5 minutes

### 1.1) What is GitHub Copilot?

GitHub Copilot is an **AI pair programmer**. It offers two complementary surfaces:

- **Code completions** — inline suggestions while you type.
- **Chat** — a conversation panel with three modes: **Ask**, **Edit**, **Agent**.

Copilot is **context‑driven**: the quality of every suggestion depends on what it can “see” when you ask. In VS Code, that context includes the open file, the current selection, attached files (`#file`), workspace symbols (`#sym`), terminal output, problems, plus any **custom instructions** present in the repo or user settings.

> Reference: *What is GitHub Copilot?* — <https://docs.github.com/en/copilot/get-started/what-is-github-copilot>

### 1.2) Plans (high‑level)

| Plan | Who it is for |
|---|---|
| **Copilot Free** | Anyone with a GitHub account — limited completions and chat |
| **Copilot Pro** | Individual developers — full completions and chat, premium models |
| **Copilot Pro+** | Power users — higher premium‑request quotas |
| **Copilot Business** | Organizations — admin policies, audit |
| **Copilot Enterprise** | Enterprises — knowledge bases, custom models, SSO |

Each plan includes a budget of **premium model requests** (Claude, GPT‑4‑class, Gemini, …). When the quota for premium models runs out, the included base model still works.

> References: *GitHub Copilot plans* and *Models and pricing* — <https://docs.github.com/en/copilot/get-started/plans> · <https://docs.github.com/copilot/reference/copilot-billing/models-and-pricing>

### 1.3) Copilot in VS Code — three chat modes

| Mode | What it does | Typical use |
|---|---|---|
| **Ask** | Read‑only conversation. Copilot answers questions and proposes code, but **does not edit files**. | Understand a codebase, plan a refactor, ask “how do I run this?” |
| **Edit** | Copilot proposes **multi‑file edits** that you accept/reject hunk by hunk. | Targeted changes when you know which files are affected. |
| **Agent** | Copilot can **read, edit, run terminal commands and tools** in a loop until the task is done. | End‑to‑end implementation of a task. |

Switch modes from the dropdown at the top of the Chat view. The model is also chosen per request.

> References: *Copilot in VS Code* and *chat / edit modes* — <https://code.visualstudio.com/docs/copilot/overview> · <https://code.visualstudio.com/docs/copilot/chat/copilot-edits>

### 1.4) How context works (and how to control it)

Copilot builds a context window from many sources. The most important ones you can control directly:

- **The active editor and selection** — always included.
- **`#file:<path>`** — attach a specific file to the prompt.
- **`#folder:<path>`, `#codebase`** — attach broader context.
- **`#problems`, `#terminalLastCommand`** — feed in diagnostics or recent terminal output.
- **Custom instructions** — see section 2.

Rule of thumb: **say what, point at where, and let Copilot read.** Don’t paste large code blocks into chat — attach files instead.

### 1.5) Custom instructions in one slide

Custom instructions are **plain Markdown files** that Copilot reads alongside every prompt. Two flavors:

| Kind | File | Loaded when |
|---|---|---|
| **Repository instructions** (general) | `.github/copilot-instructions.md` | Always, for every request in this repo |
| **Path‑scoped instructions** | `.github/instructions/*.instructions.md` with `applyTo` glob | Only when matching files are in context |

There are also **prompt files** (`.github/prompts/*.prompt.md`) for reusable prompts and **agent skills** for advanced agent workflows — out of scope for this module.

> References: *Custom instructions for GitHub Copilot* and *Custom instructions in VS Code* — <https://docs.github.com/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot> · <https://code.visualstudio.com/docs/copilot/customization/custom-instructions>

---

## 2) The starting point — what's on the `initial` branch

The repository contains a small ASP.NET Core 10 MVC project under `Src/BookStore.Web` with:

```
Src/BookStore.Web/
├── Domain/
│   ├── Entities/        Author.cs, Book.cs        (anemic — only properties)
│   ├── Data/            BookStoreContext.cs       (EF Core In‑Memory)
│   └── Seed/            DataSeeder.cs             (10 authors + 27 books)
├── Application/
│   └── Services/        IAuthorService / AuthorService
│                        IBookService  / BookService
├── Controllers/         BooksController, AuthorsController, HomeController
├── Views/               Books/*, Authors/*, Home/*  (Bootstrap 5)
└── Program.cs           DI + DbContext + seed
```

Run it locally:

```bash
cd Src/BookStore.Web
dotnet run
```

Open <http://localhost:5099> (or the URL printed in the terminal). You'll find a **Books** and an **Authors** page with full CRUD.

### 2.1) Two design issues to fix in this module

**Problem 1 — Domain logic is in the application layer.**
`AuthorService` and `BookService` are doing things that should be domain rules:

- Trimming and validating fields (`Title required`, `Price >= 0`, `BirthDate <= today`, …)
- Computing `Book.IsAvailable = Stock > 0`
- Enforcing “an author with books cannot be deleted”

The entities (`Author`, `Book`) are anemic — they only have setters. The services know **everything** about the rules. Any new caller (a future API, a CLI, a worker) would have to re‑implement the same checks.

**Problem 2 — No pagination.**
`BooksController.Index` and `AuthorsController.Index` call `GetAllAsync()` and dump the whole table into the view. The catalog already has 27 books and will grow. We will add **server‑side pagination with a page size of 10**.

---

## 3) Step 1 — Use **Ask mode** to understand the project

We start in **Ask mode** because we don't want any edits yet. We just want Copilot to *read* and *explain*.

### 3.1) Open Copilot Chat → switch the mode dropdown to **Ask**

### 3.2) Prompt — “How do I run this?”

> **Why this prompt:** validate that Copilot understands the repository layout and gives back the correct run command. This also implicitly indexes the workspace into context.

**Prompt to type:**

```text
Look at the workspace. This is a .NET MVC project under Src/BookStore.Web.
Tell me:
1. Which .NET version it targets.
2. How to build and run it from the terminal (macOS / zsh).
3. What URL it will be served on by default.
Quote the exact files you used to answer.
```

Expected answer: .NET 10, `dotnet run --project Src/BookStore.Web`, default URL from `Properties/launchSettings.json`.

### 3.3) Prompt — “Explain the architecture”

> **Why this prompt:** make Copilot summarize the layers in its own words. If its summary matches our mental model, we know its context is good and we can trust the next steps.

**Prompt to type:**

```text
#codebase Give me a short architectural overview of Src/BookStore.Web:
- The role of each folder under Domain/, Application/, Controllers/, Views/.
- Which classes hold business rules today, and which classes only orchestrate.
- Any place where domain rules are leaking into the application layer — list each occurrence with file path and line number.
Be concise.
```

This is the moment Copilot should point at `AuthorService.CreateAsync`, `BookService.CreateAsync/UpdateAsync` etc. as the “leaks”. We use that list as the input for Step 2.

### 3.4) Prompt — “Where is pagination missing?”

**Prompt to type:**

```text
#file:Src/BookStore.Web/Controllers/BooksController.cs
#file:Src/BookStore.Web/Controllers/AuthorsController.cs
#file:Src/BookStore.Web/Application/Services/BookService.cs
#file:Src/BookStore.Web/Application/Services/AuthorService.cs
Confirm whether the Index actions support pagination. If not, describe what server-side pagination with page size 10 would look like end-to-end (service → controller → view).
Do not modify anything.
```

Now we have a confirmed plan. Time to write it down for Copilot.

---

## 4) Step 2 — **General repository instructions** to fix Problem 1

We will write a `.github/copilot-instructions.md` that tells Copilot, **for every prompt in this repo**, how this codebase is supposed to be layered. Then we'll ask Agent mode to apply it.

### 4.1) Create `.github/copilot-instructions.md`

Path: `.github/copilot-instructions.md`

Content:

```markdown
# BookStore.Web — Copilot repository instructions

## Project
- ASP.NET Core 10 MVC application under `Src/BookStore.Web`.
- Persistence: Entity Framework Core In-Memory (no migrations, seeded at startup).
- UI: Razor views with Bootstrap 5 (already included via the default MVC template).
- No authentication.

## Architecture rules
- **Domain layer** (`Domain/`) owns business rules.
  - Entities (`Author`, `Book`) are **rich**: required state is set through constructors or domain methods, not through public setters used directly by callers.
  - Invariants (e.g. "Title is required", "Price >= 0", "Stock >= 0", "Published date not in the future", "BirthDate not in the future", "Author with books cannot be deleted", "IsAvailable = Stock > 0") live on the entities — not in services.
  - Throw `DomainException` (in `Domain/Exceptions`) when an invariant is violated. Do not throw `InvalidOperationException` from entities.
- **Application layer** (`Application/`) only orchestrates:
  - Loads aggregates via `BookStoreContext`.
  - Calls domain methods on the entities.
  - Persists changes (`SaveChangesAsync`).
  - Translates `DomainException` to whatever the caller expects, but does not duplicate the rules.
- **Controllers** stay thin: model binding → call service → return view/redirect. No business logic.

## Code style
- C# 12, nullable enabled, file-scoped namespaces, `var` when the type is obvious.
- Use `async`/`await` end-to-end for any DB call.
- Prefer expression-bodied members for trivial getters.
- Public domain methods are verbs (`UpdateStock`, `Rename`, `MoveTo`...).

## Tests / build
- Build with `dotnet build Src/BookStore.Web`.
- Run with `dotnet run --project Src/BookStore.Web`.
- Do not introduce a database other than EF Core In-Memory.

## When you are unsure
- Read the file you are editing first. Do not invent class names.
- Do not generate migrations or change the persistence strategy.
- Keep changes minimal and focused on the requested task.
```

> Reference: *Repository custom instructions* — <https://docs.github.com/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot>

### 4.2) Switch to **Agent mode** and ask for the refactor

> **Why Agent mode:** the change spans entities, exceptions, services and controllers. Agent mode can read, edit and run `dotnet build` itself.

**Prompt to type:**

```text
Refactor the project so that the domain layer owns its business rules, following the architecture in .github/copilot-instructions.md.

Concretely:
1. Add a `Domain/Exceptions/DomainException.cs`.
2. Make `Author` and `Book` rich entities:
   - Private setters where appropriate.
   - Static factory `Create(...)` and instance methods (`Update`, `Rename`, `Restock`, `ChangePrice`, `AssignTo`, …) that enforce the invariants listed in the instructions.
   - `Book.IsAvailable` becomes a computed property derived from `Stock`.
3. Move every validation currently inside `AuthorService` and `BookService` into the corresponding entity. Services must only:
   - Load entities, call domain methods, save.
   - Catch `DomainException` only at the boundary if needed; otherwise let it propagate.
4. Update the controllers so they catch `DomainException` (instead of `InvalidOperationException`) when surfacing errors to the model state.
5. Keep all public service signatures the same so views and controllers compile without further changes.
6. Run `dotnet build Src/BookStore.Web` and fix any error you introduce.

Do not touch pagination or views in this step.
```

When Agent mode finishes, review the diff and accept it. The expected outcome is:

- `Domain/Exceptions/DomainException.cs` exists.
- `Author.cs` and `Book.cs` have private setters + domain methods.
- `AuthorService` and `BookService` shrink dramatically — they only call EF and entity methods.
- `dotnet build` is green.

Smoke‑test:

```bash
dotnet run --project Src/BookStore.Web
```

Open `/Books`, create a book with `Price = -1`. You should still see the same error in the UI — but the rule is now enforced inside `Book`, not inside `BookService`.

---

## 5) Step 3 — **File‑specific instructions** to fix Problem 2

For pagination we want **rules that only apply to controllers and Razor views**, not to the whole codebase. That's what `applyTo` is for.

### 5.1) Create `.github/instructions/web-pagination.instructions.md`

Path: `.github/instructions/web-pagination.instructions.md`

Content:

````markdown
---
applyTo: "Src/BookStore.Web/Controllers/**/*.cs,Src/BookStore.Web/Views/**/*.cshtml,Src/BookStore.Web/Application/Services/*.cs"
---

# Pagination conventions for BookStore.Web

Whenever you implement or modify a list/index page in `BookStore.Web`:

## Backend
- Add a small generic `PagedResult<T>` type under `Application/Common/PagedResult.cs`:
  - `Items` (IReadOnlyList<T>), `PageNumber` (int, 1-based), `PageSize` (int), `TotalItems` (int).
  - Computed `TotalPages`, `HasPrevious`, `HasNext`.
- Service methods used by Index actions must expose a paged variant, e.g.
  `Task<PagedResult<Book>> GetPagedAsync(int pageNumber, int pageSize)`.
  - Default `pageSize` = **10**.
  - Clamp `pageNumber` to `>= 1` and `pageSize` to `1..100`.
  - The query must use `Skip((pageNumber - 1) * pageSize).Take(pageSize)` **before** materializing.
  - Always return `TotalItems` from a `CountAsync()` over the same filter.
- Controllers' `Index` actions accept `int page = 1` and pass it to the service.
- Do **not** load the full table into memory just to count or paginate.

## Razor views
- Index views are typed `@model PagedResult<Entity>`.
- Render the table using `Model.Items`.
- Add a Bootstrap 5 pagination component below the table:
  - "Previous" link disabled when `!Model.HasPrevious`.
  - "Next" link disabled when `!Model.HasNext`.
  - A "Page X of Y · N items" indicator.
  - Each link must use `asp-action="Index"` and `asp-route-page="@i"`.
- Keep existing styling (Bootstrap classes already used in the project).

## What not to do
- Do not introduce a new pagination NuGet package.
- Do not paginate on the client side.
- Do not change CRUD actions, only Index.
````

> Reference: *Custom instructions in VS Code (path‑scoped via `applyTo`)* — <https://code.visualstudio.com/docs/copilot/customization/custom-instructions>

### 5.2) Apply the instructions with Agent mode

**Prompt to type:**

```text
Add server-side pagination to the Books and Authors index pages, following .github/instructions/web-pagination.instructions.md.

Steps:
1. Create `Application/Common/PagedResult<T>`.
2. Add `GetPagedAsync(int pageNumber, int pageSize)` to `IBookService` / `BookService` and `IAuthorService` / `AuthorService`. Default page size is 10.
3. Update `BooksController.Index` and `AuthorsController.Index` to accept `int page = 1` and use the paged service method.
4. Update `Views/Books/Index.cshtml` and `Views/Authors/Index.cshtml` to be typed `@model PagedResult<...>`, render `Model.Items`, and include a Bootstrap 5 pager.
5. Run `dotnet build Src/BookStore.Web`. Fix any errors.

Do not change anything in the Domain layer.
```

Expected outcome:

- `Application/Common/PagedResult.cs` exists.
- Both services expose `GetPagedAsync`.
- Both controllers' `Index` accept `page` and call the paged method.
- Both views render only the current page and show a working pager at the bottom.
- The catalog of 27 books spans 3 pages (10 + 10 + 7).

---

## 6) Step 4 — Run the project and verify

```bash
dotnet run --project Src/BookStore.Web
```

Manual checks:

1. **`/Books`** loads. The table has at most **10 rows**. The pager shows *Page 1 of 3 · 27 items*. Clicking *Next* goes to page 2, then page 3.
2. **`/Authors`** loads. With 10 authors, the pager shows *Page 1 of 1*; **Previous** and **Next** are disabled.
3. **`/Books/Create`** with `Price = -1` shows the validation error — proving the rule lives in `Book`, not in `BookService`.
4. **`/Authors/Delete/1`** for an author who still has books shows the “Cannot delete an author who still has books” error — now thrown from the entity.

If all four pass, Module 1 is done.

---

## 7) Recap — which Copilot feature solved what

| Step | Copilot feature | What it gave us |
|---|---|---|
| Understand the codebase | **Ask mode** | Read‑only exploration, plan validation |
| Encode architecture rules everywhere | **`copilot-instructions.md`** (general) | Layer separation enforced for every prompt |
| Encode rules only for web layer | **`*.instructions.md` with `applyTo`** | Pagination conventions scoped to controllers/views/services |
| Execute multi‑file refactors | **Agent mode** | Edits + `dotnet build` in a loop |

---

## 8) Cleanup / wrap‑up

- Commit the result on the `module/1` branch:

  ```bash
  git checkout -b module/1
  git add .
  git commit -m "module/1: domain-driven rules + paginated index pages"
  ```

- Module 2 will start from `module/1` as its source branch and add the next set of Copilot features (prompt files, agent skills, …).

---

## References

- What is GitHub Copilot? — <https://docs.github.com/en/copilot/get-started/what-is-github-copilot>
- GitHub Copilot plans — <https://docs.github.com/en/copilot/get-started/plans>
- Models and pricing — <https://docs.github.com/copilot/reference/copilot-billing/models-and-pricing>
- GitHub Copilot in VS Code — <https://code.visualstudio.com/docs/copilot/overview>
- Get started with Copilot in VS Code — <https://code.visualstudio.com/docs/copilot/getting-started>
- Chat / edit modes — <https://code.visualstudio.com/docs/copilot/chat/copilot-edits>
- Repository custom instructions — <https://docs.github.com/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot>
- Custom instructions in VS Code — <https://code.visualstudio.com/docs/copilot/customization/custom-instructions>
- Custom instructions support matrix — <https://docs.github.com/en/copilot/reference/custom-instructions-support>
- Prompt files — <https://code.visualstudio.com/docs/copilot/customization/prompt-files>
- Agent Skills — <https://code.visualstudio.com/docs/copilot/customization/agent-skills>
- Copilot cloud agent — <https://code.visualstudio.com/docs/copilot/copilot-cloud-agent>
