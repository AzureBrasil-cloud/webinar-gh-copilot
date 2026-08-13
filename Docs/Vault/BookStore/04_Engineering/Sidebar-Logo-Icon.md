---
type: guide
created: 2026-08-13
updated: 2026-08-13
tags:
  - note
  - type/guide
  - area/engineering
  - component/web
---

# Sidebar Logo Icon

The sidebar's top-left logo box uses a PrimeIcons glyph instead of an image asset.

## Context

The logo box in `Src/BookStore.Web/ClientApp/src/components/common/sidebar/Sidebar.vue` previously rendered
`<img :src="'/images/home/books.gif'">`. It was replaced with a PrimeIcons `pi-book` glyph so the logo is a
scalable icon consistent with the rest of the icon usage in the app (nav items, `Home.vue` buttons).

## Details

- Markup now (`Sidebar.vue`):
  ```html
  <div class="flex size-10 shrink-0 items-center justify-center rounded-lg border border-surface-300 bg-surface-0">
    <i class="pi pi-book text-2xl text-surface-700" />
  </div>
  ```
- Sizing/color: `text-2xl` (~24px, matching the removed `size-6` image) and `text-surface-700`, matching the
  color of the adjacent "BookStore" brand text in the same block.
- Figma node `28:16622` (file `2zyEr3S75NxHIJ5vgTFCWR`) was the requested reference, but the `figma-mcp`
  server was not exposed as a callable tool in the session that made this change, so the exact node spec
  could not be pulled live. The container (`size-10`, `rounded-lg`, `border-surface-300`, `bg-surface-0`) was
  left unchanged since it was already an established convention; only the inner `<img>` was swapped for the
  icon. Re-verify against Figma node `28:16622` next time the Figma MCP tools are available.
- `wwwroot/images/home/books.gif` is no longer referenced anywhere in `ClientApp/src`; the file itself was
  left in place (not deleted) pending explicit confirmation it's safe to remove.

## Related

- [[Engineering-Overview]]
