#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
DB_NAME="${DB_NAME:-platformdb}"
DB_USER="${DB_USER:-platform}"

cd "$ROOT_DIR"

docker compose exec -T postgres psql \
  -U "$DB_USER" \
  -d "$DB_NAME" \
  -v ON_ERROR_STOP=1 \
  -c "drop schema if exists app cascade; create schema app;"

while IFS= read -r file; do
  echo "Applying $(basename "$file")"
  docker compose exec -T postgres psql \
    -U "$DB_USER" \
    -d "$DB_NAME" \
    -v ON_ERROR_STOP=1 \
    < "$file"
done < <(find "$ROOT_DIR/db/schema" -maxdepth 1 -name "*.sql" | sort)
