---
name: figma-discovery
description: "Discovers and extracts design specs from Figma for the BookStore Vue/PrimeVue front-end. Use when: the user gives a Figma URL or node reference, asks to implement/match a design, or a new/changed component needs accurate spacing, color, or typography from Figma before coding. Parses Figma URLs, calls the Figma MCP tools, and maps raw values to the codebase's existing PrimeVue/Tailwind design-token conventions instead of raw hex/pixel values."
argument-hint: "A Figma URL or node reference, and what component/page it's for"
---

# Figma Discovery

Extracts an implementable spec from a Figma design BEFORE any component code is written or changed. Never guess at colors, spacing, or component structure from a screenshot alone - always pull the real node data through the Figma MCP server, then translate it into this codebase's existing PrimeVue + Tailwind conventions.

## Required setup

- `.vscode/mcp.json` must have the `figma-mcp` server entry (`{"type": "http", "url": "https://mcp.figma.com/mcp"}`). If missing or malformed, stop and say this is Frontend-Tooling-Specialist's responsibility to fix - do not add it yourself from this skill.
- Figma MCP tools used: `get_metadata`, `get_screenshot`, `get_design_context`, `get_variable_defs` (all prefixed `mcp_figma_mcp_ser_*` in this workspace).

## Procedure

1. **Parse the reference.** From a URL like `figma.com/design/:fileKey/:fileName?node-id=:nodeId`, extract `fileKey` and `nodeId` (convert `-` to `:` in the node id, e.g. `39-21141` -> `39:21141`). If the user gave only a node id with no URL, ask for the file link before proceeding - never assume a `fileKey` from a previous, unrelated task still applies.
2. **Confirm the target.** Call `get_metadata` and/or `get_screenshot` for the node first to confirm it's the right frame/component before pulling the full design context (cheaper, catches a wrong node id early).
3. **Pull the real spec.** Call `get_design_context` for the node. Treat its output as a REFERENCE to adapt, not final code - it does not know this codebase's existing components, composables, or token conventions.
4. **Resolve ambiguous raw values.** If a color/spacing value in the context isn't obviously a token, call `get_variable_defs` to resolve it to a named Figma variable, then map it using the token table below.
5. **Check for an existing match first.** Before treating anything as new, search `components/common/` for an analogous existing piece (a Dialog shell, a button style, a paginator, a form field) - if the app already has a pattern for this UI, prefer reusing/extending the EXISTING code convention over the raw Figma pixel value. Only introduce a genuinely new value when there's a deliberate visual difference from anything already built.
6. **Report the discovery** (this is the hand-off to component work, e.g. the `primevue-component-build` skill): node id/name, mapped color tokens, spacing/sizing values (flag which ones need Tailwind arbitrary values vs. default scale), typography, and which existing component(s) to mirror or extend.

## Token mapping table (extend as new values appear - never leave a raw hex/px in code when a row below applies)

| Figma value observed | Use this Tailwind/PrimeVue token |
|---|---|
| `#334155` (dark slate) | `surface-700` (`bg-surface-700`, `border-surface-700`, `text-surface-700`) |
| `#1e293b` (darker slate, hover) | `surface-800` |
| `#cbd5e1` / light gray borders | `border-surface-300` |
| `#ffffff` panel background | `bg-surface-0` |
| `#64748b` muted gray text | `text-muted-color` |
| primary body text (near-black slate) | `text-color` |
| Icon glyphs | PrimeIcons `pi pi-*` class - never inline SVG or Bootstrap Icons (`bi-*`) if an equivalent `pi-*` exists |

## Rules

- Never fabricate Figma content - always call the MCP tools; if a tool call fails, say so rather than guessing values.
- Never hardcode a raw hex color when a `surface-*`/`*-color` token is an exact or near match (see table above).
- Arbitrary Tailwind values (e.g. `rounded-[21px]`, `gap-1.75`, `w-[765px]`) are acceptable and already used throughout this codebase when the Figma value doesn't land on Tailwind's default scale - don't force a bad approximation just to avoid an arbitrary value.
- Delete any temporary screenshot/reference asset after implementation is verified against it.
- Always re-derive `fileKey`/`nodeId` from what the user provided in THIS task - never reuse one from memory of a prior task without confirming.
