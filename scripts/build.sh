#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

dotnet build services/Platform.sln

if [ -f "web/package.json" ]; then
  (
    cd web
    npm install
    npm run build
  )
fi
