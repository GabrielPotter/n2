# Authentication And Authorization

## Overview

Keycloak is the authentication and authorization system for the current project version.
Only the gateway validates Keycloak JWTs directly.

This document covers:

- the general technical model
- the project-specific realms, roles, and token claims
- development startup and reset procedures
- token inspection in the current setup

## General Technical Model

### Authentication

Authentication is performed by Keycloak.
The client obtains an access token from a Keycloak realm and sends that token to the backend.
The gateway validates the JWT directly using configured issuer, audience, and JWKS settings.

### Authorization

Authorization is also driven by Keycloak-issued tokens in the current project version.
The gateway uses token claims to decide whether a request is allowed, then forwards trusted request context headers to downstream services.

The current authorization model uses:

- realm identity to distinguish `n2-users` from `n2-system`
- realm roles in the `roles` claim
- `tenant_name` in the `n2-users` token
- a gateway-resolved `tenant_id` for downstream tenant-scoped business access

### Realm Separation

The development setup uses two realms:

- `n2-users`: normal application users
- `n2-system`: system and operational users

In the current project model, a user belongs to exactly one tenant.
Tenant business endpoints accept only `n2-users` tokens.
System admin endpoints accept only `n2-system` tokens.

## Project-Specific Configuration

### Source Of Truth

The Keycloak development configuration is defined in realm import JSON files under `infra/keycloak/`.
The backend must not depend on manual Keycloak Admin UI state that is missing from those files.

Realm import files:

- `infra/keycloak/n2-users-realm.json`
- `infra/keycloak/n2-system-realm.json`

### Realms

#### `n2-users`

Purpose:

- normal application users
- tenant-scoped business access

Defined client:

- `n2-users-frontend`

Defined client scope:

- `n2-users-access`

Defined realm roles:

- `viewer`
- `editor`
- `tenant-admin`

Declarative user profile:

- users in this realm must have a `tenant_name` attribute

#### `n2-system`

Purpose:

- system and operational access
- administrative or operational endpoints

Defined client:

- `n2-system-admin`

Defined realm roles:

- `platform-admin`
- `support-admin`
- `security-admin`

This realm does not define tenant-specific user attributes.

### Roles Instead Of Keycloak Groups

In this first version, application authorization groups are modeled as Keycloak realm roles.
Keycloak groups are not used for application authorization.

This means:

- user permissions are represented as realm roles
- backend services read those values from the `roles` token claim
- the business meaning of the roles is interpreted by the application

### Development Users

Development users can be created in two ways:

- imported from the realm JSON files
- created manually through the Keycloak Admin UI

Seeded `n2-users` users:

- `admin@example.com`
- `editor@example.com`
- `viewer@example.com`

Seeded `n2-system` users:

- `platform-admin@example.com`
- `support-admin@example.com`
- `security-admin@example.com`

Default password for the seeded users:

```text
Password123!
```

## Token Model

### Validation Model

The gateway validates Keycloak JWTs directly.
Validation is configured per realm with:

- issuer
- audience
- JWKS URL

### `n2-users` Token Requirements

Tenant business endpoints require:

- a valid token from `n2-users`
- a non-empty `tenant_name` claim
- a required tenant realm role

The `n2-users` token model is defined by the imported client scope `n2-users-access`.

The access token is expected to contain at least:

- `iss`
- `sub`
- `tenant_name`
- `roles`
- `authz_version`
- `iat`
- `nbf`
- `exp`
- `jti`

### `n2-system` Token Requirements

System endpoints require:

- a valid token from `n2-system`
- a required system realm role

These endpoints do not use tenant context as their source of truth.

### Token Claim Mapping

The current `n2-users` realm import maps user attributes and realm roles into token claims.

Mapped user attributes:

- user ID -> `sub`
- `tenant_name` -> `tenant_name`
- `authz_version` -> `authz_version`

Mapped role claim:

- realm roles -> `roles`

### User ID And Session ID

The stable user identifier is the `sub` claim.
The `sid` claim, when present, identifies the login session and not the user.

That means:

- use `sub` as the user ID
- do not treat `sid` as a replacement for user identity

## Authorization Rules In The Current Project

### Tenant Roles

- `viewer`: read tenant data
- `editor`: read tenant data and create objects
- `tenant-admin`: all current tenant editor capabilities

### System Roles

- `platform-admin`: platform-level administrative access
- `support-admin`: operational or support access
- `security-admin`: security-oriented administrative access

### Tenant Source Of Truth

The `n2-users` token carries `tenant_name`.
The gateway resolves that `tenant_name` to the platform `tenant_id` through the `system` service and forwards the resolved `tenant_id` in trusted request headers.
Tenant business endpoints must not accept tenant identity from the request body as the authoritative value.

## Development Operation

### URLs

The Keycloak URLs are available only after the `keycloak` container is running and listening on local port `8081`.

- `n2-system`: `http://localhost:8081/realms/n2-system`
- `n2-users`: `http://localhost:8081/realms/n2-users`
- Admin UI: `http://localhost:8081/admin/`

Admin credentials:

- username: `admin`
- password: `admin`

### Start Full Stack

```bash
docker compose up -d --build
```

This starts the stack in detached mode and returns the shell prompt immediately.
It does not import realms into an existing or empty Keycloak database by itself.

If you want to watch startup logs:

```bash
docker compose logs -f
```

### Start Only Keycloak

Use this when you want to start the dedicated Keycloak PostgreSQL and the Keycloak container without recreating existing Keycloak state:

```bash
docker compose up -d keycloak-postgres keycloak
```

Check status:

```bash
docker compose ps keycloak-postgres keycloak
```

Follow startup logs:

```bash
docker compose logs -f keycloak
```

### Full Keycloak Cleanup And Restart

Use this when you want to delete all persisted Keycloak development data and recreate the state only from the current realm import JSON files.

```bash
make keycloak-reset
docker compose logs -f keycloak
```

This deletes only the dedicated Keycloak development database volume.
It does not delete the main application PostgreSQL volume.
This flow is also the place where realm import is explicitly enabled.

### Stop Keycloak

```bash
docker compose stop keycloak keycloak-postgres
```

### Remove Keycloak Containers Without Deleting Data

```bash
docker compose rm -sf keycloak keycloak-postgres
```

This removes the containers but keeps the `n2_keycloak-postgres-data` volume.

## Troubleshooting

### Duplicate Import Errors

If Keycloak fails during startup with messages such as:

- `Duplicate resource error`
- `duplicate key value violates unique constraint`

then the persisted Keycloak database state is out of sync with the current realm import JSON files.

In this project, that typically means:

- old Keycloak state still exists in `n2_keycloak-postgres-data`
- the realm import JSON files contain fixed IDs for seeded users
- the import tries to insert records that already exist in the persisted Keycloak database

The correct development fix is a full Keycloak cleanup and restart:

```bash
make keycloak-reset
docker compose logs -f keycloak
```

### Successful Startup Indicators

A healthy startup usually contains log lines similar to:

```text
Realm 'n2-system' imported
Realm 'n2-users' imported
Import finished successfully
Listening on: http://0.0.0.0:8080
```

## Token Inspection

### Get A User Token From `n2-users`

Example for the seeded editor user:

```bash
curl -sS -X POST 'http://localhost:8081/realms/n2-users/protocol/openid-connect/token' -H 'Content-Type: application/x-www-form-urlencoded' --data-urlencode 'grant_type=password' --data-urlencode 'client_id=n2-users-frontend' --data-urlencode 'username=editor@example.com' --data-urlencode 'password=Password123!'
```

### Get A System Token From `n2-system`

Example for the seeded support admin user:

```bash
curl -sS -X POST 'http://localhost:8081/realms/n2-system/protocol/openid-connect/token' -H 'Content-Type: application/x-www-form-urlencoded' --data-urlencode 'grant_type=password' --data-urlencode 'client_id=n2-system-admin' --data-urlencode 'username=support-admin@example.com' --data-urlencode 'password=Password123!'
```

### Extract The Access Token

If `jq` is available:

```bash
TOKEN=$(curl -sS -X POST 'http://localhost:8081/realms/n2-users/protocol/openid-connect/token' -H 'Content-Type: application/x-www-form-urlencoded' --data-urlencode 'grant_type=password' --data-urlencode 'client_id=n2-users-frontend' --data-urlencode 'username=editor@example.com' --data-urlencode 'password=Password123!' | jq -r '.access_token')
```

### Inspect Token Contents

To print the JWT payload as JSON:

```bash
printf '%s' "$TOKEN" | cut -d '.' -f2 | tr '_-' '/+' | base64 -d 2>/dev/null | jq
```

If the payload length is not padded correctly for `base64 -d`, use this more robust variant:

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

If you want a single copy-paste command without storing `TOKEN` first:

```bash
curl -sS -X POST 'http://localhost:8081/realms/n2-users/protocol/openid-connect/token' -H 'Content-Type: application/x-www-form-urlencoded' --data-urlencode 'grant_type=password' --data-urlencode 'client_id=n2-users-frontend' --data-urlencode 'username=editor@example.com' --data-urlencode 'password=Password123!' | jq -r '.access_token' | TOKEN="$(cat)" python - <<'PY'
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

For an `n2-users` access token, verify at least:

- `iss` points to the `n2-users` realm
- `sub` is present
- `tenant_id` is present and non-empty
- `roles` contains the expected tenant role
- `authz_version` is present

### What To Check In A System Token

For an `n2-system` access token, verify at least:

- `iss` points to the `n2-system` realm
- `sub` is present
- `roles` contains the expected system role
- `tenant_id` is not required

## Out Of Scope

- LDAP integration
- external IdP federation
- an intermediate token exchange service
- any dependency on Keycloak Admin UI state that is not represented in the realm import JSON files
