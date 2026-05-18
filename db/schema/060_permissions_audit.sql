\set ON_ERROR_STOP on

create table app.auth_user (
  auth_user_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  display_name text not null,
  email text,
  is_enabled boolean not null default true,
  authz_version integer not null default 1,
  auth_user_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  constraint fk_auth_user_tenant
    foreign key (tenant_id)
    references app.tenant (tenant_id),
  constraint uq_auth_user_tenant_user
    unique (tenant_id, auth_user_id),
  constraint ck_auth_user_authz_version
    check (authz_version >= 1)
);

create unique index uq_auth_user_email_active
  on app.auth_user (lower(email))
  where auth_user_status <> 'deleted' and email is not null;

comment on table app.auth_user is
  'Internal tenant-scoped user record. This is owned by the application, not by Keycloak or any external IdP.';

create table app.external_identity (
  external_identity_id uuid primary key default gen_random_uuid(),
  provider text not null,
  issuer text not null,
  external_subject text not null,
  auth_user_id uuid not null,
  external_identity_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  constraint fk_external_identity_auth_user
    foreign key (auth_user_id)
    references app.auth_user (auth_user_id)
);

create unique index uq_external_identity_provider_subject
  on app.external_identity (provider, external_subject);

create index ix_external_identity_auth_user
  on app.external_identity (auth_user_id);

comment on table app.external_identity is
  'Maps an external IdP identity to an internal auth_user. External identities are identifiers only, not business permissions.';

create table app.auth_group (
  group_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  group_name text not null,
  group_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  constraint fk_auth_group_tenant
    foreign key (tenant_id)
    references app.tenant (tenant_id),
  constraint uq_auth_group_tenant_group
    unique (tenant_id, group_id)
);

create index ix_auth_group_tenant
  on app.auth_group (tenant_id)
  where group_status <> 'deleted';

comment on table app.auth_group is
  'Tenant-scoped authorization group. Group names are display names only, not stable identifiers.';

create table app.auth_permission (
  permission_id uuid primary key default gen_random_uuid(),
  code text not null,
  permission_name text not null,
  permission_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create unique index uq_auth_permission_code_active
  on app.auth_permission (code)
  where permission_status <> 'deleted';

comment on table app.auth_permission is
  'Stable application-level permission identifiers such as network.read, network.write, and tenant.admin.';

create table app.user_group_membership (
  user_group_membership_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  auth_user_id uuid not null,
  group_id uuid not null,
  membership_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  constraint fk_user_group_membership_auth_user
    foreign key (tenant_id, auth_user_id)
    references app.auth_user (tenant_id, auth_user_id),
  constraint fk_user_group_membership_group
    foreign key (tenant_id, group_id)
    references app.auth_group (tenant_id, group_id)
);

create unique index uq_user_group_membership_active
  on app.user_group_membership (tenant_id, auth_user_id, group_id)
  where membership_status = 'active';

create index ix_user_group_membership_auth_user
  on app.user_group_membership (tenant_id, auth_user_id)
  where membership_status = 'active';

create index ix_user_group_membership_group
  on app.user_group_membership (tenant_id, group_id)
  where membership_status = 'active';

comment on table app.user_group_membership is
  'Assigns tenant-scoped users to tenant-scoped groups. Composite foreign keys prevent cross-tenant assignments.';

create table app.group_permission (
  group_permission_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  group_id uuid not null,
  permission_id uuid not null,
  group_permission_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  constraint fk_group_permission_group
    foreign key (tenant_id, group_id)
    references app.auth_group (tenant_id, group_id),
  constraint fk_group_permission_permission
    foreign key (permission_id)
    references app.auth_permission (permission_id)
);

create unique index uq_group_permission_active
  on app.group_permission (tenant_id, group_id, permission_id)
  where group_permission_status = 'active';

create index ix_group_permission_group
  on app.group_permission (tenant_id, group_id)
  where group_permission_status = 'active';

comment on table app.group_permission is
  'Assigns stable application permissions to tenant-scoped groups.';
