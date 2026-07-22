#!/usr/bin/env bash
set -euo pipefail

# ============================
# Parameters
# ============================
RG="rg-book-store"
APP_SERVICE_PLAN="asp-book-store"
APP="app-book-store"

# ============================
# Build & Package
# ============================
WEB_PROJECT="./Src/BookStore.Web/BookStore.Web.csproj"
PUB_DIR="./publish/web"
ZIP_FILE="./publish/web.zip"

echo "==> Restoring, building, and publishing (Release)"
dotnet publish "$WEB_PROJECT" -c Release -o "$PUB_DIR"

echo "==> Packaging into $ZIP_FILE"
rm -f "$ZIP_FILE"
( cd "$PUB_DIR" && zip -qr "../../$ZIP_FILE" . )

# ============================
# Deploy
# ============================
echo "==> Deploying to Azure Web App: $APP (RG: $RG, Plan: $APP_SERVICE_PLAN)"
az webapp deploy -g "$RG" -n "$APP" --type zip --src-path "$ZIP_FILE"

URL="https://$APP.azurewebsites.net"
echo "==> Deploy completed!"
echo "==> Access: $URL"
