# n2

Compact local platform stack with .NET microservices, a React TypeScript frontend, PostgreSQL, and a development Keycloak instance in Docker.

## Services

- `gateway` public backend entrypoint
- `catalog` catalog read endpoints
- `core-editor` object write endpoints
- `core-query` object read endpoints
- `system` system realm group endpoints
- `common` shared backend library
- `web` React frontend

## Quick Start

Start the full stack:

```bash
docker compose up -d --build
```

Follow logs when needed:

```bash
docker compose logs -f
```

Start PostgreSQL only, then run services locally:

```bash
make dev
dotnet run --project services/gateway/Gateway.csproj
dotnet run --project services/catalog/Catalog.csproj
dotnet run --project services/core-editor/CoreEditor.csproj
dotnet run --project services/core-query/CoreQuery.csproj
dotnet run --project services/system/System.csproj
cd web && npm run dev
```

Common commands:

```bash
make build
make test
make db-reset
```

## Documentation

Detailed project documentation lives under [`docs/`](docs/README.md).

- [Documentation index](docs/README.md)
- [Architecture](docs/architecture.md)
- [Development](docs/development.md)
- [Authentication and authorization](docs/authentication-and-authorization.md)
- [Logging](docs/logging.md)
- [Database](docs/database.md)

## Authentication Summary

- Keycloak is the authentication and authorization system.
- Development Keycloak imports two realms from `infra/keycloak/`:
  - `n2-system`
  - `n2-users`
- Development users can be created from realm import JSON or manually in the Keycloak Admin UI.
- Predefined authorization groups are modeled as Keycloak realm roles, not Keycloak groups.
- Realm roles are declared in the realm import JSON files.
- `n2-users` users must have a `tenant_id` attribute.
- External LDAP or other IdP integration may be added for production later, but it is out of scope now.

## Core Rules

- The root `README.md` is the project summary only.
- All detailed documentation lives under `docs/`.
- Documentation files must use the `.md` format.
- `db/schema/` is the only database schema source of truth.
- Business microservices validate Keycloak JWTs directly.
- Development Keycloak imports `infra/keycloak/n2-system-realm.json` and `infra/keycloak/n2-users-realm.json`.
