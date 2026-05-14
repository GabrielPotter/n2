# Database

## Setup

Local PostgreSQL runs in Docker through `docker-compose.yml` using the official `postgres` image.

- database: `platformdb`
- user: `platform`
- password: `platform`

## Schemas

- `app`
- `catalog`
- `core`

## Source Of Truth

- The database is never migrated during development.
- The database is regenerated from scratch from the SQL files under `db/schema/`.
- `db/schema/` is the only source of truth for the current database structure.
- Update the current SQL files directly instead of creating migrations or compatibility scripts.
- Backward compatibility is not required during development.

Allowed database folders:

- `db/init/`
- `db/schema/`
- `db/functions/`
- `db/views/`
- `db/seeds/`

## Reset Workflow

The expected workflow for schema changes is a destructive rebuild of the current development database.

Use:

```bash
make db-reset
./db/scripts/recreate-database.sh
```

Supported environment variables for `./db/scripts/recreate-database.sh`:

- `DB_NAME` default: `platformdb`
- `DB_USER` default: `platform`

The script uses `docker compose exec postgres psql` internally.

Equivalent `psql` example:

```bash
docker compose exec -T postgres psql -U platform -d platformdb < db/schema/010_tenant.sql
```

## Main Schema Files

- `000_drop_and_create_schema.sql`: resets the `app` schema
- `001_extensions_types.sql`: extensions and shared enum types
- `010_tenant.sql`: tenant base table
- `020_category_type_object.sql`: categories, types, and flat object storage
- `030_relations.sql`: object-to-object relation storage
- `040_rules.sql`: tenant-specific category and type rule tables
- `050_closure_virtual.sql`: node closure and virtual edge summary tables
- `060_permissions_audit.sql`: users, groups, permissions, and audit tables
- `070_functions_triggers.sql`: foundational PL/pgSQL functions and triggers
- `900_seed_minimal_test_data.sql`: minimal local development seed data

## Design Principles

- `graph_object` is flat.
- `object_kind` exists only in `object_category`.
- names are display-only.
- all references are ID-based.
- tenant isolation uses `tenant_id` in composite foreign keys.
- runtime delete is soft delete.
- development schema recreation is destructive.

## Current Limitations

- JSON Schema validation is not fully enforced.
- Full restore is not implemented.
- Full virtual summary rebuild after subtree moves is not implemented.
- Category and type rule validation is partial.
- Layer closure is not implemented.
- RLS is not enabled.
