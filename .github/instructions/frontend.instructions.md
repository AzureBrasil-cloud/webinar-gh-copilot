---
applyTo: "Src/BookStore.Web/ClientApp/**,Src/BookStore.Web/wwwroot/dist/**"
---

# Front-end instructions (Vue 3 + PrimeVue + Tailwind v4)

Reference guide for humans and coding agents working inside `Src/BookStore.Web/ClientApp/`.
For how the built bundle is served by ASP.NET Core MVC (Razor mount points, API endpoints, run/publish),
see `web.instructions.md`. For the vault rules, see `vault.instructions.md`.

This file documents the **reference implementation**. The same architecture is reused when migrating other
classic MVC + Razor + Bootstrap projects - see "Porting to a new project" at the end.

## Mandatory vault workflow

Every agent touching this folder MUST:

1. Read `.github/instructions/vault.instructions.md`.
2. Run `/vault-search` for existing context BEFORE writing code.
3. Do the work.
4. Run `/vault-write` afterwards to record what changed (new tags go in `00_Index/Tags.md` first).

Never hand-edit vault notes; always go through `/vault-write`.

## Stack

| Concern | Choice | Notes |
|---|---|---|
| Framework | Vue 3.5 SFC, `<script setup lang="ts">` | No Vue Router, no Pinia, no SPA shell. |
| Build | Vite 6 + `@vitejs/plugin-vue` | Output goes to `../wwwroot/dist`. |
| UI kit | PrimeVue 4.3 + `@primeuix/themes` (Aura preset) | Registered in `src/main.ts`, `darkModeSelector: false` (light-only app). |
| Icons | `primeicons` 7 (`pi pi-*`) | Imported in `src/style.css`. Never Bootstrap Icons (`bi-*`), never a raw SVG when a `pi-*` glyph exists. |
| CSS | Tailwind CSS v4 via `@tailwindcss/vite` + `tailwindcss-primeui` | CSS-first config: **there is no `tailwind.config.js`**. |
| Types | TypeScript 5.7, `strict: true`, `vue-tsc` | `tsconfig.json` is `noEmit` - Vite does the transpiling. |
| Bootstrap / jQuery UI | **Removed** | jQuery remains ONLY for ASP.NET unobtrusive validation on the Razor Customers forms. |

## Folder layout (do not deviate)

```
Src/BookStore.Web/ClientApp/
  vite.config.ts             # base "/dist/", outDir ../wwwroot/dist, fixed main.js / main.css names
  tsconfig.json              # strict, noEmit, includes src/**/*.ts + src/**/*.vue
  package.json               # dev | build | preview scripts
  src/
    main.ts                  # single entry: mounts one Vue app per Razor mount point
    style.css                # Tailwind + primeui + primeicons imports, @source, @layer components
    types.ts                 # shared DTO interfaces mirroring Models/Api/*Dto.cs
    shims-vue.d.ts
    components/
      <Feature>Page.vue      # page composition: PageHeader + <Feature>Table + Create dialog
      <Feature>Table.vue     # DataTableCommon wrapper + row-action wiring
      Home.vue, PoweredByBadge.vue
      common/
        dialog/              # <Verb><Entity>Dialog.vue + dialogStyles.ts
        form/                # FormField.vue (label + input slot), DetailField.vue (label + value)
        pageheader/          # PageHeader.vue (title, description, #actions slot)
        sidebar/             # Sidebar.vue (global nav, mounted from _Layout.cshtml)
        table/               # DataTableCommon.vue (generic lazy paged DataTable)
    composables/             # usePagedFetch.ts, useEntityCrud.ts, useCreateEntity.ts
    styles/                  # buttonStyles.ts - shared Tailwind class-string constants
    utils/                   # format.ts - formatCurrency (BRL), formatDate (pt-BR)
```

Placement rules:

- Directly under `components/` **only** for a page (`<Feature>Page.vue`) or a page-specific `<Feature>Table.vue`.
- Anything reused by 2+ entities/pages goes to `components/common/<category>/` (`dialog`, `form`, `pageheader`, `sidebar`, `table`). Ask before inventing a new category.
- Stateful logic (fetch, dialog visibility, loading/error) belongs in `composables/`, not copy-pasted into components.
- Reusable Tailwind class strings go to `styles/`; `pt` objects shared by dialogs go to `common/dialog/dialogStyles.ts`.
- Pure helpers (formatting, parsing) go to `utils/`.

## Setup

Prerequisites: Node.js 20+ (repo verified on Node 24) and npm 10+; .NET 10 SDK for the host app.

```bash
cd Src/BookStore.Web/ClientApp
npm install          # first clone only
npm run build        # emits ../wwwroot/dist/main.js + main.css (+ primeicons fonts)
```

MCP servers used by the front-end agents live in `.vscode/mcp.json` (`primevue` stdio + `figma-mcp` http).
Creating/repairing that file is Frontend-Tooling-Specialist's job.

## Build / run loop

- `npm run build` is **not** wired into `dotnet build` or the `.csproj`. Re-run it after **every** ClientApp change, otherwise the app keeps serving the previous `wwwroot/dist` bundle.
- `npm run dev` (Vite dev server) is only useful for isolated component work. The MVC app never loads from the Vite dev server - it always loads `~/dist/main.js`. There is no HMR through Razor.
- `wwwroot/dist/` **is committed to git on purpose**: `dotnet publish` / `deploy.sh` never run npm, so the deployed app would have no front-end without it. Commit the rebuilt bundle together with the source change. (`ClientApp/node_modules/` and `ClientApp/dist/` stay ignored.)
- Type-check with `npx vue-tsc --noEmit` before finishing. Both this and `npm run build` must pass.

## How a component reaches the page

`src/main.ts` is the only entry point. It mounts one app per Razor mount point, and each `data-*` attribute
on the mount `<div>` becomes a prop:

```ts
function mount(component: Component, selector: string) {
  const el = document.querySelector<HTMLElement>(selector);
  if (!el) return;                       // page without this mount point: no-op, never throw
  const props = { ...el.dataset };       // data-api-url -> apiUrl
  createApp(component, props)
    .use(PrimeVue, { theme: { preset: Aura, options: { darkModeSelector: false } } })
    .mount(el);
}

mount(Home, "#home-app");
mount(Sidebar, "#sidebar-app");
mount(BooksPage, "#books-app");
// ...
```

Consequences to respect:

- **Every `data-*` prop arrives as a `string`.** Declare those props as `string` and parse (`Number(...)`, `=== "true"`) inside the component. Never declare a mount-level prop as `number`/`boolean`.
- URLs are never hardcoded in Vue: Razor passes them (`data-api-url="/api/books"`, `data-create-url="@Url.Action(...)"`).
- Adding a new page = add the component, add one `mount(...)` line, add the mount `<div>` in the Razor view.
- Each mount is an independent app instance; there is no shared client-side store. Cross-page state travels through the URL or the API, not through globals.

## Data access

All data comes from the JSON API in `Src/BookStore.Web/Controllers/Api/*ApiController.cs` and is typed by
interfaces in `types.ts` mirroring `Models/Api/*Dto.cs`.

| Composable | Use for | Contract |
|---|---|---|
| `usePagedFetch<T>(apiUrl)` | Every list/table | `GET {apiUrl}?page=&pageSize=` -> `PagedResult<T>`; exposes `items`, `totalRecords`, `loading`, `error`, `load(page, pageSize)`. |
| `useEntityCrud<TEntity, TEditPayload>({apiUrl, entityLabel, reload})` | Row actions | Delete (`DELETE {apiUrl}/{id}`), Edit (`PUT {apiUrl}/{id}`), Details dialog state, per-action `loading`/`error`. |
| `useCreateEntity<TPayload>({apiUrl, entityLabel, onCreated})` | "New X" dialog | `POST {apiUrl}` + dialog visibility, `loading`, `error`. |

Rules:

- **Server-side paging only.** `DataTable` uses `lazy` + `paginator`; never fetch all rows and page client-side.
- API errors surface as `{ "message": "..." }` (a `DomainException` mapped to `400`); the composables already read `body.message` and fall back to the status code. Show it with a PrimeVue `Message severity="error"`, never `alert()`.
- Before writing a new fetch/loading/error block, extend one of the three composables instead.
- Extend `types.ts` when a DTO gains a field - keep it in sync with the C# DTO.

## Styling rules

1. **Design tokens, not hex.** Use the `tailwindcss-primeui` token utilities: `surface-0`, `surface-50`, `surface-100`, `surface-300`, `surface-500`, `surface-700`, `surface-800`, `text-color`, `text-muted-color`, `border-surface-300`, `bg-primary`, `text-primary-contrast`. Never a raw hex or an arbitrary gray when a token matches.
2. **Style PrimeVue through `pt` (passthrough) sections** (`root`, `header`, `content`, `footer`, `list`, `itemLink`, `page`, ...) rather than a bare `class`, matching `DataTableCommon.vue` and `dialogStyles.ts`.
3. **`!important` is postfix in Tailwind v4**: `bg-surface-700!`, `text-color!`. (A few prefix usages survive in `Home.vue`; do not copy them.) Only add `!` when overriding a PrimeVue theme-colored state class (`.p-paginator-page-selected`, default Button primary) - PrimeVue injects its stylesheet after Tailwind, so equal-specificity utilities lose silently. Plain layout/spacing `pt` classes never need it.
4. **Arbitrary values are fine and expected** when the design does not land on Tailwind's scale: `rounded-[21px]`, `gap-1.75`, `px-[11.5px]`, `w-69.5`.
5. **Reuse the shared class strings**: `styles/buttonStyles.ts` (`primaryButtonClass`, `secondaryButtonClass`, `dangerButtonClass`) and `common/dialog/dialogStyles.ts` (`dialogShellPt(width, contentClass?)`, `detailsDialogPt(width)`). If a new combination appears in 2+ places, extract it there.
6. **Tailwind v4 config lives in `src/style.css`**: `@import "tailwindcss"; @import "tailwindcss-primeui"; @import "primeicons/primeicons.css";`. Content scanning only covers files under `ClientApp/`, so `@source "../../Views";` is what makes utility classes used in `.cshtml` files (e.g. `h-screen` on `<body>`) get generated. Keep it.
7. **The `@layer components` block in `style.css` is not Bootstrap.** `.btn`, `.btn-primary`, `.btn-secondary`, `.btn-danger`, `.form-label`, `.form-control`, `.form-select`, `.alert-danger` are Tailwind-backed re-implementations kept so the remaining Razor forms (Customers Create/Edit/Delete/Details) keep working. Do not rename them and do not treat them as Bootstrap remnants.

### Shared building blocks

| Piece | What it gives you |
|---|---|
| `common/table/DataTableCommon.vue` | Lazy paged `DataTable` + Figma-matched paginator/menu `pt`, `columns: DataTableColumn[]` (`field`, `header`, `primary`), per-column `#col-<field>` slots, and a row-actions `Menu`. Actions either navigate (`detailsUrl`/`editUrl`/`deleteUrl`) or emit (`confirmDetails`/`confirmEdit`/`confirmDelete` -> `details`/`edit`/`delete`). |
| `common/dialog/*` | `CreateBookDialog`, `EditBookDialog`, `DetailsBookDialog`, the Author equivalents, and `ConfirmDeleteDialog`. Copy the closest one's `pt` wiring instead of hand-rolling spacing. |
| `common/form/FormField.vue` / `DetailField.vue` | Editable label + input slot / read-only label + value. |
| `common/pageheader/PageHeader.vue` | `title`, optional `description`, `#actions` slot (used for the "New X" button). |
| `common/sidebar/Sidebar.vue` | Global nav, mounted from `_Layout.cshtml`, highlights the active controller via `data-active`. |
| `utils/format.ts` | `formatCurrency` (pt-BR / BRL) and `formatDate` (pt-BR). Use these, never inline `Intl` calls. |

## Working from Figma

1. Run `/figma-discovery` FIRST for any task that references a Figma URL/node or must match a design. It parses the URL, calls the Figma MCP tools, and maps raw values onto the token conventions above.
2. Then run `/primevue-component-build` (or follow the equivalent manual steps) to implement.
3. Query the `primevue` MCP server for component APIs/examples instead of guessing prop or `pt` section names.
4. Exported image/icon assets go to `Src/BookStore.Web/wwwroot/images/<page>/...` and are referenced by absolute URL (`/images/home/books.jpg`) - not imported through Vite, so no hashing surprises.
5. Delete temporary screenshots/reference assets once the implementation is verified.

## Definition of done

- [ ] `/vault-search` ran before the work, `/vault-write` after it.
- [ ] Existing composable/style/`pt` helper reused instead of duplicated.
- [ ] No raw hex where a token exists; no Bootstrap; no `bi-*` icons; no second JS/CSS framework.
- [ ] `npx vue-tsc --noEmit` clean.
- [ ] `npm run build` clean, and the rebuilt `wwwroot/dist` committed with the change.
- [ ] `dotnet build Src/BookStore.slnx` run if a `.cshtml` mount point or an API DTO changed.
- [ ] Page opened in the running app and checked against the Figma frame.

## Never

- Never turn the app into an SPA (no Vue Router, no client-side routing) - navigation stays MVC.
- Never introduce a second UI framework (Bootstrap, Bulma) or a second JS framework (React, Angular).
- Never paginate client-side.
- Never hardcode API/route URLs inside Vue components - receive them as `data-*` props.
- Never edit `wwwroot/dist/*` by hand; it is generated.
- Never change `vite.config.ts`, `package.json` deps, or `.vscode/mcp.json` from a component task - that is Frontend-Tooling-Specialist scope.

## Porting to a new project

When migrating another classic MVC + Razor + Bootstrap app, reproduce this exact shape:

1. `<WebProject>/ClientApp/` scaffolded with Vite + Vue 3 + TS; `build.outDir` -> `../wwwroot/dist`, `base: "/dist/"`, fixed `main.js` / `main.css` output names.
2. `npm i vue primevue @primeuix/themes primeicons` + `npm i -D vite @vitejs/plugin-vue tailwindcss @tailwindcss/vite tailwindcss-primeui typescript vue-tsc`.
3. `src/style.css` with the three `@import`s, `@source "../../Views";`, and a `@layer components` shim for whatever Bootstrap classes the surviving Razor forms still use.
4. `src/main.ts` with the `mount()` + `dataset`-as-props helper and the PrimeVue/Aura registration.
5. Remove `wwwroot/lib/bootstrap/` and every Bootstrap `<link>`/`<script>`/`bi-*` icon; keep jQuery + jquery-validation only if Razor forms still use unobtrusive validation.
6. Add `Controllers/Api/*ApiController.cs` + `Models/Api/*Dto.cs` returning `PagedResultDto<T>`, reusing the existing services (never re-implement paging or business rules).
7. Convert Index views to mount `<div>`s with `data-*` URLs; migrate CRUD pages to dialogs feature by feature.
8. Copy `composables/`, `common/table`, `common/dialog`, `common/form`, `common/pageheader`, `styles/`, `utils/` as the starting kit, then adapt to that project's design system.
9. Create `.vscode/mcp.json` with the `primevue` and `figma-mcp` servers.
10. Seed the new project's vault with the equivalent front-end notes via `/vault-write`.
