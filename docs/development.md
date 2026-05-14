# Development

## Environment

- Ubuntu 24
- VS Code
- Docker
- .NET SDK
- Node.js

## Common Commands

Start the full stack:

```bash
docker compose up -d --build
```

This starts the stack in detached mode and returns the shell prompt immediately.

Follow logs when needed:

```bash
docker compose logs -f
```

Start PostgreSQL and supporting local infrastructure for running services outside Docker:

```bash
make dev
```

Run services locally:

```bash
dotnet run --project services/gateway/Gateway.csproj
dotnet run --project services/catalog/Catalog.csproj
dotnet run --project services/core-editor/CoreEditor.csproj
dotnet run --project services/core-query/CoreQuery.csproj
cd web && npm run dev
```

Build, test, and reset the database:

```bash
make build
make test
make db-reset
```

## Useful Ports

- `5432` PostgreSQL
- `8081` Keycloak
  Keycloak imports `n2-system` and `n2-users` from `infra/keycloak/`
  Development users come from those realm import JSON files or from manual creation in the Keycloak Admin UI
- `5100` `gateway`
- `5201` `catalog`
- `5202` `core-editor`
- `5203` `core-query`
- `5173` Vite dev server
- `8080` Docker web container
- `3000` Grafana
- `3100` Loki

## Observability

The logging stack is part of `docker compose up -d --build`.

Application logs flow like this:

`microservice stdout/stderr -> grafana-alloy -> loki -> grafana`

Rules:

- .NET services log structured single-line JSON to stdout/stderr
- services do not send logs directly to Loki
- Grafana Alloy discovers Docker containers and forwards labeled stdout/stderr logs to Loki

Grafana:

- URL: `http://localhost:3000`
- user: `admin`
- password: `admin`

Example Loki queries:

```logql
{service="gateway"}
{environment="Development"}
{service="core-editor"} | json
{environment="Development"} | json | correlationId="some-correlation-id"
```

## Project Conventions

- Keep the project structure compact.
- Keep service internals flat.
- Prefer editing existing files instead of adding new layers or folders.
- Prefer files such as `Program.cs`, `Api.cs`, `Contracts.cs`, `Services.cs`, `Database.cs`, and `Settings.cs`.
- `gateway` uses `Clients.cs` instead of `Database.cs`.
- Do not name services `*-api`.
- Shared backend code belongs in `services/common`.
- Use C# for backend code.
- Use TypeScript for frontend code.
- Do not generate JavaScript unless explicitly requested.
- Prefer `db/schema/` over migration history during development.
- Keycloak is the current authentication and authorization system.
- Predefined authorization groups are modeled as Keycloak realm roles defined in the realm import JSON files.
- `n2-users` users must have a `tenant_id` attribute.
- External LDAP or other IdP integration is planned for production later but is out of scope now.

## Documentation Rules

- The root `README.md` is the summary for the project.
- Detailed documentation belongs only under `docs/`.
- Documentation files must be Markdown files with the `.md` extension.
- Do not keep documentation in `db/`, `services/`, `infra/`, or other feature folders unless the user explicitly asks for an exception.
- When documentation is updated, keep links and references aligned with the current file structure.
