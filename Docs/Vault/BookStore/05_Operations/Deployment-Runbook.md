---
type: runbook
created: 2026-07-22
updated: 2026-07-22
tags:
  - note
  - type/runbook
  - area/operations
---

# Deployment Runbook

Manual production deployment of BookStore.Web to Azure App Service via `deploy.sh`.

## Source

- `deploy.sh`
- `Src/BookStore.Web/BookStore.Web.csproj`

## Prerequisites

- Azure CLI (`az`) installed and authenticated.
- `zip` installed locally.
- Correct Azure subscription already selected (the script does not pass `--subscription`).

## Azure targets

| Resource | Name |
|----------|------|
| Resource group | `rg-book-store` |
| App Service plan | `asp-book-store` |
| Web App | `app-book-store` |
| Region | East US 2 |
| SKU | B1 |

## Deployment steps

1. Run the script from the repo root:

   ```bash
   ./deploy.sh
   ```

2. The script performs the following actions:

   - Publishes `Src/BookStore.Web/BookStore.Web.csproj` in `Release` configuration to `./publish/web`:

     ```bash
     dotnet publish "$WEB_PROJECT" -c Release -o "$PUB_DIR"
     ```

   - Packages the publish output into `./publish/web.zip`:

     ```bash
     ( cd "$PUB_DIR" && zip -qr "../../$ZIP_FILE" . )
     ```

   - Deploys the zip to the Azure Web App:

     ```bash
     az webapp deploy -g "$RG" -n "$APP" --type zip --src-path "$ZIP_FILE"
     ```

3. After deployment, the script prints:

   ```
   https://app-book-store.azurewebsites.net
   ```

## Notes and caveats

- No deployment slots are used; the script deploys directly to the production Web App.
- No monitoring or observability tooling (Application Insights, log streaming) is configured yet.
- There is no CI/CD pipeline; deployment is manual and runs from a local machine.

## Rollback

Rollback is not automated. To revert, re-run `deploy.sh` from the previous known-good commit.

## Related

- [[Operations-Overview]]
- [[Azure-App-Service-Docs]]
- [[Repository-Structure]]
