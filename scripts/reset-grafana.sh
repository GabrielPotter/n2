#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_NAME="${COMPOSE_PROJECT_NAME:-$(basename "$ROOT_DIR")}"
GRAFANA_VOLUME="${PROJECT_NAME}_platform-grafana-data"

cd "$ROOT_DIR"

docker compose stop grafana >/dev/null 2>&1 || true
docker compose rm -f -s grafana >/dev/null 2>&1 || true
docker volume rm "$GRAFANA_VOLUME" >/dev/null 2>&1 || true
docker compose up -d grafana
