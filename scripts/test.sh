#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

mapfile -t test_projects < <(find services -maxdepth 2 \( -name "*Tests*.csproj" -o -name "*.Tests.csproj" \) | sort)

if [ "${#test_projects[@]}" -gt 0 ]; then
  dotnet test "${test_projects[@]}"
else
  echo "No backend test projects found."
fi

if [ -f "web/package.json" ]; then
  (
    cd web

    if [ ! -d "node_modules" ]; then
      npm install
    fi

    if npm run | grep -qE '^[[:space:]]+test$'; then
      npm run test
    else
      echo "No frontend test script found."
    fi

    if npm run | grep -qE '^[[:space:]]+lint$'; then
      npm run lint
    else
      echo "No frontend lint script found."
    fi

    if npm run | grep -qE '^[[:space:]]+typecheck$'; then
      npm run typecheck
    else
      echo "No frontend typecheck script found."
    fi
  )
fi
