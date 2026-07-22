# Example Operations

> The tag values, folder names, and template filenames used below (e.g., `type/runbook`, `component/application`, `05_Operations`, `template-runbook.md`) are **illustrative only**. At runtime, valid values come from reading the live vault:
>
> - Tags → `Docs/Vault/BookStore/00_Index/Tags.md`
> - Folders → `Docs/Vault/BookStore/00_Index/Home.md` + `fd --type d --max-depth 2 . Docs/Vault/BookStore`
> - Templates → `fd --extension md . Docs/Vault/BookStore/07_Templates`
>
> Substitute the real values from those reads when applying the patterns below.

All examples assume the vault root is the current directory:

```bash
cd Docs/Vault/BookStore
```

## Example 1: Add a single new guide

User request: "Document how the error handling flow works end-to-end."

1. Search to confirm no existing note covers it:
    ```bash
    rg -l --glob "*.md" --ignore-case --fixed-strings "error handling" .
    ```
2. Pick folder: `04_Engineering/`.
3. Pick template: `07_Templates/Note-Template.md`.
4. Filename: `Error-Handling-Flow.md`.
5. Frontmatter:
    ```yaml
    ---
    type: guide
    created: 2026-05-28
    updated: 2026-05-28
    tags:
      - note
      - type/guide
      - area/engineering
      - component/application
      - component/clientapp
      - topic/error-handling
    ---
    ```
6. Cross-link from related notes in `04_Engineering/` (under `## Related`).

## Example 2: Update an existing note

User request: "Add info about the new Offering entity to the domain notes."

1. Locate the note:
    ```bash
    rg -l --glob "*.md" --ignore-case --fixed-strings "offering" .
    # → 03_Domain/Offering.md (if exists)
    ```
2. Read the existing file.
3. Set `updated: 2026-05-28` (preserve `created:`).
4. Append or revise sections without removing existing content.
5. Do not change `type` or `area/*` tags unless scope changed.

## Example 3: Split into multiple notes (atomic rule)

User request: "Document the distributor sync — both the architecture and the operational runbook."

Split into **two** notes:

| File | Folder | Type tag |
|------|--------|----------|
| `Distributor-Sync.md` | `02_Architecture/` | `type/overview` |
| `Run-Distributor-Sync.md` | `05_Operations/` | `type/runbook` |

Each note links to the other via `[[Distributor-Sync]]` / `[[Run-Distributor-Sync]]`.

## Example 4: Introduce a new tag

User request: "Add notes tagged with `topic/renewal`."

1. Confirm the tag does not exist:
    ```bash
    rg --glob "*.md" --ignore-case --fixed-strings "topic/renewal" .
    ```
2. Update `00_Index/Tags.md` — add `topic/renewal` under the `topic/*` section with a one-line description.
3. Then write the new note with the tag.
4. Report both files in the final summary.

## Example 5: Add a new component note

User request: "Document the new `component/notifications-worker`."

1. The component does not exist in the catalog → update `00_Index/Tags.md` first.
2. Create `02_Architecture/Notifications-Worker.md`:
    ```yaml
    ---
    type: overview
    created: 2026-05-11
    updated: 2026-05-11
    tags:
      - note
      - type/overview
      - area/architecture
      - component/notifications-worker
    ---
    ```
3. Cross-link from `02_Architecture/Architecture-Overview.md`.

## Example 6: Detect existing note before adding

```bash
rg -l --glob "*.md" --ignore-case --fixed-strings "teams notifications" .
# → 03_Domain/Teams-Notifications.md
```

One strong match → switch intent from `add` to `update`.

## Example 7: List notes in a target folder before naming a new one

```bash
fd --extension md . 05_Operations
```

Use the listing to match casing and naming style of siblings.

## Example 8: Date normalization

Always use `YYYY-MM-DD`:

```yaml
created: 2026-05-11
updated: 2026-05-11
```

When updating, change only `updated`.

## Example 9: Frontmatter for a policy note

```yaml
---
type: policy
created: 2026-05-11
updated: 2026-05-11
tags:
  - note
  - type/policy
  - area/engineering
  - area/configuration
  - topic/configuration
---
```

## Example 10: Cross-linking

Inside `## Related` at the end of every note:

```markdown
## Related

- [[Error-Handling-Flow]]
- [[Distributor-Sync]]
- [[Profile]]
```

Use the filename without `.md`.

## Example 11: Output summary returned by the skill

```text
Created:
- 04_Engineering/Error-Handling-Flow.md (type/guide, component/application, topic/error-handling)

Updated:
- 04_Engineering/Validation-Pattern.md (added cross-link)

New tags: none
TODOs: none
```
