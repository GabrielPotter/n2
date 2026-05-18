#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_NAME="${COMPOSE_PROJECT_NAME:-$(basename "$ROOT_DIR")}"
KEYCLOAK_DATABASE_VOLUME="${PROJECT_NAME}_keycloak-postgres-data"
KEYCLOAK_DB_NAME="${KEYCLOAK_DB_NAME:-keycloak}"
KEYCLOAK_DB_USER="${KEYCLOAK_DB_USER:-keycloak}"

cd "$ROOT_DIR"

docker compose stop keycloak keycloak-postgres >/dev/null 2>&1 || true
docker compose rm -f -s keycloak keycloak-postgres >/dev/null 2>&1 || true
docker volume rm "$KEYCLOAK_DATABASE_VOLUME" >/dev/null 2>&1 || true
KEYCLOAK_IMPORT_REALM=true docker compose up -d keycloak-postgres keycloak

until docker compose exec -T keycloak-postgres pg_isready -U "$KEYCLOAK_DB_USER" -d "$KEYCLOAK_DB_NAME" >/dev/null 2>&1; do
  sleep 1
done
