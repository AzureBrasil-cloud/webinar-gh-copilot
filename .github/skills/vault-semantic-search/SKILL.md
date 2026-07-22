---
name: vault-semantic-search
description: "Semantic search over the BookStore Obsidian vault using the indexed codebase. Use this skill to find docs by meaning rather than exact keywords, understand how components relate, explore business rules, and discover relevant notes without knowing their exact location or tags."
argument-hint: "Describe what you want to understand or find in the vault"
---

# Vault Semantic Search

Semantic search over `Docs/Vault/BookStore/` using the VS Code Copilot workspace index (`#codebase`). For vault structure, folder layout, tag taxonomy, note format, and contribution rules, see `.github/instructions/vault.instructions.md`.

## When to Use

- Query is conceptual or phrased in natural language
- Exact keywords are unknown or ambiguous
- `vault-search` returned zero or low-quality results
- Exploring related topics without knowing which note to check

## Required Tools

- `#codebase` — VS Code Copilot semantic index; finds content by meaning, not keywords
- `read_file` — loads the full content of a note when a snippet is not enough

## Procedure

1. **Formulate a rich semantic query.** Expand the user's question with synonyms, related concepts, and domain vocabulary drawn from the BookStore project. Aim for 1–3 sentences.
2. **Run `#codebase` with the enriched query.** In agent mode, include `#codebase` as a context item.
3. **Evaluate results.** Vault notes (under `Docs/Vault/BookStore/`) are the primary authority; source code snippets provide implementation detail.
4. **Load full notes** with `read_file` when a snippet is truncated or references another note.
5. **Synthesize the answer.** Cite notes with markdown links (e.g., `[Note Title](Docs/Vault/BookStore/02_Architecture/Note.md)`).
6. **Fallback to `vault-search`** if `#codebase` returns no relevant results.
