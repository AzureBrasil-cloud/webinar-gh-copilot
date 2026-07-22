# Search Strategy

Commands and strategy for keyword search over `docs/vault/AtlasVault/`. For vault structure, tag taxonomy, and note format, see `.github/instructions/vault.instructions.md`. Run all commands from the repo root.

## Load reference data first

Decomposition needs the live tag catalog; result-gathering uses the navigation index. Always read them at runtime — never hard-code.

Tag values (deduplicated, sorted):

```bash
rg -N --no-heading -o '`([a-z]+/[a-z0-9-]+)`' --replace '$1' \
  docs/vault/AtlasVault/00_Index/Tags.md | sort -u
```

Values under one prefix (replace `area`):

```bash
awk '/^### area\/\*/{flag=1;next} /^### /{flag=0} flag' \
  docs/vault/AtlasVault/00_Index/Tags.md \
  | rg -o '`([a-z]+/[a-z0-9-]+)`' --replace '$1'
```

Navigation index (all notes + paths):

```bash
cat docs/vault/AtlasVault/00_Index/Navigation.md
```

List notes in a folder:

```bash
fd --extension md . docs/vault/AtlasVault/<folder>
```

## Query Decomposition

The skill receives a **complex natural-language query** and must break it into small search items.

### How to decompose

1. Extract the **key nouns and concepts** from the query as 1–2 word terms. Normalize to lowercase.
2. For each term, match it against the tag catalog loaded at runtime:
   - `component/*` tags match backend/frontend layers (e.g., `application` → `component/application`).
   - `entity/*` tags match domain entities (e.g., `contract` → `entity/contract`).
   - `topic/*` tags match cross-cutting concerns (e.g., `sync` → `topic/sync`).
   - `type/*` tags match note kinds (e.g., `runbook` → `type/runbook`).
   - `area/*` tags match functional areas (e.g., `security` → `area/security`).
   - `env/*` tags match environments (e.g., `production` → `env/prod`).
3. A term may have zero tags (pure free-text), one tag, or multiple tags.
4. Aim for 2–5 search items per query. Do not over-split.

### Decomposition output format

```
Search Item 1: text="distributor sync" tags=["topic/sync"]
Search Item 2: text="contract"         tags=["entity/contract"]
Search Item 3: text="company"          tags=["entity/company"]
```

## Search pipelines

### Chained pipeline (intersection)

The primary search uses chained `rg` pipes. Each pipe narrows the file list further. Prefer tags over free-text for the first filter (more selective):

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "<tag-or-text-1>" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "<tag-or-text-2>" \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "<text-3>" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

### Fallback — progressive filter removal

If the full chain returns zero results:
1. Remove the last (least-specific) pipe and re-run.
2. Repeat until at least one note is found or only one filter remains.
3. If a single filter still returns nothing, try the next search item alone.

### Broadening — individual item searches

If intersections return nothing, run each item independently and union the results:

```bash
rg -l --glob "*.md" --ignore-case --fixed-strings "<tag-or-text>" \
  docs/vault/AtlasVault \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

### Limiting results

```bash
rg -l --glob "*.md" --ignore-case --fixed-strings "<term>" \
  docs/vault/AtlasVault \
  | head -n 5 \
  | xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

### Smart recombination (when results are too few)

If the primary + fallback searches return fewer than 2 content notes (or only index files like `Tags.md`), generate **new search combinations**:

1. **Swap tag ↔ text** — replace a tag filter with its plain-text equivalent, or vice-versa:
   - Tag `entity/contract` → text `contract`
   - Text `sync` → tag `topic/sync`
2. **Use synonyms** — try alternative terms for the same concept:
   - `authentication` → `auth`, `credentials`, `login`
   - `deploy` → `deployment`, `release`, `publish`
3. **Try different tag prefixes** — a concept may live under a different prefix:
   - `topic/auth` → `area/security`
   - `entity/distributor` → `area/integration` + text `distributor`
4. **Re-pair items** — combine items that were not paired in the original chain:
   - Original: A → B → C (0 results). Try: A → C, B → C, A + C text only.

Run new pipelines with these recombined items and union with any previous results. Do at most **2 recombination rounds**.

Example — original `topic/sync` + `entity/contract` + `company` returned nothing:

```bash
# Round 1: swap tags for text
rg -l --glob "*.md" --ignore-case --fixed-strings "distributor sync" \
  docs/vault/AtlasVault \
| xargs rg -l --ignore-case --fixed-strings "contract" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'

# Round 2: try new pair (skip middle term)
rg -l --glob "*.md" --ignore-case --fixed-strings "sync" \
  docs/vault/AtlasVault \
| xargs rg -l --ignore-case --fixed-strings "company" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

### Wikilink relevance check

After collecting matched notes, extract and validate `[[wikilinks]]` to find additional relevant content:

1. **Extract links** from each matched note:
   ```bash
   rg -o --no-heading '\[\[([^\]]+)\]\]' --replace '$1' <matched-file>
   ```

2. **Resolve paths** — find the linked file in the vault:
   ```bash
   fd --extension md --full-path "<Link-Name>" docs/vault/AtlasVault
   ```

3. **Check relevance** — the linked note is relevant if it contains at least one of the original search terms (tags or text):
   ```bash
   rg -l --ignore-case --fixed-strings "<any-search-term>" <linked-file>
   ```

4. If relevant, **add to the result set** (deduplicate).

5. **One hop only** — do not follow links inside the linked notes.

6. **Exclude index files** — skip `Tags.md`, `Navigation.md`, `Home.md` unless they contain query-specific content beyond tag definitions.

Then **follow `[[Wikilinks]]`** in the matched notes to gather complete context.
