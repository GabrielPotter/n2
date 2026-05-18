#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_NAME="${COMPOSE_PROJECT_NAME:-$(basename "$ROOT_DIR")}"
LOKI_VOLUME="${PROJECT_NAME}_platform-loki-data"

cd "$ROOT_DIR"

docker compose stop grafana-alloy loki >/dev/null 2>&1 || true
docker compose rm -f -s grafana-alloy loki >/dev/null 2>&1 || true
docker volume rm "$LOKI_VOLUME" >/dev/null 2>&1 || true
docker compose up -d loki grafana-alloy
