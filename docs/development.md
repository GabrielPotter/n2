# Development

## Overview

This document contains project operation and setup information:

- local environment assumptions
- startup and reset commands
- ports, URLs, and credentials
- database, Keycloak, and logging maintenance workflows
- token inspection examples

Architecture descriptions live in the dedicated `*-architecture.md` documents under `docs/`.

## Environment

| Area | Current setup |
| --- | --- |
| Operating system | Ubuntu 24 |
| Editor | VS Code |
| Container runtime | Docker |
| Main orchestration | `docker compose` |

## Common Commands

| Command | Purpose |
| --- | --- |
| `docker compose up -d --build` | start the full stack |
| `docker compose logs -f` | follow logs for the full stack |
| `make build` | run `docker compose build` |
| `make test` | validate the Docker Compose configuration |
| `make db-reset` | recreate the application PostgreSQL state from `db/schema/` |
| `make keycloak-reset` | recreate the Keycloak PostgreSQL state from current realm imports |
| `make loki-reset` | recreate Loki persisted log storage |
| `make grafana-reset` | recreate Grafana persisted state |
| `make reset-all` | run all reset flows in sequence |

## Service Access

### Useful Ports

| Port | Service | Purpose |
| --- | --- | --- |
| `8080` | `web` | frontend |
| `5100` | `gateway` | public backend entrypoint |
| `5201` | `catalog` | internal read service |
| `5202` | `core-editor` | internal write service |
| `5203` | `core-query` | internal read service |
| `5204` | `system` | system service |
| `5432` | `postgres` | application database |
| `8081` | `keycloak` | authentication and admin UI |
| `3000` | `grafana` | log UI |
| `3100` | `loki` | log store API |

### Useful URLs

| URL | Purpose |
| --- | --- |
| `http://localhost:8080` | frontend |
| `http://localhost:5100` | gateway |
| `http://localhost:8081/admin/` | Keycloak admin UI |
| `http://localhost:8081/realms/n2-users` | `n2-users` realm |
| `http://localhost:8081/realms/n2-system` | `n2-system` realm |
| `http://localhost:3000` | Grafana |

## Credentials And Seeded Users

### Keycloak Admin

| Field | Value |
| --- | --- |
| username | `admin` |
| password | `admin` |

### Seeded User Password

| Field | Value |
| --- | --- |
| default seeded user password | `Password123!` |

### Seeded `n2-users` Accounts

| Username | Role |
| --- | --- |
| `admin@example.com` | `tenant-admin` |
| `editor@example.com` | `editor` |
| `viewer@example.com` | `viewer` |

### Seeded `n2-system` Accounts

| Username | Role |
| --- | --- |
| `platform-admin@example.com` | `platform-admin` |
| `support-admin@example.com` | `support-admin` |
| `security-admin@example.com` | `security-admin` |

### Grafana

| Field | Value |
| --- | --- |
| URL | `http://localhost:3000` |
| username | `admin` |
| password | `admin` |

## Database Operation

### Database Source Model

| Rule | Current behavior |
| --- | --- |
| Source of truth | `db/schema/` |
| Migration model | no migration chain |
| Schema change workflow | destructive rebuild of current development database |

### Reset Workflow

| Command | Behavior |
| --- | --- |
| `make db-reset` | stops or recreates only the application PostgreSQL state and rebuilds schema from `db/schema/` |
| `./scripts/recreate-database.sh` | recreates the schema using the current SQL files |

### Recreate Script Environment Variables

| Variable | Default |
| --- | --- |
| `DB_NAME` | `platformdb` |
| `DB_USER` | `platform` |

### Example Manual SQL Apply

```bash
docker compose exec -T postgres psql -U platform -d platformdb < db/schema/010_tenant.sql
```

### Volume Mountpoint Inspection

```bash
project_name="${COMPOSE_PROJECT_NAME:-$(basename "$PWD")}"

for volume in $(docker compose config --volumes | grep 'postgres-data$'); do
  actual_volume="${project_name}_${volume}"
  echo "Volume: $actual_volume"
  docker volume inspect "$actual_volume" --format '  Mountpoint: {{ .Mountpoint }}'
done
```

## Keycloak Operation

### Start Only Keycloak

```bash
docker compose up -d keycloak-postgres keycloak
docker compose ps keycloak-postgres keycloak
docker compose logs -f keycloak
```

### Reset Keycloak State

```bash
make keycloak-reset
docker compose logs -f keycloak
```

`make keycloak-reset` deletes only the dedicated Keycloak PostgreSQL volume and recreates state from:

- `infra/keycloak/n2-system-realm.json`
- `infra/keycloak/n2-users-realm.json`

### Stop Keycloak

```bash
docker compose stop keycloak keycloak-postgres
```

### Remove Keycloak Containers Without Deleting Data

```bash
docker compose rm -sf keycloak keycloak-postgres
```

### Keycloak Troubleshooting

If Keycloak startup fails with duplicate import errors, the persisted Keycloak database is out of sync with the current realm import files.
The expected development fix is:

```bash
make keycloak-reset
docker compose logs -f keycloak
```

## Logging Stack Operation

### Start Only Logging Stack

```bash
docker compose up -d loki grafana grafana-alloy
docker compose ps loki grafana grafana-alloy
```

### Stop Logging Stack

```bash
docker compose stop loki grafana grafana-alloy
```

### Remove Logging Containers Without Deleting Data

```bash
docker compose rm -sf loki grafana grafana-alloy
```

### Reset Logging State

```bash
make loki-reset
make grafana-reset
docker compose ps loki grafana grafana-alloy
```

`make loki-reset` resets only Loki log storage.
`make grafana-reset` resets only Grafana persisted state.

## Token Inspection

### Get A User Token From `n2-users`

```bash
curl -sS -X POST 'http://localhost:8081/realms/n2-users/protocol/openid-connect/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'grant_type=password' \
  --data-urlencode 'client_id=n2-users-frontend' \
  --data-urlencode 'username=editor@example.com' \
  --data-urlencode 'password=Password123!'
```

### Get A System Token From `n2-system`

```bash
curl -sS -X POST 'http://localhost:8081/realms/n2-system/protocol/openid-connect/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'grant_type=password' \
  --data-urlencode 'client_id=n2-system-admin' \
  --data-urlencode 'username=support-admin@example.com' \
  --data-urlencode 'password=Password123!'
```

### Extract The Access Token

```bash
TOKEN=$(curl -sS -X POST 'http://localhost:8081/realms/n2-users/protocol/openid-connect/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'grant_type=password' \
  --data-urlencode 'client_id=n2-users-frontend' \
  --data-urlencode 'username=editor@example.com' \
  --data-urlencode 'password=Password123!' | jq -r '.access_token')
```

### Inspect The JWT Payload

```bash
printf '%s' "$TOKEN" | cut -d '.' -f2 | tr '_-' '/+' | base64 -d 2>/dev/null | jq
```

More robust variant:

```bash
TOKEN="$TOKEN" python - <<'PY'
import base64
import json
import os

token = os.environ["TOKEN"]
payload = token.split(".")[1]
payload += "=" * (-len(payload) % 4)
decoded = base64.urlsafe_b64decode(payload.encode("ascii"))
print(json.dumps(json.loads(decoded), indent=2))
PY
```

### What To Check In A Tenant Token

| Claim | Expectation |
| --- | --- |
| `iss` | points to the `n2-users` realm |
| `sub` | present |
| `tenant_name` | present and non-empty |
| `roles` | contains the expected tenant role |
| `authz_version` | present |

### What To Check In A System Token

| Claim | Expectation |
| --- | --- |
| `iss` | points to the `n2-system` realm |
| `sub` | present |
| `roles` | contains the expected system role |

## Observability Use

### Example Loki Queries

```logql
{service="gateway"}
{service="keycloak"}
{environment="Development"}
{service="core-editor"} | json
{environment="Development"} | json | correlationId="some-correlation-id"
```

## Project Conventions

| Area | Rule |
| --- | --- |
| Backend language | C# |
| Frontend language | TypeScript |
| Service naming | do not use `*-api` names |
| Service structure | keep internals flat |
| Shared backend code | `services/common` |
| Gateway data-access file | `Clients.cs` instead of `Database.cs` |
| Database evolution | edit current SQL files directly |
| JWT validation | only `gateway` validates JWTs directly |

## Documentation Rules

| Rule | Current behavior |
| --- | --- |
| Project summary document | root `README.md` |
| Detailed docs location | `docs/` only |
| Documentation format | `.md` |
| Architecture docs naming | architecture-focused docs use the `-architecture.md` suffix |
| Setup and operations | belong in `development.md` |
