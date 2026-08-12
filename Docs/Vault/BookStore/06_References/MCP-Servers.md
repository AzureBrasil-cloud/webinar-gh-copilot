---
type: reference
created: 2026-08-12
updated: 2026-08-12
tags:
  - note
  - type/reference
  - area/project
---

# MCP Servers

VS Code project-level [Model Context Protocol](https://modelcontextprotocol.io/) server configuration for consuming external tool integrations from Copilot Chat/agents.

## What was done

Created `.vscode/mcp.json` (previously empty) with a `servers` map registering two MCP servers, so any agent session in this workspace can call their tools without per-user setup.

## Configuration (`.vscode/mcp.json`)

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

## Servers

- **primevue** — stdio server launched on demand via `npx -y @primevue/mcp`. Exposes PrimeVue component/library documentation to the agent. Source: [PrimeVue MCP docs](https://primevue.dev/mcp/).
- **figma-mcp** — remote HTTP server at `https://mcp.figma.com/mcp`. Exposes Figma design context (design-to-code, screenshots, design system search) to the agent.

## How it was added

Both entries were added by editing `servers` in `.vscode/mcp.json` directly, following each vendor's documented VS Code setup snippet. No other project files were changed.

## Scope note

This configuration is project-scoped (`.vscode/mcp.json`). To reuse either server across other projects, add the same `servers` entries to the VS Code user MCP configuration instead.

## Related

- [[Repository-Structure]]
