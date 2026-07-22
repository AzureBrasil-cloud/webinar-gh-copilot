# Write Reference

Commands and strategy for creating and updating vault notes. For vault structure, tag taxonomy, note format, and contribution rules, see `.github/instructions/vault.instructions.md`.

## Structure

The **authoritative source** for the folder layout is `00_Index/Home.md` (section `## Sections`) plus the actual folders on disk. **Always read it at runtime.** Never use a cached list from this skill.

### How to load the folder catalog

Folder → purpose mapping (authoritative description):

```bash
sed -n '/^## Sections/,/^## /p' Docs/Vault/BookStore/00_Index/Home.md
```

Actual folders that currently exist (top-level + one nested level, e.g., `05_Operations/Runbooks`):

```bash
fd --type d --max-depth 2 . Docs/Vault/BookStore
```

List notes in a specific folder before naming a new sibling:

```bash
fd --extension md . Docs/Vault/BookStore/05_Operations/Runbooks
```

### Structure of `Home.md`

- `## Sections` heading.
- Each line: `` - `NN_Name` — purpose ``.
- Folder prefix uses two-digit ordering (`00_`, `01_`, ..., `99_`) to keep the listing sorted.

## Templates

Templates live at `Docs/Vault/BookStore/07_Templates/`. The **authoritative source** is the vault itself — **always read templates at runtime**. Never use a cached copy from this skill.

List all available templates:

```bash
fd --extension md . Docs/Vault/BookStore/07_Templates
```

Read a specific template:

```bash
cat Docs/Vault/BookStore/07_Templates/<template-file>.md
```

### Template selection by `type/*`

Map the chosen `type/*` to a template filename inside `07_Templates/`. Discover the mapping by listing the folder and matching the suffix against the type value:

```bash
fd --extension md . Docs/Vault/BookStore/07_Templates
# Expect names like: template-<type>.md
```

If a matching `template-<type>.md` exists, use it. If not, fall back to the generic note template (the file whose name does **not** target a specific type — e.g., `template-note.md`).

### Replacing placeholders

Templates contain placeholders like `{{title}}`, ISO date stamps, and stub sections. After reading the template:

- Replace placeholder titles with the real title.
- Replace `created:` and `updated:` with today's date (ISO `YYYY-MM-DD`).
- Fill out the body sections from the user-provided content.
- Add `[[Wikilinks]]` to the `## Related` section pointing to relevant existing notes.

## Tag Taxonomy

Tags live in frontmatter as plain text. The **authoritative source** is `00_Index/Tags.md` — **always read it at runtime**. Never use a cached list from this skill.

### How to load all valid tags

Full file (recommended — also shows descriptions and grouping):

```bash
cat Docs/Vault/BookStore/00_Index/Tags.md
```

Extract every tag value as plain text (deduplicated, sorted):

```bash
rg -N --no-heading -o '`([a-z]+/[a-z0-9-]+)`' --replace '$1' \
  Docs/Vault/BookStore/00_Index/Tags.md | sort -u
```

Extract values under a single prefix (example: `area/*`):

```bash
awk '/^### area\/\*/{flag=1;next} /^### /{flag=0} flag' \
  Docs/Vault/BookStore/00_Index/Tags.md \
  | rg -o '`([a-z]+/[a-z0-9-]+)`' --replace '$1'
```

### Structure of `Tags.md`

- `## Tag catalog` section.
- `### <prefix>/*` headers (e.g., `### type/*`, `### area/*`, `### component/*`, `### env/*`, `### topic/*`).
- Each value under a header on its own line as `` - `prefix/value` `` (optionally followed by `— description`).
- The `### Base` section lists the base tag `note`.

## Detect Existing Notes

Before adding a new note, check the vault for overlap:

```bash
cd Docs/Vault/BookStore
rg -l --glob "*.md" --ignore-case --fixed-strings "<topic>" .
```

List notes in a folder:

```bash
fd --extension md . 05_Operations/Runbooks
```
