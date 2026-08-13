---
name: primevue-component-build
description: "Builds or updates Vue 3 SFC components for BookStore.Web using PrimeVue components, PrimeVue design tokens, Tailwind CSS v4 utilities, and PrimeIcons - and strips any leftover Bootstrap along the way. Use when: implementing a new component/dialog/table (often right after figma-discovery), restyling an existing component, or auditing a file for Bootstrap remnants."
argument-hint: "The component to create/update and, if available, the figma-discovery report for it"
---

# PrimeVue + Tailwind Component Build

Implements a Vue 3 component (or restyles an existing one) so it matches the codebase's established PrimeVue-tokens + Tailwind + PrimeIcons conventions, with zero Bootstrap. Assumes a `figma-discovery` report exists if the work is design-driven; if not, work from the closest existing analogous component instead of inventing new patterns.

## Procedure

1. **Place the file correctly.**
   - Page-level composition (`PageHeader` + table + create dialog) → `components/<Feature>Page.vue`.
   - Page-specific table wrapper → `components/<Feature>Table.vue`.
   - Anything reused across 2+ entities/pages → `components/common/<category>/`, where `<category>` is `dialog`, `form`, `pageheader`, `sidebar`, or `table`. If none fit, ask before inventing a new category folder.
2. **Reuse before creating.** Check these first and extend them instead of duplicating:
   - `composables/useEntityCrud.ts` (delete/edit/details row-action state + fetch) and `composables/useCreateEntity.ts` (create-dialog state + POST) for any stateful CRUD logic.
   - `styles/buttonStyles.ts` (`primaryButtonClass`/`secondaryButtonClass`/`dangerButtonClass`) for footer/action button classes.
   - `components/common/dialog/dialogStyles.ts` (`dialogShellPt`/`detailsDialogPt`) for any new Dialog's `pt` shell.
   - `components/common/form/FormField.vue` (editable label+input) / `DetailField.vue` (read-only label+value) for labeled fields.
   - `utils/format.ts` (`formatCurrency`, `formatDate`) for any pt-BR/BRL formatting.
3. **Apply PrimeVue design tokens for color** - never a raw hex value. Use `surface-0/300/700/800`, `text-color`, `text-muted-color`, `border-surface-300`, etc. (see the token table in the `figma-discovery` skill if translating from a Figma spec).
4. **Style through `pt` (passthrough) sections** for components that expose them (`Dialog`, `DataTable`, `Paginator`, `Button` state overrides) rather than a bare `class` - matches every existing dialog/table in this codebase.
5. **Apply Tailwind utilities for layout/spacing**, using arbitrary values (`w-[765px]`, `rounded-[21px]`, `gap-1.75`) when the Figma/design spec doesn't land on Tailwind's default scale - this is the established convention here, not a workaround to avoid.
6. **`!important` syntax:** always postfix (`bg-surface-700!`, `text-color!`), never prefix (`!bg-surface-700`). Only add `!` when overriding a PrimeVue component's OWN theme-colored state class (e.g. `.p-paginator-page-selected`, default Button primary color) - plain layout/spacing `pt` classes never need it.
7. **Icons:** use PrimeIcons (`<i class="pi pi-*">` or a component's `icon` prop) exclusively. Never add Bootstrap Icons (`bi`/`bi-*`) or a new inline SVG when an equivalent `pi-*` glyph exists.
8. **Bootstrap audit (always run before finishing):** grep the touched files and `Src/BookStore.Web/**` (excluding `node_modules`/`wwwroot/dist`) for `bootstrap|bi-|bi bi-|cdn.jsdelivr.net/npm/bootstrap`. Some Razor views legitimately use class names like `btn`, `btn-primary`, `form-control`, `alert-danger` - these are custom `@layer components` Tailwind classes defined in `src/style.css` with the same names as the old Bootstrap classes, NOT real Bootstrap; leave them alone. Only remove genuine Bootstrap CSS/JS `<link>`/`<script>` tags, the `wwwroot/lib/bootstrap/` folder, or `bi bi-*` icon usages if found.
9. **Verify.** From `Src/BookStore.Web/ClientApp`: run `npx vue-tsc --noEmit` then `npm run build`; fix all errors before finishing. If a `.cshtml` mount point or a Controller/Api DTO changed, also run `dotnet build Src/BookStore.slnx`.

## Rules

- Never hardcode a color that already has a `surface-*`/`*-color` token equivalent.
- Never duplicate a Tailwind class string, `pt` config object, or fetch/loading/error state machine that already exists in `styles/`, `common/dialog/dialogStyles.ts`, or `composables/` - import and reuse it, or extend it if the existing helper is close but not quite sufficient.
- Never introduce Bootstrap, jQuery-Bootstrap plugins, or `bi bi-*` icons - PrimeVue + Tailwind + PrimeIcons is the only sanctioned UI stack.
- Never introduce a second JS framework - Vue 3 is the only sanctioned front-end framework here.
- Never skip the `vue-tsc --noEmit` + `npm run build` verification step.
