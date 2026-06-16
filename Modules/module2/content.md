# Module 2 - GitHub Copilot: Custom Agents, Subagents, Prompt Files, and Plan Mode

**Goal:** use workspace-scoped custom agents, the `/create-agent` command, prompt files, and Plan mode to add new properties to the BookStore domain entities and build a test suite - all orchestrated by Copilot without manually switching between agents.

**Deliverable:** the `BookStore` solution with (1) a computed `Age` property on `Author`, a `BookStore.Tests` xUnit project created, and `AuthorTests` passing; (2) a `NumberOfPages` property on `Book` with validation, `BookTests` created and passing - all implemented by specialized custom agents routing work automatically via subagents.

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

✅ **Repository checked out - `feature/module1` branch**

```bash
git checkout feature/module1
```

---

## Problems to solve in this module

### Problem 1 - Author entity lacks an Age property and the solution has no test project

The `Author` entity in `Src/BookStore.Domain/Entities/Author.cs` exposes `BirthDate` but never exposes a derived `Age`. Product owners want to see the author's current age in the Authors Index and Details views. There is also no test project anywhere in the solution - domain logic is entirely untested.

**Fix:** add a computed read-only `Age` property to `Author`; update `Views/Authors/Index.cshtml` and `Views/Authors/Details.cshtml` to display it; create a `BookStore.Tests` xUnit project; add tests that verify the `Age` computation and key domain invariants.

### Problem 2 - Book entity lacks a NumberOfPages property

The `Book` entity in `Src/BookStore.Domain/Entities/Book.cs` has no `NumberOfPages` field. The product owner wants to capture how many pages each book has. The domain must enforce that `NumberOfPages >= 1`.

**Fix:** add `NumberOfPages` (int, validated >= 1) to `Book`; update `Create()` and `Update()` factory methods; propagate the field through the controller, form views, index, details, and seed data; update the test project with `BookTests` covering valid and invalid values.

---

## GitHub Copilot features used in this module

### Custom Agents (workspace-scoped)

Custom agents are specialized AI assistants defined as `.agent.md` files inside `.github/agents/`. Each file declares a `name`, a `description` (used by Copilot for routing), a `tools` list, and a system prompt that scopes the agent's expertise. Once committed to the repo, any team member can select the agent from the Copilot Chat agent picker. Use custom agents when you want a reusable, context-rich persona - such as a domain developer or a test engineer - so you never have to re-explain the project architecture on every prompt.

> Reference: [Custom agents in VS Code](https://code.visualstudio.com/docs/agent-customization/custom-agents)

### `/create-agent` command

`/create-agent` is a built-in Copilot slash command available in Agent mode. Type it followed by a description of the agent you want, and Copilot bootstraps the `.agent.md` file for you - generating the frontmatter, tools list, and initial system prompt. Use it to avoid writing agent files from scratch.

> Reference: [Custom agents in VS Code](https://code.visualstudio.com/docs/agent-customization/custom-agents)

### Subagents and `runSubagent`

A subagent is a custom agent invoked by another agent during task execution using the `runSubagent` tool. When an orchestrating agent assigns a subtask to a specialized agent by name, Copilot runs that agent's system prompt and toolset for that subtask only - then returns control to the orchestrator. This enables multi-agent pipelines without the user manually switching between agents. Note: the Hard Rules in the task-planner prompt are instructional constraints that guide the orchestrator's behavior, not a platform-level enforcement guarantee.

> References: [Asking GitHub Copilot questions in your IDE](https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide) - [Custom agents and sub-agent orchestration](https://docs.github.com/en/copilot/how-tos/copilot-sdk/features/custom-agents)

### Prompt files (`.github/prompts/`)

Prompt files are reusable, parameterized Markdown prompts stored as `.prompt.md` files in `.github/prompts/`. They appear in the Copilot Chat prompt picker and carry a `description`, `name`, and `argument-hint` in YAML frontmatter. Use them to encode complex, repeatable instructions - like a task decomposition strategy - so any team member can invoke them without memorizing the full prompt text.

> Reference: [Use prompt files in VS Code](https://code.visualstudio.com/docs/copilot/customization/prompt-files)

### Plan mode

Plan mode generates a step-by-step implementation plan and presents it for approval before any file is changed. Switch the Copilot Chat dropdown to **Plan**, describe the task, and Copilot responds with an ordered checklist. Review the plan, adjust if needed, then click **Start** to execute. Use Plan mode when you want to inspect and approve an approach before committing to a multi-file change - especially when paired with a custom prompt file that instructs Copilot how to decompose the work.

> References: [Planning with agents in VS Code](https://code.visualstudio.com/docs/agents/planning) - [Copilot CLI plan mode best practices](https://docs.github.com/copilot/how-tos/copilot-cli/cli-best-practices)

---

## Solution layout (what is on the `feature/module1` branch)

```
Src/
├── BookStore.slnx
├── BookStore.Domain/
│   ├── Entities/
│   │   ├── Author.cs      (rich model - no Age property yet)
│   │   └── Book.cs        (rich model - no NumberOfPages property yet)
│   ├── Data/       BookStoreContext.cs
│   ├── Exceptions/ DomainException.cs
│   └── Seed/       DataSeeder.cs        (10 authors + 27 books, no NumberOfPages in seed)
├── BookStore.Application/
│   ├── Common/    PagedResult.cs
│   └── Services/  AuthorService.cs, BookService.cs  (orchestration only)
└── BookStore.Web/
    ├── Controllers/  BooksController.cs, AuthorsController.cs, HomeController.cs
    ├── Views/        Books/*, Authors/*, Shared/_Pagination.cshtml
    └── Program.cs
```

**No test project exists.** Domain model is rich (from module 1), services orchestrate only, pagination is in place.

---

## Step 0 - Understand the project (Ask mode only)

Before creating any agents or fixing any problems, explore and understand the current state of the codebase using Ask mode. This discovery phase ensures team alignment on what already exists and what is missing.

### 0.1) Overall project structure

Switch the Copilot Chat dropdown to **Ask**. This is read-only - no edits will be made.

**Prompt:**

```text
Look at the workspace under Src/. Describe the three projects in the solution and the purpose of each.
Include the .NET version each project targets, the key folders in each project, and how the projects depend on each other.
```

**Expected outcome:** Copilot identifies BookStore.Domain (entities, context, seed), BookStore.Application (services, PagedResult), and BookStore.Web (controllers, views). It confirms all target .NET 10 and the dependency direction Domain <- Application <- Web.

### 0.2) Domain entities and their properties

**Prompt:**

```text
Describe the Author and Book entities in the domain layer.
For each entity list every property (name, type, whether it has a public setter or is read-only),
all domain methods (factory, update, validation), and any computed properties.
```

**Expected outcome:** Copilot lists all Author properties (Id, Name, Bio, BirthDate, Nationality, Books) and all Book properties (Id, Title, Isbn, Description, Genre, Price, Stock, PublishedDate, IsAvailable, AuthorId, Author). It confirms Author has no `Age` property and Book has no `NumberOfPages` property.

### 0.3) Domain validation rules

**Prompt:**

```text
What are the domain validation rules enforced in Author and Book?
Where exactly are they enforced (which method, which class)?
List every rule with the exact error message thrown on violation.
```

**Expected outcome:** Copilot lists all invariants enforced in the private `Validate()` methods and `EnsureCanBeDeleted()` - for example: "Author name is required", "Price cannot be negative", "Published date cannot be in the future", "Cannot delete an author who still has books."

### 0.4) Application services

**Prompt:**

```text
Describe the application services in BookStore.Application.Services.
For each service list the public methods, what they do, and which entity domain methods they call.
Are there any business rules duplicated in the services, or are they pure orchestration?
```

**Expected outcome:** Copilot confirms both services are clean orchestration - they load via EF Core, call entity factory/domain methods (Create, Update, EnsureCanBeDeleted), and save. No duplicated business rules in services.

### 0.5) Controllers and views for Authors

**Prompt:**

```text
Look at the AuthorsController. For each action method tell me which service method is called and which view is returned.
Then look at Views/Authors/Index.cshtml and Views/Authors/Details.cshtml - which properties of Author are currently displayed?
Is Age shown anywhere?
```

**Expected outcome:** Copilot maps each controller action to its service call and view. It confirms Age is not displayed anywhere because the property does not exist on the entity yet.

### 0.6) Test projects in the solution

**Prompt:**

```text
Does the solution have any test projects?
Look at the solution file and all directories under Src/ for any xUnit, NUnit, or MSTest project files.
```

**Expected outcome:** Copilot confirms no test projects exist. The solution file references only BookStore.Domain, BookStore.Application, and BookStore.Web.

### 0.7) Seed data

**Prompt:**

```text
Describe the seed data: how many authors and books are seeded on startup, what fields are populated for each record,
and which class and method performs the seeding. Does any book record include a NumberOfPages field?
```

**Expected outcome:** Copilot identifies `DataSeeder.cs` as the seeding class, lists the 10 authors and 27 books, and confirms no `NumberOfPages` field is seeded (because the property does not exist yet on `Book`).

### 0.8) Custom agents and prompt files

**Prompt:**

```text
Does this workspace have any custom agents or prompt files defined?
Look for .agent.md files under .github/agents/ and .prompt.md files under .github/prompts/.
```

**Expected outcome:** Copilot confirms neither folder exists yet. This is the baseline before module 2 introduces these features.

---

## Step 1 - Problem 1: Author lacks Age and the solution has no tests

### 1.1) Discover the problem (Ask mode)

Switch to **Ask** mode.

**Prompt:**

```text
Look at the Author entity. Does it have an Age property?
BirthDate is already present - what would be the cleanest way to expose Age as a computed property (formula, type, access modifier)?
Which files would need to change if we added a computed Age and wanted to display it in the Authors Index and Details views?
```

**Expected outcome:** Copilot confirms no `Age` property exists, recommends a computed read-only property derived from `BirthDate` (e.g., `(int)Math.Floor((DateTime.UtcNow - BirthDate).TotalDays / 365.25)`), and identifies the files to update: `Author.cs`, `Views/Authors/Index.cshtml`, `Views/Authors/Details.cshtml`.

### 1.2) Create the Developer-Specialist agent using `/create-agent`

You will now create a custom agent that understands the BookStore architecture and specializes in domain changes.

Switch to **Agent** mode. Type the following prompt and press **Enter**:

**Prompt:**

```text
/create-agent Create a workspace-scoped custom agent named "Developer-Specialist".
This agent specializes in implementing .NET domain changes in the BookStore solution.
It knows the three-layer architecture (BookStore.Domain, BookStore.Application, BookStore.Web),
C# 12 with file-scoped namespaces, and the rich domain model pattern where entities enforce their own
invariants by throwing DomainException from BookStore.Domain.Exceptions.
When asked to add or modify a property, it: reads the entity file first, adds the property with the
correct type and access modifier, adds validation to the private Validate() method if required,
updates Create() and Update() signatures if the property is settable, propagates changes to controllers
and Razor views, updates seed data when needed, and always runs dotnet build Src/BookStore.slnx at the
end to confirm no compilation errors.
```

**Expected outcome:** Copilot creates `.github/agents/developer-specialist.agent.md` with content similar to:

```markdown
---
name: Developer-Specialist
description: Implements .NET domain changes in the BookStore solution. Use for adding or modifying entity properties, updating Create/Update factory methods, propagating changes to services, controllers, and Razor views, and running builds to verify correctness.
tools:
  - codebase
  - editFiles
  - runCommands
  - search
---

# Developer Specialist

You are a .NET domain expert for the BookStore solution.

## Architecture
- Three-layer solution: BookStore.Domain, BookStore.Application, BookStore.Web.
- Entities are rich: invariants are enforced through domain methods using DomainException.
- Application services only orchestrate: load via EF Core, call entity methods, save.
- Controllers are thin: model binding, service call, redirect or view.

## When adding a property to an entity
1. Read the current entity file before making any changes.
2. Add the property with the correct type and access modifier.
3. If the property requires validation, add the rule to the private Validate() method.
4. Update Create() and Update() signatures and bodies if the property is settable.
5. Check whether controllers and views reference the entity - update if needed.
6. Update seed data in DataSeeder.cs if a required field is added.
7. Run dotnet build Src/BookStore.slnx and fix any compilation errors before finishing.

## Code style
- C# 12, nullable enabled, file-scoped namespaces.
- var when type is obvious.
- Domain methods use verbs: Rename, Restock, ChangePrice, etc.
```

Verify the file was created at `.github/agents/developer-specialist.agent.md`.

### 1.3) Use Developer-Specialist to add the Age property

Select the **Developer-Specialist** agent from the Copilot Chat agent picker (click the agent selector dropdown and choose Developer-Specialist).

**Prompt:**

```text
Add a computed Age property to the Author entity.

Requirements:
- Age is a read-only computed property (int) derived from BirthDate.
- Formula: (int)Math.Floor((DateTime.UtcNow - BirthDate).TotalDays / 365.25)
- No new constructor parameters. Do not change the Create() or Update() signatures.
- Add an "Age" column to Views/Authors/Index.cshtml (after the Name column).
- Add an Age field to Views/Authors/Details.cshtml.
- Run dotnet build Src/BookStore.slnx and confirm no errors.
```

**Expected outcome:** The agent edits `Author.cs` (adds the computed `Age` property), updates `Views/Authors/Index.cshtml` and `Views/Authors/Details.cshtml`, and reports a successful build.

**Verify:**

```bash
dotnet run --project Src/BookStore.Web
```

Open <http://localhost:5045/Authors>. The table should include an "Age" column showing a computed integer age for each author.

### 1.4) Create the Tester-Specialist agent using `/create-agent`

Switch back to the default **Agent** mode (not Developer-Specialist). Type the following prompt and press **Enter**:

**Prompt:**

```text
/create-agent Create a workspace-scoped custom agent named "Tester-Specialist".
This agent specializes in .NET testing for the BookStore solution.
It can create new xUnit test projects using dotnet new xunit, add them to the solution file
with dotnet sln add, add project references with dotnet add reference, and add NuGet packages
such as FluentAssertions with dotnet add package.
It knows the BookStore domain model - Author and Book entities that throw DomainException on invariant violations.
When writing tests, it follows the Arrange-Act-Assert pattern, uses FluentAssertions for assertions
(e.g., result.Should().Be(expected)), and organizes tests by entity in separate files named <Entity>Tests.cs.
It always runs dotnet test after writing or modifying tests and fixes any failures before finishing.
```

**Expected outcome:** Copilot creates `.github/agents/tester-specialist.agent.md` with content similar to:

```markdown
---
name: Tester-Specialist
description: Creates and maintains xUnit test projects for the BookStore solution. Use when creating a new test project, adding NuGet packages for testing, writing unit tests for domain entities, or running tests to verify correctness.
tools:
  - codebase
  - editFiles
  - runCommands
  - search
---

# Tester Specialist

You are a .NET testing expert for the BookStore solution.

## Creating a new test project
1. Run: dotnet new xunit -n BookStore.Tests -o Src/BookStore.Tests
2. Add to solution: dotnet sln Src/BookStore.slnx add Src/BookStore.Tests/BookStore.Tests.csproj
3. Add domain reference: dotnet add Src/BookStore.Tests reference Src/BookStore.Domain
4. Add FluentAssertions: dotnet add Src/BookStore.Tests package FluentAssertions

## Writing tests
- Follow the Arrange-Act-Assert pattern.
- Use FluentAssertions for all assertions.
- One test class per entity, file named <Entity>Tests.cs.
- Test happy paths, boundary conditions, and all DomainException throws.

## Running tests
- Always run dotnet test Src/BookStore.Tests after writing or modifying tests.
- Fix any failures before finishing.
```

Verify the file was created at `.github/agents/tester-specialist.agent.md`.

### 1.5) Use Tester-Specialist to create the test project and add Author tests

Select the **Tester-Specialist** agent from the Copilot Chat agent picker.

**Prompt:**

```text
Create a new xUnit test project for the BookStore solution and add tests for the Author entity.

Steps:
1. Create the test project at Src/BookStore.Tests using dotnet new xunit.
2. Add it to Src/BookStore.slnx.
3. Add a project reference to BookStore.Domain.
4. Add the FluentAssertions NuGet package.
5. Create Src/BookStore.Tests/AuthorTests.cs with tests that cover:
   - Author.Create() with valid data succeeds and returns a non-null Author.
   - Author.Create() throws DomainException when name is empty or whitespace.
   - Author.Create() throws DomainException when BirthDate is in the future.
   - Author.Age returns the correct computed age: create an Author with BirthDate exactly
     30 years ago (use DateTime.UtcNow.AddYears(-30)) and assert Age == 30.
   - Author.EnsureCanBeDeleted() throws DomainException when the author has at least one Book.
6. Run dotnet test Src/BookStore.Tests and confirm all tests pass.
```

**Expected outcome:** The test project is created at `Src/BookStore.Tests/`, added to the solution, and all 5+ tests in `AuthorTests.cs` pass. The terminal output shows 0 failures.

---

## Step 2 - Problem 2: Book lacks NumberOfPages and tests are missing

### 2.1) Discover the problem (Ask mode)

Switch to **Ask** mode.

**Prompt:**

```text
Look at the Book entity and its Create and Update methods.
Does it have a NumberOfPages property?
What validation rule should apply to NumberOfPages?
Which files would need to change to add NumberOfPages end-to-end:
entity, controller, form views, index and details views, and seed data?
Does the test project we just created have any Book tests yet?
```

**Expected outcome:** Copilot confirms no `NumberOfPages` property exists on `Book`. It recommends validation `NumberOfPages >= 1` enforced via `DomainException`. It lists the files to touch: `Book.cs`, `BooksController.cs`, `Views/Books/Create.cshtml`, `Views/Books/Edit.cshtml`, `Views/Books/Index.cshtml`, `Views/Books/Details.cshtml`, `DataSeeder.cs`. It confirms no `BookTests.cs` exists yet.

### 2.2) Create the task-planner prompt file

Instead of manually switching between Developer-Specialist and Tester-Specialist for Problem 2, you will use a **prompt file** combined with **Plan mode** to automatically decompose the task and route each subtask to the right agent.

Create the file `.github/prompts/task-planner.prompt.md` in your project with the following content. You can create the file manually in VS Code, or ask Agent mode to create it for you:

**Path:** `.github/prompts/task-planner.prompt.md`

````markdown
---
description: "Break complex development tasks into smaller subtasks and route each to the appropriate specialized agent"
name: "task-planner"
argument-hint: "Describe your complex task"
---

# Task Planner

You are a task decomposition specialist. Your job is to:
1. **Break down** the complex task into focused, independent subtasks
2. **Route each subtask** to the most appropriate specialized agent (or default Copilot if none matches)
3. **Verify scope** - ensure subtasks don't overlap and boundaries are clear
4. **Create an actionable plan** - output a sequential checklist the user can follow

## Available Agents

Check `.github/agents/` directory to discover all available specialized agents. For each agent file:
- Extract the `name` field from frontmatter
- Extract the `description` field to understand when to use it
- Route subtasks based on what the agent specializes in

Generic fallback: Use **Default Copilot** if no specialized agent in `.github/agents/` matches the subtask.

## Decomposition Strategy

When breaking down the complex task:
1. **Scan `.github/agents/`** for available agents and their descriptions
2. **Identify which agent** best matches the subtask based on agent descriptions
3. **Route to most specialized agent** for the job (don't over-generalize)
4. **Fall back to Default Copilot** only if no specialized agent applies

## Output Format

Generate a **sequential checklist** with:

## Task Plan: [User's Task Title]

### Subtask 1: [Name]
- **Assigned to**: [Agent Name]
- **Boundary**: [What this task INCLUDES and EXCLUDES]
- **Dependencies**: [Other subtasks that must complete first, if any]
- **Action**: [What the user should ask the agent to do]

### Subtask 2: [Name]
- **Assigned to**: [Agent Name]
- **Boundary**: [What this task INCLUDES and EXCLUDES]
- **Dependencies**: [Prerequisites]
- **Action**: [What to ask]

### Hard Rules

> **Every subtask assigned to a custom agent MUST be executed by invoking that agent as a subagent using `runSubagent` with the exact `name` value from the agent's frontmatter (e.g., `Developer-Specialist`). The executor MUST NOT perform the subtask itself or delegate to a general-purpose agent when a custom agent is assigned. No exceptions - all assigned agents must be invoked as subagents.**

### Next Steps
- Verify all subtasks are complete
- Build and run integration tests if applicable
- Code review checklist

## Rules

- DO NOT combine subtasks across agent boundaries
- DO NOT create subtasks that require multiple agents working together (split into sequential steps with clear handoff points)
- DO NOT include testing/deployment in the plan unless explicitly requested
- ONLY use agents from the "Available Agents" list above, or "Default Copilot" if no specialized agent matches
- ONLY route to a specialized agent if that agent's description explicitly mentions the task type
- Each subtask should be completable in isolation by its assigned agent

## Hard Execution Rules (MANDATORY - apply when running the plan)

These rules govern how subtasks MUST be executed once the plan is approved. Include them verbatim in every generated plan under a "Hard Rules" section:

> **Every subtask assigned to a custom agent MUST be executed by invoking that agent as a subagent using `runSubagent` with the exact `name` value from the agent's frontmatter (e.g., `Developer-Specialist`). The executor MUST NOT implement the subtask itself using a general-purpose or default agent when a custom agent is assigned. No exceptions.**
>
> - Use the exact agent name as declared in `.github/agents/<agent-file>.agent.md` frontmatter (`name:` field).
> - Passing the task to a "general-purpose" agent when a custom agent is assigned is a plan violation.
> - Only use "Default Copilot" when NO custom agent in `.github/agents/` matches the subtask.

## Example

**User Input**: "Refactor the Order entity to use value objects and optimize its LINQ queries"

**Generated Plan**:

## Task Plan: Refactor Order Entity & Optimize LINQ

### Subtask 1: Extract Order Value Objects
- **Assigned to**: .NET Developer
- **Boundary**: Create `OrderStatus`, `OrderTotal` value objects. Refactor Order entity class. NO database migrations.
- **Dependencies**: None
- **Action**: "Create value object classes for Order and refactor the Order entity to use them"
- **Rationale**: .NET Developer handles C# class creation and entity refactoring

### Subtask 2: Optimize Order LINQ Queries
- **Assigned to**: EF Core LINQ Optimizer
- **Boundary**: Review DbContext queries for Order. Add .Include() for related entities. NO service layer changes.
- **Dependencies**: Subtask 1 (new value objects exist)
- **Action**: "Optimize all LINQ queries in DbContext that fetch Order entities. Look for N+1 problems and add eager loading."
- **Rationale**: EF Core LINQ Optimizer specializes in query performance optimization

### Hard Rules
- Every subtask assigned to a custom agent MUST be executed by invoking that agent as a subagent using `runSubagent`. The executor MUST NOT perform the subtask itself or delegate to a general-purpose agent when a custom agent is assigned. No exceptions - all assigned agents must be invoked as subagents.

### Next Steps
- Verify Order entity compiles and migrations are created
- Test OrderService queries return correct data
````

#### How the three elements work together

Here is how **task-planner + Plan mode + your task** interact to automatically route subtasks without you switching agents:

1. **Your task (the input):** you describe the full complex task in natural language - for example, "Add NumberOfPages to Book and write tests for it."

2. **task-planner prompt:** loaded alongside your input, it instructs Copilot to act as a task decomposition specialist. It scans `.github/agents/` to discover available agents and their descriptions, then assigns each subtask to the most appropriate agent, emitting a structured plan that includes Hard Rules requiring delegation via `runSubagent`.

3. **Plan mode:** switches Copilot into plan-before-act behavior. Instead of immediately editing files, Copilot generates the task plan for your review. You inspect the subtask assignments and click **Start** only when satisfied.

When you click **Start**, Copilot (acting as orchestrator) reads the approved plan and, for each subtask, calls `runSubagent` with the agent `name` declared in the plan. Each subagent runs with its own specialized system prompt and toolset, completes its subtask, and returns control to the orchestrator automatically - no manual agent switching required.

### 2.3) Use Plan mode + task-planner to solve Problem 2

Switch to **Plan** mode in the Copilot Chat dropdown. Then click the prompt file picker (the `/` button in the chat input) and select **task-planner** from the list.

**Prompt (to use together with the task-planner prompt file in Plan mode):**

```text
/task-planner
Add a NumberOfPages property to the Book entity and write tests for it.

Requirements:
- NumberOfPages is an integer property on Book.
- Domain invariant: NumberOfPages must be >= 1. Throw DomainException with message
  "NumberOfPages must be at least 1." if violated.
- Update Book.Create() and Book.Update() to accept and validate NumberOfPages.
- Update BooksController (Create and Edit actions) to include NumberOfPages in the model binding.
- Update Views/Books/Create.cshtml and Views/Books/Edit.cshtml to include a NumberOfPages field.
- Update Views/Books/Index.cshtml and Views/Books/Details.cshtml to display NumberOfPages.
- Update DataSeeder.cs to include a NumberOfPages value for all seeded books (use realistic values).
- In Src/BookStore.Tests, create BookTests.cs with tests that cover:
  - Book.Create() with valid NumberOfPages (e.g., 300) succeeds.
  - Book.Create() throws DomainException when NumberOfPages = 0.
  - Book.Create() throws DomainException when NumberOfPages is negative.
  - Book.Update() with a valid NumberOfPages succeeds.
- Run dotnet build Src/BookStore.slnx and dotnet test Src/BookStore.Tests - both must pass.
```

**Expected plan output:** the task-planner discovers `Developer-Specialist` and `Tester-Specialist` in `.github/agents/` and produces a plan similar to:

```
## Task Plan: Add NumberOfPages to Book + Tests

### Subtask 1: Implement NumberOfPages on the Book entity
- **Assigned to**: Developer-Specialist
- **Boundary**: Edit Book.cs, BooksController.cs, all Books views, DataSeeder.cs. NO test files.
- **Dependencies**: None
- **Action**: "Add NumberOfPages (int, >= 1, DomainException on violation) to the Book entity.
  Update Create() and Update() signatures, BooksController, all Books views, and DataSeeder.cs."

### Subtask 2: Write tests for Book.NumberOfPages
- **Assigned to**: Tester-Specialist
- **Boundary**: Create BookTests.cs in Src/BookStore.Tests and run dotnet test. NO entity code changes.
- **Dependencies**: Subtask 1 (NumberOfPages property exists on Book)
- **Action**: "Create BookTests.cs in Src/BookStore.Tests with tests for Book.Create
  with valid and invalid NumberOfPages values. Run dotnet test."

### Hard Rules
[...as defined in task-planner prompt...]
```

Review the plan. Once satisfied, click **Start** to execute.

**Expected outcome:** Copilot executes Subtask 1 by invoking Developer-Specialist as a subagent, then Subtask 2 by invoking Tester-Specialist as a subagent - without any manual agent switching. Both subtasks complete, the build passes, and all tests pass.

---

## Step 3 - Verify everything

```bash
dotnet run --project Src/BookStore.Web
```

| Check | Expected |
|---|---|
| `/Authors` index | "Age" column present with integer values for all authors |
| `/Authors/Details/1` | Age field displayed on the detail page |
| `/Books` index | "NumberOfPages" column present for all books |
| `/Books/Details/1` | NumberOfPages displayed on the detail page |
| `/Books/Create` with NumberOfPages = 0 | Validation error: "NumberOfPages must be at least 1." |
| `/Books/Create` with valid data | Book saved and shown with NumberOfPages |

Run the tests:

```bash
dotnet test Src/BookStore.Tests
```

All `AuthorTests` and `BookTests` should pass with 0 failures.

---

## Recap

| Step | Purpose | Copilot Mode | Outcome |
|---|---|---|---|
| **0** | Explore codebase | Ask mode (8 prompts) | Team alignment on entities, services, missing properties, no test project |
| **1.1** | Discover Author.Age gap | Ask mode | Confirmed: no Age property, no tests exist |
| **1.2** | Create Developer-Specialist | Agent mode + `/create-agent` | `.github/agents/developer-specialist.agent.md` created |
| **1.3** | Add Author.Age + views | Developer-Specialist agent | Computed Age property and updated Authors views |
| **1.4** | Create Tester-Specialist | Agent mode + `/create-agent` | `.github/agents/tester-specialist.agent.md` created |
| **1.5** | Test project + Author tests | Tester-Specialist agent | BookStore.Tests created, AuthorTests all pass |
| **2.1** | Discover Book.NumberOfPages gap | Ask mode | Confirmed: no NumberOfPages, no Book tests |
| **2.2** | Create task-planner prompt | Manual file creation | `.github/prompts/task-planner.prompt.md` created |
| **2.3** | Add NumberOfPages + tests | Plan mode + task-planner + subagents | Book updated, BookTests pass, no manual agent switching |
| **3** | Verify everything | Manual + `dotnet test` | All routes correct, all tests pass |

---

## Wrap-up

Commit the result to `feature/Module2`:

```bash
git checkout -b feature/Module2
git add .
git commit -m "feature/Module2: custom agents + subagents + prompt file + Age and NumberOfPages properties"
```

---

## References

- Custom agents in VS Code - <https://code.visualstudio.com/docs/agent-customization/custom-agents>
- Asking GitHub Copilot questions in your IDE (subagents) - <https://docs.github.com/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide>
- Custom agents and sub-agent orchestration - <https://docs.github.com/en/copilot/how-tos/copilot-sdk/features/custom-agents>
- Use prompt files in VS Code - <https://code.visualstudio.com/docs/copilot/customization/prompt-files>
- Planning with agents in VS Code - <https://code.visualstudio.com/docs/agents/planning>
- Copilot CLI plan mode best practices - <https://docs.github.com/copilot/how-tos/copilot-cli/cli-best-practices>
- Adding repository custom instructions for GitHub Copilot - <https://docs.github.com/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot>
- Copilot customization cheat sheet - <https://docs.github.com/en/copilot/reference/customization-cheat-sheet>
- Customize AI in Visual Studio Code overview - <https://code.visualstudio.com/docs/agent-customization/overview>
