#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_NAME="${COMPOSE_PROJECT_NAME:-$(basename "$ROOT_DIR")}"
DATABASE_VOLUME="${PROJECT_NAME}_platform-postgres-data"
DB_NAME="${DB_NAME:-platformdb}"
DB_USER="${DB_USER:-platform}"

cd "$ROOT_DIR"

docker compose stop postgres >/dev/null 2>&1 || true
docker compose rm -f -s postgres >/dev/null 2>&1 || true
docker volume rm "$DATABASE_VOLUME" >/dev/null 2>&1 || true
docker compose up -d postgres

until docker compose exec -T postgres pg_isready -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1; do
  sleep 1
done

DB_NAME="$DB_NAME" DB_USER="$DB_USER" ./scripts/recreate-database.sh
