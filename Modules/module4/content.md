# Module 4 - GitHub Copilot: MCP servers (Figma + PrimeVue), front-end agents and skills

**Goal:** connect GitHub Copilot to two external knowledge sources through **MCP servers** - the **Figma Remote MCP Server** (design specs) and the **PrimeVue MCP Server** (component API docs) - and then use two specialized custom agents plus two design-to-code skills to migrate the classic MVC + Razor + Bootstrap front-end to **MVC + Vue 3 + PrimeVue + Tailwind CSS v4**, rebuilding the Home page and the full Authors CRUD screen pixel-faithful to the approved Figma frames.

**Deliverable:** (1) `.vscode/mcp.json` with the `figma-mcp` (remote HTTP) and `primevue` (stdio) servers running and authenticated; (2) the `Frontend-Tooling-Specialist` and `Frontend-Specialist` agents, the `figma-discovery` and `primevue-component-build` skills, and the `frontend.instructions.md` / updated `web.instructions.md` / updated `copilot-instructions.md` in active use; (3) `Src/BookStore.Web/ClientApp` scaffolded (Vite 6 + Vue 3 + TypeScript + PrimeVue + Tailwind v4), Bootstrap removed, the Vite bundle served from `_Layout.cshtml`, the Home page rendered by a Vue component that matches the Figma design, and the **Authors** screen rebuilt as a paged Vue data table with Create / Edit / Details / Delete dialogs matching their Figma nodes.

---

## Prerequisites

✅ **Tools installed**

- VS Code (latest - MCP requires 1.99 or later)
- GitHub Copilot + GitHub Copilot Chat extensions, signed in
- .NET 10 SDK
- **Node.js 22+** (the PrimeVue MCP server requires Node 22+; the ClientApp build is verified on Node 20+/24)
- `ripgrep` and `fd` (used by the vault skills carried over from module 3)

```bash
code --version
dotnet --version
node -v
npm -v
brew install ripgrep fd
```

✅ **A Figma account with access to the BookStore prototype**

The Figma Remote MCP Server only reaches files the **authenticated user already has permission to open**. A **Dev seat** or **Full seat** is recommended for reading design context, variables and screenshots; View/Collab seats work but with much lower rate limits.

Design file used in this module:

```text
https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=39-20049&t=kh0vKKSKWMHQICkj-4
```

✅ **Repository checked out - `feature/Module3` branch**

Module 4 builds on the result of module 3 (populated vault, vault-aware agents, Customer CRUD).

```bash
git checkout feature/Module3
```

✅ **Quick start**

```bash
dotnet build Src/BookStore.slnx
dotnet run --project Src/BookStore.Web
```

Open <http://localhost:5045>. Today every page is a classic Razor view styled with Bootstrap 5.

---

## Problems to solve in this module

### Problem 1 - Copilot has no access to the design system or to the component library docs

The team has an approved Figma prototype for BookStore, but Copilot cannot read it. Asked to "build the Home page like the design", the agent can only guess colors, spacing and typography from a description. The same happens for the UI kit: without the PrimeVue documentation in context, the agent invents prop names, `pt` passthrough section names and theme tokens that do not exist.

There is **no `.vscode/mcp.json`** in the repository, so no external tool is reachable from chat.

**Fix:** configure two MCP servers in `.vscode/mcp.json` - the Figma Remote MCP Server (`https://mcp.figma.com/mcp`, HTTP + OAuth) and the PrimeVue MCP Server (`npx -y @primevue/mcp`, stdio) - start them, authenticate to Figma, and smoke-test both from chat.

### Problem 2 - There is no front-end specialization in the customization layer

Modules 2 and 3 produced back-end oriented agents (`Developer-Specialist`, `Tester-Specialist`) and vault skills. Nothing in `.github/` knows how to translate a Figma node into a Vue component, which PrimeVue design tokens map to which Figma color, where components live, or who owns the build tooling versus who owns the components.

**Fix:** put the front-end customization layer to work. It is **already committed in this repository** and only needs to be read, understood and used:

- `.github/agents/frontend-tooling-specialist.agent.md` - owns the build/tooling layer and `.vscode/mcp.json`.
- `.github/agents/frontend-specialist.agent.md` - owns Vue components and Figma fidelity.
- `.github/skills/figma-discovery/SKILL.md` - turns a Figma URL into an implementable spec.
- `.github/skills/primevue-component-build/SKILL.md` - implements/restyles a component with PrimeVue + Tailwind.
- `.github/instructions/frontend.instructions.md` (new) and `.github/instructions/web.instructions.md` (updated), plus an updated `.github/copilot-instructions.md` and `developer-specialist.agent.md`.

### Problem 3 - The front-end is classic Razor + Bootstrap and does not match the design

`Src/BookStore.Web` renders everything server-side with Bootstrap 5 (`wwwroot/lib/bootstrap/`, `card`, `btn btn-primary`, `bi-*` icons). There is no `ClientApp`, no component model, no JSON API for a front-end to consume, and `Views/Home/Index.cshtml` is a two-card Bootstrap grid that looks nothing like the approved Figma Home frame.

**Fix:** have `Frontend-Tooling-Specialist` migrate the project to MVC + Vue 3 islands (Vite, PrimeVue, Tailwind v4, Bootstrap removal, `/api/*` layer), then have `Frontend-Specialist` rebuild the Home page and the full Authors screen (paged table + row-actions menu + Create / Edit / Details / Delete dialogs) from their Figma nodes through `/figma-discovery` + `/primevue-component-build`.

---

## GitHub Copilot features used in this module

### MCP servers (`.vscode/mcp.json`)

The **Model Context Protocol** is an open standard for connecting AI agents to external tools and data. In VS Code you declare servers in `.vscode/mcp.json` (workspace scope, shareable through source control) or in your user profile. Each server exposes **tools** that Copilot can call in **Agent** mode, plus optional resources and prompts. Servers can be **stdio** (a local process launched by a command, e.g. `npx`) or **http** (a remote endpoint, usually behind OAuth). VS Code shows Start/Stop/Restart code lenses directly in `mcp.json`, and `MCP: List Servers` in the Command Palette lets you inspect logs and tools.

> Reference: [Add and manage MCP servers in VS Code](https://code.visualstudio.com/docs/agent-customization/mcp-servers) - [Extending Copilot Chat with MCP servers (GitHub Docs)](https://docs.github.com/en/copilot/how-tos/provide-context/use-mcp/extend-copilot-chat-with-mcp)

### Custom agents (`.agent.md`)

A custom agent is a Markdown file in `.github/agents/` with YAML frontmatter (`name`, `description`, `tools`) and a body that defines a persona, its workflow and its constraints. The `description` is what makes an agent **discoverable**: Copilot reads it to decide whether the agent fits the task, and you can also invoke it explicitly. Restricting `tools` limits what the agent may do. In this module two agents split the front-end responsibility so neither one drifts outside its lane.

> Reference: [Custom agents in VS Code](https://code.visualstudio.com/docs/agent-customization/custom-agents)

### Agent Skills (`SKILL.md`)

Skills are folders of instructions and resources loaded **on demand**: Copilot first reads only `name` + `description`, then the `SKILL.md` body when the task matches, then referenced resource files only if needed. They package repeatable, task-specific procedures. Here they encode the two halves of design-to-code: `figma-discovery` (extract and translate the spec) and `primevue-component-build` (implement it in the codebase's conventions).

> Reference: [Use Agent Skills in VS Code](https://code.visualstudio.com/docs/agent-customization/agent-skills) - [About agent skills (GitHub Docs)](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills)

### Custom instructions (`copilot-instructions.md` + `*.instructions.md`)

Repository instructions apply to every prompt; scoped `.instructions.md` files apply only when files matching their `applyTo` glob are in context. In this module `frontend.instructions.md` (scoped to `ClientApp/**`) and `web.instructions.md` (scoped to controllers, views, `Program.cs`) carry the conventions that keep the migration consistent, so the agent files themselves stay short.

> Reference: [Use custom instructions in VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-instructions) - [Add repository instructions (GitHub Docs)](https://docs.github.com/en/copilot/how-tos/copilot-on-github/customize-copilot/add-custom-instructions/add-repository-instructions)

---

## Background: what we are plugging into Copilot

### The Figma MCP Server

The Figma MCP Server connects an MCP client (VS Code, Copilot CLI, Claude Code, Cursor, Codex) to Figma and gives the agent **structured design context** - layout, components, variables, screenshots - instead of a flat image. Figma recommends the **Remote MCP Server**:

```text
https://mcp.figma.com/mcp
```

It is **link-based**. You paste a normal Figma link into the prompt:

```text
https://www.figma.com/design/:fileKey/:fileName?node-id=39-20049
```

The client extracts `fileKey` and `node-id` (converting `39-20049` to `39:20049`) and asks the server for that node. It never "browses" the URL like a browser.

```text
Figma URL -> node-id -> Figma MCP -> design context -> coding agent -> code
```

Main tools exposed by the server:

| Tool | What it returns |
|---|---|
| `get_metadata` | Node tree/structure summary - cheap way to confirm you targeted the right frame |
| `get_screenshot` | A rendered image of the node |
| `get_design_context` | The full structured spec (layout, styles, hierarchy) |
| `get_variable_defs` | Named design variables behind raw color/spacing values |

Access is governed by your Figma **plan, seat and file permissions**: reading design context, variables and screenshots is recommended on a **Dev** or **Full** seat; writing to a Figma file requires a Full seat plus edit permission. The server can only reach what the authenticated user can already open, and View/Collab seats get much lower rate limits.

> References: [Figma MCP Server](https://developers.figma.com/docs/figma-mcp-server/) - [Remote server installation](https://developers.figma.com/docs/figma-mcp-server/remote-server-installation/) - [Tools and prompts](https://developers.figma.com/docs/figma-mcp-server/tools-and-prompts/) - [Rate limits and access](https://developers.figma.com/docs/figma-mcp-server/rate-limits-access/)

### PrimeVue

**PrimeVue** is PrimeTek's UI component library for Vue 3: DataTable, Dialog, Button, Select, DatePicker, Menu, Paginator, forms, theming and accessibility out of the box. Installation with Vite:

```bash
npm install primevue @primeuix/themes
```

```ts
import PrimeVue from 'primevue/config';
import Aura from '@primeuix/themes/aura';

app.use(PrimeVue, { theme: { preset: Aura, options: { darkModeSelector: false } } });
```

**Licensing matters for your team.** PrimeVue 4 and earlier remain MIT. The current generation (PrimeUI / PrimeVue 5) uses a license key: **Community** is free while the organization stays under all of the eligibility limits (annual revenue under US$ 1M, fewer than 5 developers, fewer than 10 employees, never raised more than US$ 3M), otherwise a **Commercial** license applies (US$ 599/dev promotional until 31/12/2026, US$ 799/dev standard; perpetual, 1 year of updates included). This repository pins **PrimeVue 4.3**, which is MIT.

> References: [PrimeVue](https://primevue.dev/) - [Install PrimeVue with Vite](https://primevue.dev/vite/) - [PrimeUI pricing](https://primeui.dev/pricing) - [Community license](https://primeui.dev/licenses/community)

### The PrimeVue MCP Server

`@primevue/mcp` is PrimeTek's official **read-only** MCP server. It lets the agent query the PrimeVue documentation directly: components, APIs, props and events, guides, examples and setup. It exposes 8 tools including `search`, `get_component`, `get_example`, `get_setup` and `validate_usage`, and requires **Node.js 22+**.

```text
GitHub Copilot -> @primevue/mcp -> PrimeVue docs -> components / API / examples
```

PrimeTek also ships a **PrimeVue Plugin** that installs the MCP server **plus 7 PrimeVue skills** (implementation, setup, theming, Tailwind, accessibility, migration, troubleshooting) in one command:

```bash
pnpm dlx @primeui/cli plugin install --tool copilot --library primevue
```

Installed for the Copilot CLI, the plugin is automatically discovered by VS Code. In this module we configure the **MCP server only**, because the repository already ships its own design-system skills (`figma-discovery`, `primevue-component-build`) tailored to BookStore's conventions.

> References: [PrimeVue MCP Server](https://primevue.dev/mcp/) - [PrimeVue Plugin](https://primevue.dev/plugin)

### Tailwind CSS v4 and how it integrates with PrimeVue

Tailwind is a **utility-first** CSS framework: instead of authoring a class per component, you compose small utilities in the markup (`flex`, `p-4`, `rounded-lg`), with built-in responsive and state variants (`md:`, `hover:`, `dark:`). With Vite:

```bash
npm install tailwindcss @tailwindcss/vite
```

```ts
// vite.config.ts
import tailwindcss from '@tailwindcss/vite'
export default { plugins: [tailwindcss()] }
```

```css
/* src/style.css */
@import "tailwindcss";
```

**Tailwind v4 is CSS-config based - there is no `tailwind.config.js`.** Everything (imports, content sources via `@source`, `@layer components`) lives in the CSS file.

The bridge to PrimeVue is the **`tailwindcss-primeui`** plugin, which exposes PrimeVue's theme tokens as Tailwind utilities. That is what lets the codebase write `bg-surface-0`, `border-surface-300`, `text-muted-color`, `bg-primary`, `text-primary-contrast` instead of raw hex values, so a theme change propagates everywhere:

```css
@import "tailwindcss";
@import "tailwindcss-primeui";
@import "primeicons/primeicons.css";
@source "../../Views";
```

Two conventions follow from this integration and are enforced by the instructions files:

- Style PrimeVue components through their **`pt` (passthrough)** sections (`root`, `header`, `content`, `footer`, `page`, ...) rather than a bare `class`.
- Tailwind v4 `!important` is **postfix** (`bg-surface-700!`), and is only needed when overriding a PrimeVue theme-colored state class, because PrimeVue injects its stylesheet after Tailwind.

> References: [Tailwind CSS docs](https://tailwindcss.com/docs) - [Tailwind CSS v4](https://tailwindcss.com/blog/tailwindcss-v4) - [Framework guides](https://tailwindcss.com/docs/installation/framework-guides)

---

## Solution layout (what is on the `feature/Module3` branch)

```
.github/
├── copilot-instructions.md                   # already updated for the Vue/PrimeVue target state
├── agents/
│   ├── developer-specialist.agent.md         # already updated
│   ├── tester-specialist.agent.md
│   ├── frontend-tooling-specialist.agent.md  # READY - build/tooling + MCP owner
│   └── frontend-specialist.agent.md          # READY - component + Figma owner
├── instructions/
│   ├── pagination.instructions.md, vault.instructions.md
│   ├── web.instructions.md                   # already updated (MVC hosting Vue + /api contract)
│   └── frontend.instructions.md              # READY - ClientApp conventions
└── skills/
    ├── vault-search/, vault-write/, vault-full-sync/
    ├── figma-discovery/SKILL.md              # READY
    └── primevue-component-build/SKILL.md     # READY

Src/BookStore.Web/
├── Controllers/    Books, Authors, Customers, Home          (no Controllers/Api yet)
├── Views/          Razor + Bootstrap 5 everywhere
├── wwwroot/lib/    bootstrap/, jquery/, jquery-validation/  (Bootstrap still present)
└── (no ClientApp/, no wwwroot/dist/)

.vscode/
└── (no mcp.json yet)                          <- Problem 1
```

Everything under `.github/` is **already authored and committed**. Module 4 is about **understanding it and using it** - you will not write those files by hand.

---

## Step 0 - Understand the project and the customization layer (Ask mode only)

Switch the Copilot Chat dropdown to **Ask** for every prompt in this step. Ask is read-only - no file is modified. The goal is shared understanding of where the front-end stands today and what the pre-built customization layer is prepared to do.

### 0.1) The front-end stack today

**Prompt:**

```text
Describe how the user interface of the web project in this solution is rendered today.
Which CSS framework and JavaScript libraries are used, where are they referenced,
and is there any client-side build step (bundler, npm project) in the repository?
```

**Expected outcome:** Copilot identifies classic Razor views, Bootstrap 5 under `wwwroot/lib/bootstrap/`, jQuery + jquery-validation, no bundler, no npm project.

### 0.2) The shared layout and static assets

**Prompt:**

```text
Show me the shared layout used by every page in the web project.
Which stylesheets and scripts does it load, in what order, and what does the navigation look like?
Also list what is inside the static assets folder.
```

**Expected outcome:** Copilot describes `Views/Shared/_Layout.cshtml`, the Bootstrap CSS/JS references, the navbar, `wwwroot/css/site.css`, `wwwroot/js/site.js` and `wwwroot/lib/`.

### 0.3) The Home page

**Prompt:**

```text
Describe the Home page of the web application: what markup does it render,
what components or cards does it show, and which CSS classes drive its styling?
```

**Expected outcome:** Copilot shows the Bootstrap two-card grid ("Books" and "Authors") with `card`, `btn btn-primary`, `display-4`, `lead`.

### 0.4) How the pages get their data

**Prompt:**

```text
How do the index pages of this application receive their data today?
Do the controllers pass a model to the view, or is there any JSON/REST endpoint that a
JavaScript client could consume? Are there DTOs anywhere?
```

**Expected outcome:** Copilot confirms server-side rendering with `@model PagedResult<T>`, no `/api/*` endpoints, no DTOs - which is exactly the gap a Vue front-end would need filled.

### 0.5) The custom agents available in the repository

**Prompt:**

```text
List every custom agent defined in this repository. For each one, give me its name,
its description, the tools it is allowed to use, and a one-line summary of what it owns.
Which of them are responsible for front-end work, and how is the responsibility split between them?
```

**Expected outcome:** Copilot lists `Developer-Specialist`, `Tester-Specialist`, `Frontend-Tooling-Specialist`, `Frontend-Specialist` (and the content-authoring agent), and explains the split: tooling/build/MCP versus components/Figma.

### 0.6) The front-end skills

**Prompt:**

```text
Which agent skills in this repository deal with design systems or front-end components?
For each one, explain what it does, when it should be triggered, and what it produces as output.
Which one must run first when a task references a design file?
```

**Expected outcome:** Copilot describes `figma-discovery` (runs first, produces a discovery report, never code) and `primevue-component-build` (implements from that report), plus the vault skills from module 3.

### 0.7) The front-end instruction files

**Prompt:**

```text
Find the instruction files that describe front-end and web-hosting conventions in this repository.
What glob patterns do they apply to, and summarize the rules they define about folder structure,
styling tokens, the build loop, and how Razor views host client-side components.
```

**Expected outcome:** Copilot summarizes `frontend.instructions.md` (`ClientApp/**`) and `web.instructions.md` (views/controllers/`Program.cs`): the ClientApp folder layout, the design-token rules, `npm run build` not being part of `dotnet build`, and the mount-point + `/api/*` contract.

### 0.8) MCP configuration status

**Prompt:**

```text
Does this repository configure any MCP server for the editor? If yes, show the configuration
and describe each server. If not, tell me where that configuration file is expected to live
and which agent in this repository is responsible for creating it.
```

**Expected outcome:** Copilot reports that `.vscode/mcp.json` does not exist yet, and that `Frontend-Tooling-Specialist` explicitly owns it (its description and body both mention configuring/verifying the PrimeVue and Figma MCP servers).

---

## Step 1 - Problem 1: Give Copilot access to Figma and PrimeVue through MCP

### 1.1) Discover the gap (Ask mode)

Stay in **Ask** mode.

**Prompt:**

```text
If I ask you right now to implement a screen from our Figma prototype using PrimeVue components,
what information would you be missing? Can you read a Figma file, or query the PrimeVue component
API from this workspace? Explain what would have to be configured first and why guessing
colors, spacing and component props would be a bad idea.
```

**Expected outcome:** Copilot states that it has no tool to reach Figma or the PrimeVue docs, that it would have to invent design values and prop names, and that MCP servers declared in `.vscode/mcp.json` are the missing piece.

### 1.2) Fix using the `Frontend-Tooling-Specialist` agent (Agent mode)

The agent that owns `.vscode/mcp.json` is `Frontend-Tooling-Specialist`. Read it first so everyone sees what is being invoked - here is its frontmatter and the MCP section of its body:

```markdown
---
name: Frontend-Tooling-Specialist
description: "Use when: installing or configuring front-end tooling in BookStore.Web (Vite, Vue 3,
  PrimeVue, Tailwind CSS v4, tailwindcss-primeui, PrimeIcons), scaffolding the ClientApp project in a
  project being migrated from classic MVC + Razor + Bootstrap, removing Bootstrap, wiring the Vite
  bundle into _Layout.cshtml, adding JSON API endpoints/DTOs that feed Vue components, or
  configuring/verifying the PrimeVue and Figma MCP servers in .vscode/mcp.json.
  Owns the build/tooling layer - component work belongs to Frontend-Specialist."
tools: [read, edit, search, execute, web]
---
```

```markdown
- `.vscode/mcp.json`:
  {
      "servers": {
          "primevue":  { "command": "npx", "args": ["-y", "@primevue/mcp"] },
          "figma-mcp": { "type": "http", "url": "https://mcp.figma.com/mcp" }
      }
  }
  Verify both entries before any PrimeVue/Figma work; recreate the file with this exact shape if
  missing or malformed. The `primevue` server is stdio (`npx` fetches it on demand, needs network the
  first time); `figma-mcp` is remote HTTP and requires the user to complete the Figma OAuth prompt in
  the IDE - if tools return auth errors, tell the user to re-authenticate, do not work around it.
```

Note the `description`: it is written so Copilot can route MCP/tooling requests to this agent automatically. It also carries the **Mandatory Vault Workflow** from module 3, so it will search and update the vault around the change.

Switch to **Agent** mode, select the **Frontend-Tooling-Specialist** agent in the agent picker, and give it a deliberately **narrow** instruction so nothing else in the project is touched yet:

**Prompt:**

```text
Configure ONLY the .vscode/mcp.json file: add the Figma remote MCP server and the PrimeVue MCP server.
Do not change anything else in the project - no dependencies, no ClientApp, no Razor views, no C# code.
```

**Expected outcome:** the agent creates `.vscode/mcp.json` with exactly two servers and reports that nothing else changed:

```json
{
    "servers": {
        "primevue": {
            "command": "npx",
            "args": ["-y", "@primevue/mcp"]
        },
        "figma-mcp": {
            "type": "http",
            "url": "https://mcp.figma.com/mcp"
        }
    }
}
```

Why two different shapes: `primevue` is a **stdio** server - VS Code launches a local process with `npx` (which downloads `@primevue/mcp` on first use, so the first start needs network access and Node 22+). `figma-mcp` is a **remote HTTP** server - VS Code talks to Figma's hosted endpoint and negotiates OAuth.

### 1.3) Start both servers and authenticate to Figma

1. Open `.vscode/mcp.json`. VS Code shows a **Start** code lens above each server - click it for both. Confirm the **trust** prompt (a stdio server runs code on your machine, so only start servers you trust).
2. Starting `figma-mcp` triggers the Figma **OAuth** flow. Approve it in the browser with the account that has access to the BookStore prototype. If the file is not shared with that account, the tools will return a permission error - fix the sharing, do not work around it.
3. Verify: open the Command Palette and run **MCP: List Servers**. Both `primevue` and `figma-mcp` must appear as running. Use **Show Output** on a server to inspect its log if it fails to start.
4. In the Chat view, open **Configure Tools** and confirm the new tool groups appear (Figma: `get_metadata`, `get_screenshot`, `get_design_context`, `get_variable_defs`; PrimeVue: `search`, `get_component`, `get_example`, `get_setup`, `validate_usage`, ...).

> MCP tools are only callable in **Agent** mode.

### 1.4) Smoke-test the Figma MCP server (Agent mode)

**Prompt:**

```text
Give me the details of this Figma component: https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=39-20049&t=kh0vKKSKWMHQICkj-4
Use the Figma MCP server.
```

**Expected outcome:** Copilot calls the Figma tools (you may be asked to approve each call), converts `node-id=39-20049` into `39:20049`, and returns the real structure of the **Home** frame: layout, sections, colors, typography, and optionally a screenshot. If it answers without calling a tool, say so explicitly ("call the Figma MCP tools, do not guess") - the point of the exercise is the tool call.

### 1.5) Smoke-test the PrimeVue MCP server (Agent mode)

**Prompt:**

```text
Give me the details of the PrimeVue Button component. Use the PrimeVue MCP server.
```

**Expected outcome:** Copilot returns the Button API straight from the PrimeVue docs: props (`label`, `icon`, `severity`, `outlined`, `text`, `raised`, `rounded`, `loading`), events, slots, `pt` passthrough sections and usage examples. Compare it with what the model would produce from memory - this is the difference between a documented answer and a plausible-looking guess.

---

## Step 2 - Problem 2: Put the front-end customization layer to work

Everything in this step **already exists in the repository**. The exercise is to read it, understand the division of responsibility, and confirm Copilot picks it up.

### 2.1) Discover what is available (Ask mode)

Switch to **Ask** mode.

**Prompt:**

```text
I want to implement a screen from a Figma design using PrimeVue in this repository.
Walk me through the exact workflow the repository expects: which agent should I invoke,
which skills will run and in what order, which instruction files will be applied,
and what each of them contributes. Point out which rules are non-negotiable.
```

**Expected outcome:** Copilot describes the chain: `Frontend-Specialist` -> `/figma-discovery` (first) -> `/primevue-component-build`, with `frontend.instructions.md` and `web.instructions.md` applied by `applyTo`, plus the mandatory vault workflow. Non-negotiables: no raw hex where a token exists, no Bootstrap, `pt` over bare `class`, server-side paging only, `npm run build` after every ClientApp change.

### 2.2) The `figma-discovery` skill

Open `.github/skills/figma-discovery/SKILL.md`. Key points to show the class:

```markdown
---
name: figma-discovery
description: "Discovers and extracts design specs from Figma for a Vue/PrimeVue front-end.
  Use when: the user gives a Figma URL or node reference, asks to implement/match a design ...
  Parses Figma URLs, calls the Figma MCP tools, and maps raw values to the codebase's existing
  PrimeVue/Tailwind design-token conventions instead of raw hex/pixel values."
---
```

Its procedure is the interesting part:

1. **Parse the reference** - extract `fileKey` and `nodeId` from the URL, converting `39-21141` to `39:21141`.
2. **Confirm the target** - `get_metadata` / `get_screenshot` first (cheap, catches a wrong node id early).
3. **Pull the real spec** - `get_design_context`, treated as a *reference to adapt*, never as final code.
4. **Resolve ambiguous values** - `get_variable_defs` to turn a raw color into a named variable.
5. **Check for an existing match first** - reuse an existing component/pattern before introducing a new value.
6. **Identify assets** - PrimeIcons when an equivalent glyph exists; real images exported to `wwwroot/images/<page>/`.
7. **Map interaction and data** - what is static design versus data-bound to `/api/*`.
8. **Report** - a discovery report handed to `/primevue-component-build`. **The skill never writes component code.**

And the piece that makes the output usable, the **token mapping table**:

| Figma value observed | Use this Tailwind/PrimeVue token |
|---|---|
| `#334155` (dark slate) | `surface-700` |
| `#1e293b` (darker slate, hover) | `surface-800` |
| `#cbd5e1` / light gray borders | `border-surface-300` |
| `#f8fafc` / very light row hover | `surface-50` / `surface-100` |
| `#ffffff` panel background | `bg-surface-0` |
| `#64748b` muted gray text | `text-muted-color` |
| primary body text | `text-color` |
| brand/primary action fill | `bg-primary` + `text-primary-contrast` |
| icon glyphs | PrimeIcons `pi pi-*` |

This table is what prevents the classic failure mode of design-to-code: a wall of hardcoded hex values that ignores the theme.

**Try it (Ask mode):**

```text
Read the Figma discovery skill in this repository and explain, step by step, what it would do
with a Figma URL, which MCP tools it calls in which order, and why it deliberately produces a
report instead of code.
```

### 2.3) The `primevue-component-build` skill

Open `.github/skills/primevue-component-build/SKILL.md`. It is the implementation half:

- **Placement rules** - `components/<Feature>Page.vue`, `components/<Feature>Table.vue`, or `components/common/<category>/` for anything reused by 2+ pages.
- **Reuse before creating** - `composables/usePagedFetch.ts`, `useEntityCrud.ts`, `useCreateEntity.ts`, `styles/buttonStyles.ts`, `common/dialog/dialogStyles.ts`, `DataTableCommon.vue`, `FormField.vue`, `utils/format.ts`.
- **Mount contract** - every `data-*` prop from Razor arrives as a `string`; never hardcode a URL.
- **Tokens, not hex**; **`pt` sections, not bare `class`**; Tailwind arbitrary values are allowed when the design does not land on the scale; `!important` is postfix.
- **Bootstrap audit** before finishing, with the important nuance that `btn`, `form-control` and `alert-danger` in Razor are **custom `@layer components` classes**, not Bootstrap remnants.
- **Verify**: `npx vue-tsc --noEmit`, `npm run build`, `dotnet build` when a `.cshtml`/DTO changed.
- **Record it** with `/vault-write`.

### 2.4) The `Frontend-Specialist` agent

```markdown
---
name: Frontend-Specialist
description: "Use when: creating or modifying Vue 3 SFC components inside Src/BookStore.Web/ClientApp/src,
  implementing or matching a Figma design through the Figma MCP server, refactoring existing UI to align
  with the Figma design system, styling with PrimeVue design tokens + Tailwind CSS v4 ...
  Component-level front-end work only - build/tooling/MCP setup belongs to Frontend-Tooling-Specialist."
tools: [read, edit, search, execute, web]
---
```

Its body declares the two skills it owns and, crucially, the **boundaries**:

- `DO NOT change front-end build tooling, vite.config.ts, package.json dependencies, or .vscode/mcp.json` - that is the tooling agent's scope.
- `DO NOT invent design values when a Figma node exists - pull it through the MCP server, and say so if the tool call fails rather than guessing.`
- `DO NOT delete Razor CRUD views as a side effect of a component task - ask first.`

The mirror-image constraint lives in `Frontend-Tooling-Specialist`: `DO NOT author or restyle Vue components`. Two agents, one seam, no overlap - this is what keeps a large migration from turning into an uncontrolled rewrite.

### 2.5) The instruction files (already updated for you)

| File | Status | What it carries |
|---|---|---|
| `.github/copilot-instructions.md` | **updated** | Describes the target stack (MVC + Vue 3 + PrimeVue + Tailwind, Bootstrap removed, jQuery only for unobtrusive validation) and points to the two front-end instruction files. Reminds that `npm run build` is **not** part of `dotnet build`. |
| `.github/instructions/frontend.instructions.md` | **new** | `applyTo: Src/BookStore.Web/ClientApp/**` - the ClientApp reference implementation: stack table, folder layout, the `mount()`/dataset-as-props contract, the three composables, styling rules, Figma workflow, definition of done, and a "Porting to a new project" playbook. |
| `.github/instructions/web.instructions.md` | **updated** | `applyTo: views, controllers, models, Program.cs` - the hosting model (MVC islands, not an SPA), the `_Layout.cshtml` bundle contract, how to write a Razor view that hosts a Vue component, and the `/api/*` JSON controller conventions (reuse services, always DTO, `DomainException` -> `BadRequest`). |
| `.github/agents/developer-specialist.agent.md` | **updated** | Keeps the module 3 vault workflow and hands front-end work to the front-end agents instead of editing Razor/Bootstrap itself. |

**Confirm Copilot sees them (Ask mode):**

```text
Which custom instruction files would be applied if I open a file inside the ClientApp folder,
and which ones if I open a Razor view or an API controller? Quote the applyTo pattern of each.
```

**Expected outcome:** Copilot names `frontend.instructions.md` for `ClientApp/**` and `web.instructions.md` for `*.cshtml` / `Controllers/**` / `Models/**` / `Program.cs`, plus the always-on `copilot-instructions.md`.

---

## Step 3 - Problem 3: Migrate to MVC + Vue 3 and rebuild Home from Figma

### 3.1) Discover the gap (Agent mode)

Switch to **Agent** mode.

**Prompt:**

```text
Compare the Home page currently rendered by this application with the approved design at
https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=39-20049
Describe, at a high level, everything that would have to change in the project before that design
could be implemented: build tooling, dependencies, how the layout loads assets, how the view hosts
the UI, and where the data would come from.
```

**Expected outcome:** Copilot lists the missing pieces - no ClientApp/Vite project, no Vue/PrimeVue/Tailwind dependencies, Bootstrap still loaded in `_Layout.cshtml`, no mount point in `Views/Home/Index.cshtml`, no `/api/*` endpoints - and confirms this is exactly the split between the two front-end agents.

### 3.2) Fix with both agents in sequence (Agent mode)

Switch to **Agent** mode. This single prompt drives both agents: the tooling agent does the platform migration, then the component agent rebuilds Home from Figma.

**Prompt:**

```text
Use the Frontend-Tooling-Specialist agent to migrate this .NET MVC project to MVC + Vue.js,
installing and configuring PrimeVue and Tailwind CSS.
Then use the Frontend-Specialist agent to migrate the Home page to the new format, matching this design:
https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=39-20049&t=kh0vKKSKWMHQICkj-4

Add the side bar menu as well https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=28-16662&t=kh0vKKSKWMHQICkj-4
```

If you prefer full control, run it as two turns - select `Frontend-Tooling-Specialist` in the agent picker for the first half, then `Frontend-Specialist` for the second.

**What the tooling agent should produce:**

- `Src/BookStore.Web/ClientApp/` - Vite 6 + Vue 3.5 + TypeScript, **not** part of `BookStore.slnx` and **not** built by `dotnet build`.
  - dependencies: `vue`, `primevue`, `@primeuix/themes`, `primeicons`
  - devDependencies: `vite`, `@vitejs/plugin-vue`, `tailwindcss`, `@tailwindcss/vite`, `tailwindcss-primeui`, `typescript`, `vue-tsc`
- `vite.config.ts` with `base: "/dist/"`, `build.outDir` -> `../wwwroot/dist`, fixed `main.js` / `main.css` output names.
- `src/style.css` with the three `@import`s, `@source "../../Views";` and a `@layer components` shim for the Bootstrap class names the surviving Razor forms still use.
- `src/main.ts` with the `mount(Component, "#<feature>-app")` helper that copies `el.dataset` into props and registers PrimeVue with the Aura preset.
- `_Layout.cshtml` loading `~/dist/main.css` and `~/dist/main.js` (`type="module"`, `asp-append-version="true"`) **globally, once**.
- Bootstrap removed: `wwwroot/lib/bootstrap/` deleted, every Bootstrap `<link>`/`<script>` gone, `bi-*` icons replaced by PrimeIcons. jQuery + jquery-validation stay **only** for unobtrusive validation on the remaining Razor forms.
- `Controllers/Api/*ApiController.cs` + `Models/Api/*Dto.cs` - thin `[ApiController]` endpoints reusing the existing services, mapping to DTOs (entities have circular `Book.Author` / `Author.Books` navigations), with `DomainException` -> `BadRequest(new { message })`.

**What the component agent should produce:**

- A `/figma-discovery` run against node `39:20049` **before** any code, producing a discovery report with mapped tokens, spacing, typography and assets.
- `ClientApp/src/components/Home.vue` implementing that report with PrimeVue components, token utilities and PrimeIcons.
- Exported images under `Src/BookStore.Web/wwwroot/images/home/`, referenced by absolute URL (`/images/home/...`), not imported through Vite.
- `Views/Home/Index.cshtml` reduced to a mount point:
  ```cshtml
  <div id="home-app" data-books-url="@Url.Action("Index", "Books")" ...></div>
  ```
- A `mount(Home, "#home-app")` line in `src/main.ts`.
- A `/vault-write` run recording the new front-end stack, the Figma node -> component mapping and any new token mapping.

> If the Figma tool calls fail with an auth error, stop and re-authenticate (Step 1.3). Both the skill and the agent are instructed to **never** invent design values as a fallback.

### 3.3) Build and run

The front-end build is **not** wired into `dotnet build`, so it must be run explicitly:

```bash
cd Src/BookStore.Web/ClientApp
npm install
npx vue-tsc --noEmit
npm run build          # emits ../wwwroot/dist/main.js + main.css (+ PrimeIcons fonts)

cd ../../..
dotnet build Src/BookStore.slnx
dotnet run --project Src/BookStore.Web
```

Open <http://localhost:5045>.

> `wwwroot/dist/` is generated **and committed on purpose**: `dotnet publish` and `deploy.sh` never run npm, so the deployed app would ship without a front-end otherwise. Commit the rebuilt bundle together with the source change. `ClientApp/node_modules/` stays ignored.

### 3.4) Compare against the design (Agent mode)

**Prompt:**

```text
Take a screenshot of the Figma node 39:20049 through the Figma MCP server and compare it with the
Home.vue component that was just implemented. List every difference you find in layout, spacing,
colors and typography, and tell me which ones are real deviations versus deliberate adaptations
to our design tokens.
```

**Expected outcome:** a concrete difference list. Fix any real deviation with `Frontend-Specialist` (not by hand), then re-run `npm run build`.

---

## Step 4 - Migrate the Authors page: a full CRUD screen from five Figma nodes

Home is a static page - one frame, no data, no interaction. The Authors page is the real test: a **paginated data table** plus **four modals**, all data-bound to `/api/authors`. It is also where the skills pay off, because a single prompt now carries five Figma nodes and the agent has to keep them consistent with each other and with the codebase conventions.

### 4.1) Discover the current Authors page (Ask mode)

Switch to **Ask** mode.

**Prompt:**

```text
Describe the Authors screen of this application as it exists right now: the controller action,
the view, how the list is paginated, and how the create, edit, details and delete operations are
presented to the user. How many separate page loads does a user go through to delete an author?
Then tell me which JSON endpoints exist for authors after the migration, and what a Vue component
would need in order to replace those Razor pages with dialogs.
```

**Expected outcome:** Copilot describes the Razor `Index/Create/Edit/Details/Delete` views with server-side navigation between them, and confirms that `/api/authors` (paged), `/api/authors/{id}` (GET/PUT/DELETE) and `POST /api/authors` already exist from Step 3 - so everything the dialogs need is in place.

### 4.2) Fix using `Frontend-Specialist` with all five Figma nodes (Agent mode)

Switch to **Agent** mode and select the **Frontend-Specialist** agent. This prompt hands the agent the whole screen at once - the page plus its four modals - and states the contract explicitly so `/figma-discovery` runs per node before any code is written.

**Prompt:**

```text
Migrate the Authors screen to the new Vue + PrimeVue + Tailwind format, replacing the Razor pages
with a data table and dialogs. Implement it faithfully to the design.

Run /figma-discovery for EVERY node below before writing any code, in this order, and produce one
consolidated discovery report. Do not guess any color, radius, spacing or typography value: pull it
from the Figma MCP server, and map it to our existing PrimeVue/Tailwind design tokens.

Figma nodes:
- Authors page (list + header + paginator):
  https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=14-8860
- Row-actions popup menu on the table:
  https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=44-23160
- Create author modal:
  https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=12-8767
- Edit author modal:
  https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=44-23181
- Delete author confirmation modal:
  https://www.figma.com/design/2zyEr3S75NxHIJ5vgTFCWR/Prototype-%7C-BookStore?node-id=58-25580

Then implement with /primevue-component-build, honouring the repository conventions:
- All data comes from the existing JSON API. Server-side paging only, through the paged endpoint -
  never fetch every author and paginate on the client.
- Extract anything reusable (the table shell, the dialog shell styling, the row-actions menu, the
  labelled form field) into components/common/<category>/ so the Books page can reuse it later.
- Put fetch, dialog visibility and loading/error state in composables, not inside the components.
- Style through PrimeVue `pt` passthrough sections and design tokens. No raw hex, no Bootstrap,
  PrimeIcons only, Tailwind v4 postfix `!important` only where a PrimeVue theme class must be beaten.
- The Razor view must become a mount point that passes every URL as a data-* attribute; register the
  page in src/main.ts. Never hardcode an API or route URL inside the component.
- Surface API errors from the response body message in a PrimeVue Message, never an alert().
- Query the PrimeVue MCP server for component props and `pt` section names instead of guessing.

Do not delete the Razor Authors CRUD views yet - ask me first once the dialogs work.
Finish with: npx vue-tsc --noEmit, npm run build, dotnet build, and a /vault-write recording the
components created and the Figma node -> component mapping.
```

**Expected outcome:**

- A consolidated `/figma-discovery` report covering the five nodes, with the token mapping resolved once and reused across the page and the modals.
- An `AuthorsPage.vue` (page header + table + create dialog) and an `AuthorsTable.vue` (table wrapper + row-action state).
- Reusable pieces extracted under `components/common/` - the generic lazy paged table, the dialog shell `pt` helpers, the row-actions menu, the labelled form field - so the Books page migration is mostly assembly.
- Create / Edit / Details / Confirm-delete dialogs wired to `POST`, `PUT`, `GET` and `DELETE` on the authors API.
- `Views/Authors/Index.cshtml` reduced to `<div id="authors-app" data-api-url="/api/authors" ...>` plus a `mount(AuthorsPage, "#authors-app")` line in `src/main.ts`.
- A `/vault-write` run recording the new components and the node-to-component mapping.

> The agent must **ask** before deleting `Views/Authors/Create|Edit|Details|Delete.cshtml`. Keeping them until the dialogs are proven is the safe intermediate state.

### 4.3) Build and check against the design

```bash
cd Src/BookStore.Web/ClientApp
npx vue-tsc --noEmit
npm run build
cd ../../..
dotnet run --project Src/BookStore.Web
```

Open <http://localhost:5045/Authors> and exercise every path: paginate, open the row-actions menu, create an author, edit one, open details, and delete one (deleting an author who has books must show the domain error message inside the dialog, not an alert).

**Prompt (Agent mode) for a design diff:**

```text
Compare the Authors page and its four dialogs against the Figma nodes 14:8860, 44:23160, 12:8767,
44:23181 and 58:25580 using the Figma MCP server. List every real deviation in layout, spacing,
colors, radius and typography, separating genuine mistakes from deliberate design-token adaptations.
```

Fix any real deviation with `Frontend-Specialist`, then re-run `npm run build`.

---

## Step 5 - Verify everything

```bash
cd Src/BookStore.Web/ClientApp && npm run build && cd ../../..
dotnet build Src/BookStore.slnx
dotnet run --project Src/BookStore.Web
```

| Route / check | Expected behaviour |
|---|---|
| Command Palette -> **MCP: List Servers** | `primevue` and `figma-mcp` both listed and running |
| Chat -> **Configure Tools** | Figma and PrimeVue tool groups visible in Agent mode |
| `/` (Home) | Rendered by `Home.vue`, visually matching the Figma frame `39:20049`; no Bootstrap card markup |
| Page source of `/` | `~/dist/main.css` and `~/dist/main.js` loaded once from `_Layout.cshtml`, with a version query string |
| Browser DevTools -> Network | No request to any `bootstrap.*` file; PrimeIcons fonts load from `/dist/` |
| `/Authors` | Vue data table matching Figma `14:8860`; paging hits `/api/authors?page=&pageSize=` on every page change |
| `/Authors` row-actions button | Popup menu matching Figma `44:23160`, with Details / Edit / Delete entries |
| `/Authors` -> New Author | Create dialog matching Figma `12:8767`; saving `POST`s and the new row appears in the list |
| `/Authors` -> Edit | Edit dialog matching Figma `44:23181`, pre-filled; saving `PUT`s and updates the row |
| `/Authors` -> Delete | Confirm dialog matching Figma `58:25580`; deleting an author who has books shows the domain error message inside the dialog |
| `/Books`, `/Customers` | Still reachable and functional after the migration |
| `Src/BookStore.Web/wwwroot/lib/` | Contains jquery and jquery-validation only - **no `bootstrap/` folder** |
| `npx vue-tsc --noEmit` | No errors |
| `npm run build` | Emits `wwwroot/dist/main.js` and `main.css` |
| `dotnet build Src/BookStore.slnx` | Build succeeded |
| `dotnet test Src/BookStore.Tests` | All tests still pass - the migration is front-end only |

Extra checks from the terminal:

```bash
# no genuine Bootstrap reference left (btn / form-control in Razor are the @layer components shim)
grep -rn "bootstrap\|bi-" Src/BookStore.Web \
  --exclude-dir=node_modules --exclude-dir=dist --exclude-dir=bin --exclude-dir=obj

# the vault grew with the front-end notes
find Docs/Vault/BookStore -name "*.md" | sort
```

---

## Recap

| Step | Purpose | Copilot Mode | Outcome |
|---|---|---|---|
| **0** | Explore the front-end stack and the pre-built customization layer | Ask mode (8 prompts) | Team alignment on where the UI stands and who owns what - no changes |
| **1** | Give Copilot access to Figma and PrimeVue | Ask + Agent mode with `Frontend-Tooling-Specialist` | `.vscode/mcp.json` created, both servers started, Figma OAuth completed, both MCPs smoke-tested |
| **2** | Understand and activate the front-end agents, skills and instructions | Ask mode | Clear discovery -> build workflow, explicit agent boundaries, `applyTo` scoping confirmed |
| **3** | Migrate to MVC + Vue 3 + PrimeVue + Tailwind and rebuild Home from Figma | Agent mode with both front-end agents + `/figma-discovery` + `/primevue-component-build` | `ClientApp` scaffolded, Bootstrap removed, `/api/*` layer added, `Home.vue` matching the design |
| **4** | Migrate the Authors screen: paged table + four dialogs from five Figma nodes | Ask + Agent mode with `Frontend-Specialist` | `AuthorsPage.vue`, `AuthorsTable.vue` and reusable `common/` building blocks wired to `/api/authors` |
| **5** | Verify everything | Manual + terminal | MCP servers running, build clean, Home and Authors match Figma, tests green, vault updated |

---

## Wrap-up

Commit the result to `feature/Module4`:

```bash
git checkout -b feature/Module4
git add .
git commit -m "feature/Module4: Figma + PrimeVue MCP servers, front-end agents and skills, MVC + Vue 3 migration, Home from Figma"
```

Make sure `Src/BookStore.Web/wwwroot/dist/` is included in the commit and `Src/BookStore.Web/ClientApp/node_modules/` is not.

---

## References

**GitHub Copilot / VS Code**

- Add and manage MCP servers in VS Code - <https://code.visualstudio.com/docs/agent-customization/mcp-servers>
- MCP configuration reference - <https://code.visualstudio.com/docs/agents/reference/mcp-configuration>
- Extending Copilot Chat with MCP servers - <https://docs.github.com/en/copilot/how-tos/provide-context/use-mcp/extend-copilot-chat-with-mcp>
- About Model Context Protocol (MCP) - <https://docs.github.com/en/copilot/concepts/context/mcp>
- Enhancing Copilot agent mode with MCP - <https://docs.github.com/en/copilot/tutorials/enhance-agent-mode-with-mcp>
- Custom agents in VS Code - <https://code.visualstudio.com/docs/agent-customization/custom-agents>
- Use Agent Skills in VS Code - <https://code.visualstudio.com/docs/agent-customization/agent-skills>
- About agent skills (GitHub Docs) - <https://docs.github.com/en/copilot/concepts/agents/about-agent-skills>
- Use custom instructions in VS Code - <https://code.visualstudio.com/docs/copilot/customization/custom-instructions>
- Add repository custom instructions - <https://docs.github.com/en/copilot/how-tos/copilot-on-github/customize-copilot/add-custom-instructions/add-repository-instructions>
- Customize AI in Visual Studio Code overview - <https://code.visualstudio.com/docs/agent-customization/overview>

**Figma MCP**

- Figma MCP Server - <https://developers.figma.com/docs/figma-mcp-server/>
- Remote server installation - <https://developers.figma.com/docs/figma-mcp-server/remote-server-installation/>
- Tools and prompts - <https://developers.figma.com/docs/figma-mcp-server/tools-and-prompts/>
- Rate limits and access - <https://developers.figma.com/docs/figma-mcp-server/rate-limits-access/>
- Write to canvas - <https://developers.figma.com/docs/figma-mcp-server/write-to-canvas/>

**PrimeVue**

- PrimeVue - <https://primevue.dev/>
- Install PrimeVue with Vite - <https://primevue.dev/vite/>
- PrimeVue MCP Server - <https://primevue.dev/mcp/>
- PrimeVue Plugin (MCP + skills) - <https://primevue.dev/plugin>
- PrimeUI pricing - <https://primeui.dev/pricing>
- PrimeUI Community license - <https://primeui.dev/licenses/community>

**Tailwind CSS**

- Tailwind CSS docs - <https://tailwindcss.com/docs>
- Tailwind CSS v4 - <https://tailwindcss.com/blog/tailwindcss-v4>
- Framework guides - <https://tailwindcss.com/docs/installation/framework-guides>
- Responsive design - <https://tailwindcss.com/docs/responsive-design>
- Dark mode - <https://tailwindcss.com/docs/dark-mode>
