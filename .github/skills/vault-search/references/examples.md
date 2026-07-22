# Example Queries

> The tag values and folder names used below (e.g., `type/policy`, `component/application`, `05_Operations`) are **illustrative only**. At runtime, the actual valid tags come from `docs/vault/AtlasVault/00_Index/Tags.md` and the actual navigation index comes from `docs/vault/AtlasVault/00_Index/Navigation.md`. Substitute the real values from those reads when running the commands below.

All examples assume the vault root as the base path:

```bash
docs/vault/AtlasVault
```

---

## Example 1: Distributor sync and Contract matching

**Query:** `How does distributor sync match Contracts to Companies?`

**Decomposition:**

| # | Text | Tags |
|---|------|------|
| 1 | distributor sync | `topic/sync` |
| 2 | contract | `entity/contract` |
| 3 | company | `entity/company` |

**Primary search (full chain):**

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "topic/sync" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "entity/contract" \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "company" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

**Fallback 1 — drop `company`:**

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "topic/sync" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "entity/contract" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

**Fallback 2 — tag only:**

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "topic/sync" \
  docs/vault/AtlasVault \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

**Smart recombination (if results are only index files):**

Round 1 — swap tags for free-text:
```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "distributor sync" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "contract" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

Round 2 — try new pairing:
```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "sync" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "company" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

**Wikilink follow** — if any matched note contains links:
```bash
# Extract links from a matched note
rg -o --no-heading '\[\[([^\]]+)\]\]' --replace '$1' docs/vault/AtlasVault/03_Domain/Distributor-Sync.md

# Resolve link path
fd --extension md --full-path "Contract" docs/vault/AtlasVault

# Check relevance against original query terms
rg -l --ignore-case --fixed-strings "sync" docs/vault/AtlasVault/03_Domain/Contract.md
```

If the linked note is relevant, add it to the result set.

---

## Example 2: Opportunity lifecycle for renewals

**Query:** `Show me how the renewal flow works for Opportunities`

**Decomposition:**

| # | Text | Tags |
|---|------|------|
| 1 | opportunity | `entity/opportunity` |
| 2 | renewal | _(none — free text)_ |
| 3 | flow | _(none — free text)_ |

**Primary search:**

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "entity/opportunity" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "renewal" \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "flow" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

---

## Example 3: Error handling and validation patterns

**Query:** `Find the error handling and validation patterns`

**Decomposition:**

| # | Text | Tags |
|---|------|------|
| 1 | error handling | `topic/error-handling` |
| 2 | validation | `topic/validation` |

**Primary search:**

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "topic/error-handling" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "topic/validation" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

---

## Example 4: Authentication configuration

**Query:** `How is authentication configured in Atlas?`

**Decomposition:**

| # | Text | Tags |
|---|------|------|
| 1 | authentication | `topic/auth` |
| 2 | configuration | _(none — free text)_ |

**Primary search:**

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "topic/auth" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "configuration" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

**Fallback — tag only:**

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "topic/auth" \
  docs/vault/AtlasVault \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

**Smart recombination (if only Tags.md matched):**

Round 1 — swap `topic/auth` for synonym text:
```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "OIDC" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "authentication" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

Round 2 — try `area/security` instead of `topic/auth`:
```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "area/security" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "Entra" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

---

## Example 5: Application layer service patterns

**Query:** `How are services structured in the Application layer?`

**Decomposition:**

| # | Text | Tags |
|---|------|------|
| 1 | application | `component/application` |
| 2 | service | _(none — free text)_ |

**Primary search:**

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "component/application" \
  docs/vault/AtlasVault \
| xargs rg -l \
  --ignore-case \
  --fixed-strings "service" \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```

---

## Example 6: Soft deletion pattern

**Query:** `How does soft deletion work?`

**Decomposition:**

| # | Text | Tags |
|---|------|------|
| 1 | soft deletion | `topic/soft-deletion` |

**Primary search:**

```bash
rg -l \
  --glob "*.md" \
  --ignore-case \
  --fixed-strings "topic/soft-deletion" \
  docs/vault/AtlasVault \
| xargs -I {} sh -c 'echo "\n===== {} ====="; cat "{}"'
```
