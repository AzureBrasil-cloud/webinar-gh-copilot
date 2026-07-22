# Module 3 - GitHub Copilot: Agent Skills and the Obsidian Vault as Agent Context

**Goal:** use GitHub Copilot Agent Skills and an Obsidian knowledge vault as a persistent context source, so specialized agents can implement a full Customers CRUD (backend and frontend) with maximum accuracy - grounded in curated project notes instead of re-discovering the codebase every time.

**Deliverable:** (1) the `Docs/Vault/BookStore/` vault populated with accurate context notes reconciled from the codebase, `full-doc.md`, and `deploy.sh`; (2) the `Developer-Specialist` and `Tester-Specialist` agents updated with a mandatory vault workflow (read instructions, search, semantic-search, implement, write back); (3) a working `Customer` CRUD across Domain, Application, and Web, implemented via Plan mode + the task-planner prompt, with each subtask grounded in the vault.

---

## Prerequisites

✅ **Tools installed**

- VS Code (latest)
- GitHub Copilot + GitHub Copilot Chat extensions, signed in
- .NET 10 SDK
- `ripgrep` and `fd` (used by the vault search skills)

```bash
code --version
dotnet --version
brew install ripgrep fd
```

✅ **Repository checked out - `feature/Module2` branch**

Module 3 builds on the result of module 2 (custom agents, prompt file, and the tests project).

```bash
git checkout feature/Module2
```

✅ **Quick start**

```bash
dotnet build Src/BookStore.slnx
dotnet run --project Src/BookStore.Web
```

Open <http://localhost:5045>.

---

## Problems to solve in this module

### Problem 1 - The agent has no persistent, curated project context

Every time an agent touches the code, it re-discovers architecture, invariants, and conventions from scratch. The repository already ships an Obsidian **vault** at `Docs/Vault/BookStore/` with a folder structure, index files, a note template, and a `vault.instructions.md` rulebook - but the content folders are effectively empty. Without curated notes, the agent cannot ground its work and tends to drift from the established patterns.

**Fix:** populate the vault by running the `/vault-full-sync` skill against the real sources of truth (the codebase, `full-doc.md`, and `vault.instructions.md`), including a note that documents the production release process in `deploy.sh`.

### Problem 2 - Specialist agents do not consult or maintain the vault

The `Developer-Specialist` and `Tester-Specialist` agents from module 2 implement changes correctly, but they never read the vault before working, and never record what they learned afterward. The context stays stale and the vault never grows.

**Fix:** update both agent definitions so that, before touching any code, they follow a mandatory flow: read `vault.instructions.md`, search the vault (`/vault-search`), semantic-search the vault (`/vault-semantic-search`), perform the task, then write back with `/vault-write` (creating new tags if needed).

### Problem 3 - There is no Customer feature

The BookStore solution manages `Author` and `Book`, but the product owner now wants to manage **Customers**. There is no `Customer` entity, service, controller, or views.

**Fix:** implement a full `Customer` CRUD (backend and frontend) using Plan mode + the task-planner prompt from module 2, so each subtask is routed to the right specialized agent and grounded in the vault context.

---

## GitHub Copilot features used in this module

### Agent Skills

Agent Skills are folders of instructions, scripts, and resources (each defined by a `SKILL.md` file with `name` and `description` frontmatter) that Copilot loads on demand when a task matches the skill's description. Skills load progressively - Copilot first reads only the name and description, then loads the `SKILL.md` body when relevant, and finally accesses referenced resource files only when needed. Unlike custom instructions (which define always-on guidelines), skills package specialized, task-specific capabilities and workflows, and they are portable across VS Code, Copilot CLI, and the Copilot cloud agent. In this module the vault skills are the primary example. This module uses skills that already exist in the repository - you will not author new skills here, only use them.

> Reference: [Use Agent Skills in VS Code](https://code.visualstudio.com/docs/agent-customization/agent-skills) - [About agent skills (GitHub Docs)](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills)

### The vault skills (already created in this repo)

The repository ships four skills under `.github/skills/` that treat the Obsidian vault as a first-class context store. Each is invocable as a slash command:

- **`/vault-search`** - full-text keyword search over `Docs/Vault/BookStore/`. It loads the tag catalog and navigation index live, decomposes your query into 2-5 search items, and runs chained `rg` (ripgrep) intersection pipelines to find matching notes, following `[[wikilinks]]` one hop. Use it when you know the terms, tags, or keywords.
- **`/vault-semantic-search`** - meaning-based search that uses the VS Code Copilot workspace index (`#codebase`). It expands your question with synonyms and domain vocabulary, then finds relevant notes even when you do not know the exact wording. Use it for conceptual questions or when keyword search comes up empty.
- **`/vault-write`** - the only sanctioned way to write to the vault. It detects add-vs-update, picks the correct folder and note template, builds a `PascalCase-With-Dashes.md` filename, writes compliant frontmatter, cross-links via `[[wikilinks]]`, and keeps `Navigation.md` / `Tags.md` in sync. External links are placed only in `06_References/`.
- **`/vault-full-sync`** - an end-to-end audit that diffs the whole vault against the codebase, classifies each note (missing, outdated, incomplete, orphaned, unlinked, mistagged, aligned), and reconciles drift by delegating writes to `/vault-write`. It always ends with a written report.

> Reference: [Use Agent Skills in VS Code](https://code.visualstudio.com/docs/agent-customization/agent-skills) - [About agent skills (GitHub Docs)](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills)

### The search engine and write-back logic behind the skills

The two search skills use different engines on purpose:

- **Keyword engine (`/vault-search`)**: deterministic `rg --glob "*.md" --ignore-case --fixed-strings` pipelines scoped strictly to `Docs/Vault/BookStore/`. It reads the live tag catalog (`00_Index/Tags.md`) and navigation index (`00_Index/Navigation.md`) before searching, decomposes the query, intersects results across terms, falls back progressively when there are too few hits, and follows `[[wikilinks]]` one hop. It never fabricates content that is not in the vault.
- **Semantic engine (`/vault-semantic-search`)**: rides the VS Code Copilot semantic index via `#codebase`. It finds notes by meaning, treats vault notes as the primary authority and code as supporting detail, and falls back to `/vault-search` if it finds nothing.
- **Write-back logic (`/vault-write`)**: enforces the vault's structure so notes stay consistent no matter who (human or agent) contributes. It searches first to avoid duplicates, keeps notes atomic (one topic per note), never invents folders or tags silently (it updates `Tags.md` / `Home.md` in the same change), and keeps external links confined to `06_References/`. This is why the rule is: **always write to the vault through `/vault-write`** - never hand-edit notes.

The net effect: the vault becomes a bidirectional context source for the agent - it **reads** curated notes to ground its work, and **writes** new notes back so future tasks are even better grounded.

### Repository custom instructions for the vault (`vault.instructions.md`, already created)

`.github/instructions/vault.instructions.md` is a scoped instruction file (`applyTo: "Docs/Vault/BookStore/**"`) that is the single source of the vault's rules: hard rules (never invent content, always cite source file paths, keep code snippets minimal), when-to-update triggers, the required YAML frontmatter, the folder structure, and the content guidelines. Every vault skill reads this file live at runtime, so the rules live in exactly one place. Because it is a custom instruction, Copilot also applies it automatically whenever a vault file is in context.

> Reference: [Use custom instructions in VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-instructions) - [Add repository instructions (GitHub Docs)](https://docs.github.com/en/copilot/how-tos/copilot-on-github/customize-copilot/add-custom-instructions/add-repository-instructions)

### Plan mode + task-planner prompt + subagents (recap from module 2)

You will reuse the `task-planner` prompt file and Plan mode from module 2 to decompose the Customer CRUD into subtasks and route each to the right specialized agent via `runSubagent`, with no manual agent switching.

> Reference: [Planning with agents in VS Code](https://code.visualstudio.com/docs/agents/planning) - [Use prompt files in VS Code](https://code.visualstudio.com/docs/copilot/customization/prompt-files)

---

## How the vault works

Think of the vault as a curated, human-readable knowledge base that doubles as machine-readable agent context.

- **What it is:** an Obsidian vault at `Docs/Vault/BookStore/`. Notes are Markdown files with YAML frontmatter and `[[wikilinks]]` between them. It is already structured but its content folders start out effectively empty.
- **Who writes it:** in our workflow the vault is written **primarily by the agent**, but it is **readable by anyone**. Humans and agents alike must write through `/vault-write` so the organization, frontmatter, tags, and navigation stay consistent. Never hand-edit notes.
- **Why it exists here:** it is a **context source for the agent**. The agent searches it to ground its work and writes new notes back after non-trivial changes so the context keeps improving.
- **Its sources of truth (in priority order):** the codebase under `Src/` (authoritative for behavior), then `full-doc.md` (a maintained working document), then `vault.instructions.md` (the rulebook). Vault notes are never the source of truth themselves - they are the artifact derived from those sources.

### Existing vault structure

```
Docs/Vault/BookStore/
├── 00_Index/        # Home, Navigation, Tags, Glossary (index files)
├── 01_Project/      # Repo structure, dependencies, project-level conventions
├── 02_Architecture/ # Layer responsibilities, patterns, key decisions
├── 03_Domain/       # Entities, relationships, business rules from code
├── 04_Engineering/  # Code style, error handling, validation, pagination, services
├── 05_Operations/   # Build, run, test, and deployment procedures
├── 06_References/    # External docs and API references (only place for external links)
└── 07_Templates/    # Note templates for consistent authoring
```

---

## Step 0 - Understand the vault and the skills (Ask mode only)

Before changing anything, explore the vault, its rules, and the skills using **Ask** mode. This is read-only - no edits are made. Switch the Copilot Chat dropdown to **Ask** for every prompt in this step.

### 0.1) Vault purpose and structure

**Prompt:**

```text
There is an Obsidian vault in this repository under Docs/Vault. Describe its folder structure,
what each top-level folder is meant to hold, and which files act as index or navigation notes.
Is the vault currently populated with content, or mostly empty?
```

**Expected outcome:** Copilot describes the `00_Index` through `07_Templates` folders, identifies `Home`, `Navigation`, `Tags`, and `Glossary` as index notes, and confirms the content folders are effectively empty.

### 0.2) The vault rulebook

**Prompt:**

```text
Find the instruction file that defines the rules for the Obsidian vault.
Summarize its hard rules, the required note frontmatter, the folder structure it mandates,
and the triggers that say when the vault must be updated.
```

**Expected outcome:** Copilot locates `vault.instructions.md`, summarizes the hard rules (never invent content, cite source paths, minimal code snippets, link to Navigation and tag per taxonomy), the YAML frontmatter shape, and the when-to-update triggers.

### 0.3) The note template and tag taxonomy

**Prompt:**

```text
What note template is used when authoring new vault notes, and what sections does it contain?
Separately, describe the tag taxonomy defined for the vault: what tag categories exist and give examples of each.
```

**Expected outcome:** Copilot describes the template in `07_Templates/` (frontmatter + Context / Details / Related sections) and the tag categories (`type/*`, `area/*`, `component/*`, `entity/*`, plus the base `note` tag).

### 0.4) The vault skills

**Prompt:**

```text
List the agent skills defined in this repository that relate to the Obsidian vault.
For each one, tell me its name, what it does, and when I would use it.
```

**Expected outcome:** Copilot lists `vault-search`, `vault-semantic-search`, `vault-write`, and `vault-full-sync`, describing each one's purpose.

### 0.5) The search engines behind the skills

**Prompt:**

```text
Compare how the vault keyword search skill and the vault semantic search skill actually find notes.
What underlying mechanism does each use, when should I prefer one over the other,
and how do they fall back on each other?
```

**Expected outcome:** Copilot explains that keyword search uses ripgrep pipelines scoped to the vault while semantic search uses the `#codebase` index, and describes the fallback behavior between them.

### 0.6) The write-back logic

**Prompt:**

```text
Explain how the vault write skill keeps the vault consistent.
How does it decide between creating and updating a note, how does it choose the folder and filename,
and how does it keep the navigation index and tag catalog in sync?
Why should everyone write to the vault only through this skill?
```

**Expected outcome:** Copilot explains add-vs-update detection, folder/template/filename selection, `Navigation.md` and `Tags.md` maintenance, and why hand-editing would break consistency.

### 0.7) The data sources for the vault

**Prompt:**

```text
If we were to populate this vault, what are the authoritative sources of truth for its content,
and in what priority order? Consider the source code, any maintained documentation file at the repo root,
and the vault rules file.
```

**Expected outcome:** Copilot identifies the codebase under `Src/` as authoritative, `full-doc.md` as the maintained working document, and `vault.instructions.md` as the rulebook - and notes that vault notes themselves are never the source of truth.

### 0.8) The production release process

**Prompt:**

```text
Is there a deployment or release script in this repository?
Describe what it does end to end: what it builds, how it packages the app, where it deploys,
and which cloud resources it targets. Should this be documented in the vault operations section?
```

**Expected outcome:** Copilot finds `deploy.sh`, explains that it publishes `BookStore.Web` in Release, zips the output, and deploys to Azure App Service (`app-book-store` in resource group `rg-book-store`, plan `asp-book-store`), and agrees it belongs in `05_Operations/`.

---

## Step 1 - Problem 1: Populate the vault with curated context

### 1.1) Discover the gap (Ask mode)

Switch to **Ask** mode.

**Prompt:**

```text
Audit the current state of the Obsidian vault content folders (01_Project through 06_References).
Which topics that clearly exist in the codebase and in the root documentation file are NOT yet documented as vault notes?
List the concrete gaps: architecture, domain entities and invariants, application services, engineering conventions,
build and run operations, and production deployment.
```

**Expected outcome:** Copilot confirms the content folders are empty and lists the concrete topics that should exist as notes: solution layout and layering, `Author`/`Book` entities and their invariants, application services and pagination conventions, code style, build/run/test operations, and the `deploy.sh` release process.

### 1.2) Fix using the `/vault-full-sync` skill

You will populate the vault in one pass with the `/vault-full-sync` skill. It reads the codebase, `full-doc.md`, and `vault.instructions.md`, diffs them against the (empty) vault, and reconciles every gap through `/vault-write` - then prints a report.

Switch to **Agent** mode. Open the prompt/skill picker by typing `/` in the chat input and select **vault-full-sync**, then provide this prompt:

**Prompt:**

```text
/vault-full-sync
Scope: the entire BookStore vault (Docs/Vault/BookStore).

Sources of truth, in priority order:
1. The codebase under Src/ (authoritative for behavior).
2. full-doc.md at the repo root (maintained working document).
3. .github/instructions/vault.instructions.md (vault rules).

Populate the vault so it accurately documents at least:
- 02_Architecture: the three-layer solution layout, project dependency graph, the composition root
  in Src/BookStore.Web/Program.cs, and the layering rule (Domain owns invariants, Application orchestrates,
  Web controllers are thin).
- 03_Domain: the Author and Book entities with all properties, computed members, and every invariant
  (with the exact DomainException messages), the Author/Book relationship, and the seed data behavior.
- 04_Engineering: C# code style, the application service orchestration pattern, and the pagination
  conventions (PagedResult<T>, GetPagedAsync, default page size 10).
- 05_Operations: build, run, and test commands, the EF Core In-Memory persistence note, AND a dedicated
  runbook for production deployment based on deploy.sh (what it publishes, how it zips, and the exact
  Azure targets: resource group rg-book-store, App Service plan asp-book-store, web app app-book-store).
- 01_Project: repository structure and the NuGet dependency inventory per project.

Follow .github/instructions/vault.instructions.md exactly: correct frontmatter, one topic per note,
cite source file paths, keep code snippets minimal, place any external links only in 06_References,
and update 00_Index/Navigation.md and 00_Index/Tags.md. End with the discrepancy-and-changes report.
```

The agent will now execute the sync and generate its own report - the exact notes it creates and the wording it chooses may vary from run to run, so treat the report it produces as the source of what changed.

**Verify:** confirm the content folders now contain notes and that the deployment runbook exists.

```bash
find Docs/Vault/BookStore -name "*.md" | sort
```

You should see notes across `01_Project/`, `02_Architecture/`, `03_Domain/`, `04_Engineering/`, `05_Operations/` (including a deployment runbook), and `06_References/`, plus updated `00_Index/Navigation.md` and `00_Index/Tags.md`.

### 1.3) Confirm the deployment note captured `deploy.sh`

The production release process must be in the vault. Verify the runbook reflects `deploy.sh`.

**Prompt (Ask mode):**

```text
Show me the operations note in the vault that documents production deployment.
Does it correctly describe that deploy.sh publishes BookStore.Web in Release, zips the output,
and deploys to Azure App Service with resource group rg-book-store, plan asp-book-store, and web app app-book-store?
```

**Expected outcome:** Copilot displays the `05_Operations/` deployment runbook and confirms it matches `deploy.sh`. If anything is missing, re-run `/vault-full-sync` scoped to operations, or write the correction through `/vault-write`.

---

## Step 2 - Explore the populated vault with `/vault-search`

Now that the vault has content, practice retrieving it. These are read-only searches - run them in **Agent** mode so the skill can execute its `rg` pipelines, or via the `/` picker.

### 2.1) Keyword search examples

**Prompt (example 1 - find invariants):**

```text
/vault-search Book invariants and DomainException messages
```

**Expected outcome:** the skill returns the `03_Domain` note(s) describing the `Book` entity invariants and the exact exception messages.

**Prompt (example 2 - find the deployment runbook):**

```text
/vault-search production deployment Azure App Service deploy.sh
```

**Expected outcome:** the skill returns the `05_Operations` deployment runbook with the Azure resource names.

**Prompt (example 3 - find the pagination convention):**

```text
/vault-search pagination PagedResult GetPagedAsync page size
```

**Expected outcome:** the skill returns the `04_Engineering` pagination note describing `PagedResult<T>`, `GetPagedAsync`, and the default page size of 10.

### 2.2) Semantic search example

When you do not know the exact terms, use meaning-based search.

**Prompt:**

```text
/vault-semantic-search How does the application enforce business rules and where does validation live
across the layers?
```

**Expected outcome:** the skill uses the `#codebase` index to surface the architecture and domain notes explaining that invariants live in the entities (rich domain model) and services only orchestrate.

---

## Step 3 - Problem 2: Make the specialist agents vault-aware

### 3.1) Discover the gap (Ask mode)

Switch to **Ask** mode.

**Prompt:**

```text
Look at the custom agents defined under .github/agents. Do the Developer-Specialist and Tester-Specialist agents
currently instruct the agent to consult the Obsidian vault before making changes, or to write context back to the
vault afterward? If not, what mandatory steps should be added so every task reads vault context first and records
new context after?
```

**Expected outcome:** Copilot confirms neither agent references the vault, and suggests adding a mandatory pre-work flow (read `vault.instructions.md`, `/vault-search`, `/vault-semantic-search`) and a post-work step (`/vault-write`).

### 3.2) Fix by updating both agent definitions

Add the mandatory vault workflow to each agent. You can edit the two `.agent.md` files by hand, or ask Agent mode to apply the change. The required flow, for both agents, is:

1. Read `.github/instructions/vault.instructions.md`.
2. Use `/vault-search` to find relevant context notes in the vault.
3. Use `/vault-semantic-search` to find additional context by meaning.
4. Perform the task according to the agent's own instructions.
5. Use `/vault-write` to write context back into the vault, creating new tags if needed.

**Prompt (Agent mode) to apply the change:**

```text
Update both custom agents at .github/agents/developer-specialist.agent.md and
.github/agents/tester-specialist.agent.md. In each file, add a new mandatory section titled
"Mandatory Vault Workflow" that MUST run before any code is touched and after the task completes.

The workflow, in order:
1. Read .github/instructions/vault.instructions.md to load the vault rules.
2. Run /vault-search to find relevant existing context notes for the task.
3. Run /vault-semantic-search to find additional relevant notes by meaning.
4. Perform the assigned task following the rest of this agent's instructions.
5. Run /vault-write to record new or updated context notes about what changed,
   creating new tags in 00_Index/Tags.md if the taxonomy does not yet cover the topic.

State clearly that writing to the vault must ONLY happen through /vault-write (never hand-edit notes),
and that steps 1-3 are prerequisites the agent must not skip. Keep the rest of each agent file unchanged.
```

The agent will apply the edits; review the diff before accepting.

### 3.3) The updated agent definitions

After the change, `.github/agents/developer-specialist.agent.md` should read approximately as follows (the "Mandatory Vault Workflow" section is the new part):

````markdown
---
name: Developer-Specialist
description: "Use when: adding or modifying entity properties in the BookStore domain, updating Create/Update factory methods, propagating changes to application services, controllers, or Razor views, updating seed data, or running a build to verify correctness. Specializes in implementing .NET domain changes for the BookStore three-layer solution."
tools:
  - read
  - edit
  - search
  - execute
---

# Developer Specialist

You are a .NET domain expert for the BookStore solution. Your job is to implement domain changes end-to-end across the three-layer architecture.

## Mandatory Vault Workflow

Before touching ANY code, and again after the task is complete, you MUST run this flow. Do not skip steps 1-3.

1. Read `.github/instructions/vault.instructions.md` to load the current vault rules.
2. Run `/vault-search` to find relevant existing context notes for the task.
3. Run `/vault-semantic-search` to find additional relevant notes by meaning.
4. Perform the assigned task following the rest of this agent's instructions.
5. Run `/vault-write` to record new or updated context notes about what changed, creating new tags in `00_Index/Tags.md` if the taxonomy does not yet cover the topic.

Writing to the vault MUST happen ONLY through `/vault-write` - never hand-edit vault notes.

## Architecture

- **BookStore.Domain** - rich entities (`Author`, `Book`). All invariants are enforced inside the entity by throwing `DomainException` (from `BookStore.Domain/Exceptions/DomainException.cs`). Never throw `InvalidOperationException` from entities.
- **BookStore.Application** - pure orchestration: load aggregates via `BookStoreContext`, call entity domain methods, persist via `SaveChangesAsync`. No business rules here.
- **BookStore.Web** - thin controllers (model binding -> service call -> view/redirect). No business logic in controllers or views.

## Code Style

- C# 12, nullable enabled, file-scoped namespaces.
- `var` when type is obvious.
- `async`/`await` end-to-end for all DB calls.
- Domain methods use verbs: `Rename`, `Restock`, `ChangePrice`, `AssignAuthor`, etc.

## Workflow: Adding or Modifying a Property

1. **Read the entity file first** - never invent class names or method signatures.
2. Add the property with the correct type and access modifier:
   - Computed/derived properties: fully read-only (no setter).
   - Settable properties: private setter.
3. If the property requires validation, add the rule inside the existing private `Validate()` method and throw `DomainException` with a clear message.
4. If the property is settable, update `Create()` and `Update()` method signatures and bodies.
5. Propagate to **controllers**: read the relevant controller before editing; update action method parameters and model binding.
6. Propagate to **Razor views**: update form views (`Create.cshtml`, `Edit.cshtml`) and display views (`Index.cshtml`, `Details.cshtml`).
7. Update **seed data** in `DataSeeder.cs` when a new required field is added.
8. Run `dotnet build Src/BookStore.slnx` and fix all compilation errors before finishing.

## Constraints

- DO NOT duplicate business rules in services or controllers - invariants belong in the entity.
- DO NOT throw `InvalidOperationException` from entities - always use `DomainException`.
- DO NOT add EF Core migrations or change the persistence provider (In-Memory only).
- DO NOT write or modify test files - that is outside this agent's scope.
- DO NOT skip the build step - always confirm a clean build before reporting completion.
````

And `.github/agents/tester-specialist.agent.md` should read approximately as follows:

````markdown
---
name: Tester-Specialist
description: "Use when: creating a new xUnit test project, adding it to the BookStore solution, adding NuGet packages for testing, writing unit tests for BookStore domain entities (Author, Book), or running dotnet test to verify correctness. Specializes in .NET testing for the BookStore solution."
tools:
  - read
  - edit
  - search
  - execute
---

# Tester Specialist

You are a .NET testing expert for the BookStore solution. Your job is to create and maintain xUnit test projects that verify domain entity behavior.

## Mandatory Vault Workflow

Before writing ANY test code, and again after the task is complete, you MUST run this flow. Do not skip steps 1-3.

1. Read `.github/instructions/vault.instructions.md` to load the current vault rules.
2. Run `/vault-search` to find relevant existing context notes for the task.
3. Run `/vault-semantic-search` to find additional relevant notes by meaning.
4. Perform the assigned task following the rest of this agent's instructions.
5. Run `/vault-write` to record new or updated context notes about what changed, creating new tags in `00_Index/Tags.md` if the taxonomy does not yet cover the topic.

Writing to the vault MUST happen ONLY through `/vault-write` - never hand-edit vault notes.

## Domain Knowledge

- **Author** entity: `Create()` factory, `Update()`, `EnsureCanBeDeleted()`, computed `Age` property. Throws `DomainException` for: empty name, name > 150 chars, BirthDate in the future, deleting author with books.
- **Book** entity: `Create()` factory, `Update()`. Throws `DomainException` for: empty title, negative price, negative stock, PublishedDate in the future.
- `DomainException` is in `BookStore.Domain.Exceptions`. Always use `FluentAssertions` to assert it is thrown: `act.Should().Throw<DomainException>().WithMessage("...")`.

## Creating a New Test Project

Run these commands in order:

```bash
dotnet new xunit -n BookStore.Tests -o Src/BookStore.Tests
dotnet sln Src/BookStore.slnx add Src/BookStore.Tests/BookStore.Tests.csproj
dotnet add Src/BookStore.Tests reference Src/BookStore.Domain
dotnet add Src/BookStore.Tests package FluentAssertions
```

## Writing Tests

- Follow **Arrange-Act-Assert** pattern in every test method.
- Use `FluentAssertions` for all assertions (e.g., `result.Should().NotBeNull()`, `result.Age.Should().Be(30)`).
- One test class per entity, file named `<Entity>Tests.cs` (e.g., `AuthorTests.cs`, `BookTests.cs`).
- Cover: happy paths, boundary conditions (e.g., 0, -1, max), and every `DomainException` throw.
- Use `DateTime.UtcNow` for date calculations in tests; never hardcode specific dates.

## Running Tests

- Always run `dotnet test Src/BookStore.Tests` after writing or modifying tests.
- Read the failure output carefully and fix all failures before finishing.
- Report the final test count and 0 failures.

## Constraints

- DO NOT modify entity source files (`BookStore.Domain`) - only write test code.
- DO NOT modify application services, controllers, or views.
- DO NOT skip the `dotnet test` step - always confirm a green run before reporting completion.
````

**Verify:** confirm the new section is present in both files.

```bash
grep -l "Mandatory Vault Workflow" .github/agents/*.agent.md
```

Both agent files should be listed.

---

## Step 4 - Problem 3: Implement the Customer CRUD with the vault as context

Now use Plan mode + the `task-planner` prompt file (from module 2) to build the `Customer` feature. Because both specialist agents now consult the vault first, each subtask is grounded in the curated context you populated in Step 1.

### 4.1) Discover the gap (Ask mode)

Switch to **Ask** mode.

**Prompt:**

```text
Does the solution have any Customer concept today - an entity, application service, controller, or views?
Following the same patterns as Author and Book, describe end to end what a Customer CRUD would need:
domain entity with invariants, application service, controller, Razor views, DI registration, and seed data.
```

**Expected outcome:** Copilot confirms there is no `Customer` anywhere and outlines the pieces needed, mirroring the `Author`/`Book` structure (rich entity with `DomainException` invariants, orchestration-only service, thin controller, Bootstrap views, `Program.cs` registration, and seed data).

### 4.2) Plan and execute with Plan mode + task-planner

Switch to **Plan** mode. Open the `/` picker and select the **task-planner** prompt file (created in module 2), then provide the task below. Plan mode will produce a reviewable plan that routes each subtask to `Developer-Specialist` and `Tester-Specialist` as subagents; because those agents now run the mandatory vault workflow, they will read and write vault context automatically.

**Prompt (use with the task-planner prompt file in Plan mode):**

```text
/task-planner
Implement a full Customer CRUD (backend and frontend) in the BookStore solution, following the exact same
architecture and conventions already used for Author and Book. Ground every subtask in the Obsidian vault:
read the relevant context notes before implementing, and write new context notes back afterward.

Requirements:
- Domain (BookStore.Domain):
  - Add a rich Customer entity with: Id, FullName (required, max 150), Email (required, valid email format),
    PhoneNumber (optional), and CreatedAt.
  - Enforce all invariants inside the entity by throwing DomainException with clear messages
    (e.g., "Customer full name is required.", "Customer email is required.", "Customer email is invalid.").
  - Provide a static Create(...) factory and an Update(...) method, mirroring Author/Book.
  - Register the DbSet<Customer> in BookStoreContext and configure the field lengths in OnModelCreating.
  - Add a few seeded customers in DataSeeder.cs.
- Application (BookStore.Application):
  - Add ICustomerService and CustomerService with GetPagedAsync (default page size 10, following the
    existing pagination convention), GetByIdAsync, CreateAsync, UpdateAsync, and DeleteAsync.
    Orchestration only - no business rules in the service.
- Web (BookStore.Web):
  - Register ICustomerService/CustomerService in Program.cs.
  - Add a thin CustomersController with Index (paged), Details, Create (GET/POST), Edit (GET/POST),
    and Delete/DeleteConfirmed, catching DomainException and re-adding errors to ModelState.
  - Add Bootstrap 5 Razor views under Views/Customers (Index with the shared pagination component,
    Details, Create, Edit, Delete), matching the existing Books/Authors views.
- Tests (BookStore.Tests):
  - Add CustomerTests.cs covering Create with valid data, and DomainException on empty name, empty email,
    and invalid email.
- Run dotnet build Src/BookStore.slnx and dotnet test Src/BookStore.Tests - both must pass.

Route implementation subtasks to Developer-Specialist and testing subtasks to Tester-Specialist,
invoking each as a subagent per the task-planner Hard Rules.
```

Review the generated plan and its subtask-to-agent assignments. When you are satisfied, click **Start** to execute. The orchestrator will invoke each specialist as a subagent; each subagent runs its mandatory vault workflow, implements its slice, and (via `/vault-write`) records new context notes - the concrete notes, tags, and code the agents produce will vary from run to run, so rely on their output rather than a fixed expected response.

---

## Step 5 - Verify everything

```bash
dotnet build Src/BookStore.slnx
dotnet run --project Src/BookStore.Web
```

| Route / check | Expected behaviour |
|---|---|
| `/Customers` | Paged list of customers, page size 10, using the shared pagination component |
| `/Customers/Details/1` | Customer details page renders FullName, Email, PhoneNumber, CreatedAt |
| `/Customers/Create` with valid data | Customer is created and appears in the list |
| `/Customers/Create` with empty email | Validation error shown; rule enforced inside the `Customer` entity, not the service |
| `/Customers/Create` with an invalid email | Validation error "Customer email is invalid." (or equivalent) |
| `/Customers/Edit/1` | Existing values pre-filled; save persists changes |
| `/Customers/Delete/1` | Confirmation page, then removal from the list |

Run the tests:

```bash
dotnet test Src/BookStore.Tests
```

All existing tests plus the new `CustomerTests` should pass with 0 failures.

Confirm the vault grew:

```bash
find Docs/Vault/BookStore -name "*.md" | sort
```

You should now see Customer-related context note(s) written back through `/vault-write`, plus any new tags added to `00_Index/Tags.md`.

---

## Recap

| Step | Purpose | Copilot Mode | Outcome |
|---|---|---|---|
| **0** | Explore the vault, its rules, the skills, and the search engines | Ask mode (8 prompts) | Team alignment on how the vault and skills work - no changes |
| **1** | Populate the empty vault from code + `full-doc.md` + rules, including the `deploy.sh` runbook | Agent mode + `/vault-full-sync` | Vault content folders filled; deployment runbook created; indexes updated |
| **2** | Retrieve notes from the populated vault | `/vault-search` + `/vault-semantic-search` | Confirmed the vault is searchable by keyword and by meaning |
| **3** | Make specialist agents read and write the vault | Ask mode + agent-file edits | Both agents gain a mandatory vault workflow |
| **4** | Build the Customer CRUD grounded in vault context | Plan mode + task-planner + subagents | Customer entity, service, controller, views, and tests implemented; new context written back |
| **5** | Verify everything | Manual + `dotnet test` | CRUD works end to end, all tests pass, vault has grown |

---

## Wrap-up

Commit the result to `feature/Module3`:

```bash
git checkout -b feature/Module3
git add .
git commit -m "feature/Module3: agent skills + populated vault as context + vault-aware agents + Customer CRUD"
```

---

## References

- Use Agent Skills in VS Code - <https://code.visualstudio.com/docs/agent-customization/agent-skills>
- About agent skills (GitHub Docs) - <https://docs.github.com/en/copilot/concepts/agents/about-agent-skills>
- Adding agent skills for the Copilot CLI - <https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-skills>
- Adding agent skills for the Copilot cloud agent - <https://docs.github.com/en/copilot/how-tos/copilot-on-github/customize-copilot/customize-cloud-agent/add-skills>
- GitHub Copilot Chat concept - <https://docs.github.com/en/copilot/concepts/chat>
- Add repository custom instructions - <https://docs.github.com/en/copilot/how-tos/copilot-on-github/customize-copilot/add-custom-instructions/add-repository-instructions>
- Use custom instructions in VS Code - <https://code.visualstudio.com/docs/copilot/customization/custom-instructions>
- Use prompt files in VS Code - <https://code.visualstudio.com/docs/copilot/customization/prompt-files>
- Planning with agents in VS Code - <https://code.visualstudio.com/docs/agents/planning>
- Chat overview in VS Code - <https://code.visualstudio.com/docs/chat/chat-overview>
- Copilot Chat context in VS Code - <https://code.visualstudio.com/docs/chat/copilot-chat-context>
- Obsidian help - <https://obsidian.md/help/Home>
- Obsidian internal links - <https://obsidian.md/help/links>
- Obsidian developer docs - <https://docs.obsidian.md/Home>
