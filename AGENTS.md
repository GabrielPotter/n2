# AGENTS

Project instructions for Codex.

## 1. Technology stack

- Use C# for backend code.
- Use TypeScript for frontend code.
- Do not generate JavaScript.
- Do not introduce extra frameworks unless explicitly requested.
- Prefer simple, readable code over unnecessary abstraction.

## 2. Project structure

Keep the project structure compact.

### Backend services

- Do not name services `*-api`.
- Keep service internals flat.
- Each backend service should prefer files such as:
  - `Program.cs`
  - `Api.cs`
  - `Contracts.cs`
  - `Services.cs`
  - `Database.cs`
  - `Settings.cs`

### Gateway service

- The `gateway` service should use `Clients.cs` instead of `Database.cs`.

### Shared backend code

- Shared backend code belongs in `services/common`.

### Modification rules

- When modifying code, prefer changing existing files instead of creating new folders.
- Preserve the compact structure unless a new structure is explicitly requested.

## 3. Documentation rules

- The project root `README.md` is the summary document for the project.
- Detailed documentation belongs only under `docs/`.
- Documentation files must always use the `.md` extension.
- Do not keep documentation files in `db/`, `services/`, `infra/`, or other folders unless the user explicitly requests an exception.
- When moving or consolidating documentation, keep all information but remove duplication.

## 4. Database source of truth

This project is under active development.

The database is never migrated during development.

The database structure is generated from scratch from the SQL files in `db/schema/`.

The `db/schema/` folder is the only source of truth for the database structure.

Use SQL files under `db/` as the database source of truth.

Allowed database folders:

- `db/init/`
- `db/schema/`
- `db/functions/`
- `db/views/`
- `db/seeds/`

Avoid `db/migrations/` unless real migration support is explicitly requested later.

## 5. Database generation workflow

When the database model changes:

1. Update the current SQL files directly.
2. Stop the local PostgreSQL container.
3. Remove the PostgreSQL Docker volume.
4. Recreate the database from the current SQL files.
5. Update all backend and frontend code to match the new schema.

During development, destructive database regeneration is allowed and expected.

Do not create:

- migration files
- migration chains
- migration runners
- upgrade scripts
- downgrade scripts
- schema diff logic
- compatibility patches
- backward-compatible database transition code
- migration-style `ALTER` scripts for old schemas

Prefer simple destructive reset scripts over migration scripts during development.

## 6. Development-stage compatibility rules

This project is under active development.

The current version is the only supported version.

There is no requirement for backward compatibility in:

- application code
- APIs
- database schema
- SQL files
- SQL functions
- triggers
- stored procedures
- configuration files
- seed data
- Docker setup
- frontend contracts
- backend contracts

Do not generate compatibility support unless explicitly requested.

Do not create:

- compatibility layers
- legacy adapters
- fallback code
- old DTO versions
- deprecated endpoints
- schema migration chains
- transitional code
- legacy columns
- deprecated compatibility views
- transitional tables
- aliases for old names
- compatibility wrappers
- support for old object formats

Do not generate code or files using names such as:

- `legacy`
- `v1`
- `v2`
- `deprecated`
- `old`
- `compat`
- `migration`

unless explicitly requested by the user.

Clean consistency is more important than preserving old behavior.

## 7. Refactoring rules

When changing a concept, rename and update it everywhere.

Do not keep duplicate old and new names.

Do not preserve outdated code only to support a previous version of the project.

If an incompatible change breaks old code, old data, old endpoints, or old database structures, that is acceptable.

The correct solution is to update all affected parts of the current project so they work consistently with the new design.

If something breaks because of an incompatible change, fix the broken current code directly.

Broken compatibility with old project versions is not a problem.

Inconsistency inside the current project is a bug.

## 8. Cross-layer consistency rules

When an API contract changes, update all callers and consumers immediately.

When a DTO changes, update all related frontend and backend code immediately.

When a table or column changes, update all affected parts immediately, including:

- SQL files
- C# contracts
- database access code
- frontend types
- frontend API clients
- seed data
- tests, if present

When a database change is needed, update the current SQL initialization and schema files, then update the reset scripts if needed.

## 9. Runtime delete behavior

Development schema regeneration is destructive.

Runtime business delete behavior is different.

Business objects are soft-deleted using `status = 'deleted'`.

Business records are normally not physically deleted during application use.

Do not confuse development database reset behavior with runtime business delete behavior.

## 10. Codex behavior rules

When modifying the project, always target the current design.

Do not ask whether backward compatibility should be preserved.

Backward compatibility must not be preserved unless explicitly requested.

If a change makes existing code incompatible, update the current code instead of adding compatibility code.

If a database change is needed, update the SQL source files directly.

If an API, DTO, table, column, SQL function, trigger, stored procedure, endpoint, or frontend contract changes, update every affected part of the current project immediately.

The goal is a clean, internally consistent current version of the project.

## 11. Reserved folders

The `/temp` folder is reserved for human developers.

Codex must not read files from `/temp`.

Codex must not use `/temp` content as project input, specification, context, or source material.

Codex must not rely on `/temp` when making implementation decisions.

## 12. Logging rules

All .NET microservices must use `ILogger<T>` for application logging.

Serilog is the configured logging provider.

All microservices must log to stdout/stderr.

Logs must be structured, single-line JSON.

Microservices must not write diagnostic logs directly to files, PostgreSQL, Loki, or Grafana.

Microservices must not use a direct Loki sink.

Grafana Alloy is responsible for collecting Docker stdout/stderr logs.

Grafana Alloy forwards logs to Grafana Loki.

Grafana is the UI for querying and viewing logs.

Loki labels must remain low-cardinality.

Allowed Loki labels:

- `service`
- `environment`
- `host`
- `runtime`
- `container`

High-cardinality values must remain JSON fields, not Loki labels.

Examples of high-cardinality fields:

- `correlationId`
- `requestId`
- `userId`
- `tenantId`
- `objectId`
- `categoryId`
- `traceId`
- `runId`

Use structured logging templates, not string concatenation and not manually serialized JSON strings.

Do not use `Console.WriteLine` for application logging after logging is initialized.

Audit logging, if needed, must be designed separately from diagnostic and service logging.

## 13. Authentication and authorization architecture

During development, use a containerized Keycloak instance for authentication and authorization.

Business microservices must validate Keycloak JWTs directly.

Keycloak may later connect to external LDAP or other IdP providers in production, but that integration is out of scope for now.

Do not implement LDAP integration now.

Do not implement external IdP federation now.

Do not add an intermediate token exchange service.

Keycloak-issued tokens used by the platform must carry the claims required by the current application.

A user belongs to exactly one tenant.

Development users are created from the realm import JSON files under `infra/keycloak/` or manually in the Keycloak Admin UI.

The application currently expects tokens to include at least:

- `iss`
- `sub`
- `tenant_id`
- `roles`
- `authz_version`
- `iat`
- `nbf`
- `exp`
- `jti`

Use short-lived access tokens.

Do not introduce backward compatibility layers.

This project is under active development. If auth contracts change, update the code consistently instead of preserving old contracts.

Do not add database migrations. Database structure is regenerated from SQL files under the schema folder.

Prefer simple, readable C# code.

Keep service internals flat.

Backend code must be C#.

Frontend code, if touched, must be TypeScript.

Do not generate JavaScript.

## 14. REST API rules:

- Use resource-oriented REST endpoints.
- Do not put verbs into URLs unless modeling a command collection is impossible.
- GET must be read-only.
- POST creates resources or starts business commands.
- PUT replaces a complete resource.
- PATCH partially updates a resource.
- DELETE performs soft-delete by setting status = deleted.
- Normal tenant APIs must not contain tenantId in the URL.
- Tenant context must come from the Keycloak JWT tenant_id claim.
- System/admin APIs may use /system/tenants/{tenantId}/... routes.
- All references must use IDs only.
- Names are display-only.
- Do not add code fields.
- object.object_kind is forbidden.
- Object kind must be resolved only through object.category_id -> object_category.object_kind.
- object.type_id must belong to the same category as object.category_id.