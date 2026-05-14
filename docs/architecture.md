# Architecture

## Services

- `gateway`: public backend entrypoint and upstream proxy for internal services
- `catalog`: read-only catalog endpoints
- `core-editor`: write endpoints for business objects
- `core-query`: read endpoints for business objects
- `web`: React TypeScript frontend
- `postgres`: application database
- `keycloak`: development authentication and authorization server with imported `n2-system` and `n2-users` realms

## Request Flow

1. The frontend requests a Keycloak access token from the `n2-users` realm.
2. The frontend calls `gateway` with that token.
3. `gateway` validates the Keycloak JWT and forwards it to internal services.
4. Business microservices validate the same Keycloak JWT directly.
5. Tenant-scoped database queries use `tenant_id` from the token claims.
6. System status endpoints accept only `n2-system` tokens and do not use request-body tenant identifiers as a source of truth.

## Authorization Model

- Keycloak is the current authentication and authorization system.
- `n2-system` is reserved for system users.
- `n2-users` is used for normal application users.
- Business services validate Keycloak JWTs directly.
- Backend validation is configured with per-realm issuer, audience, and JWKS URL settings.
- Development users are created either from realm import JSON or manually in the Keycloak Admin UI.
- Predefined authorization groups are modeled as Keycloak realm roles.
- Realm roles are declared in the realm import JSON files.
- `n2-users` users must have a `tenant_id` attribute.
- The current application expects tokens to include `tenant_id`, `roles`, and `authz_version`.
- Role-based policies use the `roles` claim values `viewer`, `editor`, and `tenant-admin`.
- System policies use the `roles` claim values `platform-admin`, `support-admin`, and `security-admin`.
- Each access token must represent exactly one active tenant context.
- In the current project model, a user belongs to exactly one tenant.
- External LDAP or other IdP integration may be added for production later, but it is out of scope now.

## Development Stack

- PostgreSQL
- Keycloak PostgreSQL
- Keycloak
- `gateway`
- `catalog`
- `core-editor`
- `core-query`
- `web`
