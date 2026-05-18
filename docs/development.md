# Development

## Environment

- Ubuntu 24
- VS Code
- Docker

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

Build, test, and reset the database:

```bash
make build
make test
make db-reset
make keycloak-reset
make loki-reset
make grafana-reset
make reset-all
```

`make build` runs `docker compose build`.
`make test` validates the Docker Compose configuration. There is no Dockerized automated test suite configured yet.
`make db-reset` deletes only the application PostgreSQL Docker volume, restarts only the `postgres` service, then recreates the schema from `db/schema/`.
`make keycloak-reset` deletes only the Keycloak PostgreSQL Docker volume, then restarts `keycloak-postgres` and `keycloak`.
`make loki-reset` deletes only the Loki Docker volume, then restarts `loki` and `grafana-alloy`.
`make grafana-reset` deletes only the Grafana Docker volume, then restarts `grafana`.
`make reset-all` runs all four reset flows in sequence.

List the host-side physical mount locations of all database volumes:

```bash
project_name="${COMPOSE_PROJECT_NAME:-$(basename "$PWD")}"

for volume in $(docker compose config --volumes | grep 'postgres-data$'); do
  actual_volume="${project_name}_${volume}"
  echo "Volume: $actual_volume"
  docker volume inspect "$actual_volume" --format '  Mountpoint: {{ .Mountpoint }}'
done
```

## Useful Ports

- `5432` PostgreSQL
- `8081` Keycloak
  `make keycloak-reset` imports `n2-system` and `n2-users` from `infra/keycloak/`
  Development users come from those realm import JSON files or from manual creation in the Keycloak Admin UI
- `5100` `gateway`
- `5201` `catalog`
- `5202` `core-editor`
- `5203` `core-query`
- `5204` `system`
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
- `n2-users` users must have a `tenant_name` attribute.
- Only `gateway` validates Keycloak JWTs directly.
- Background services trust the request context headers forwarded by `gateway`.
- External LDAP or other IdP integration is planned for production later but is out of scope now.

## Documentation Rules

- The root `README.md` is the summary for the project.
- Detailed documentation belongs only under `docs/`.
- Documentation files must be Markdown files with the `.md` extension.
- Do not keep documentation in `db/`, `services/`, `infra/`, or other feature folders unless the user explicitly asks for an exception.
- When documentation is updated, keep links and references aligned with the current file structure.
