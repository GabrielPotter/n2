#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

docker compose up -d postgres

cat <<'EOF'
PostgreSQL is starting with Docker Compose.

Run backend services in separate terminals:
  dotnet run --project services/gateway/Gateway.csproj
  dotnet run --project services/catalog/Catalog.csproj
  dotnet run --project services/core-editor/CoreEditor.csproj
  dotnet run --project services/core-query/CoreQuery.csproj

Run frontend:
  cd web
  npm install
  npm run dev
EOF
