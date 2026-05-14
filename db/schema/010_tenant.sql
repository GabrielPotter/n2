\set ON_ERROR_STOP on

create table app.tenant (
  tenant_id uuid primary key default gen_random_uuid(),
  code text not null,
  name text not null,
  status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create unique index uq_tenant_code_active
  on app.tenant (code)
  where status <> 'deleted';
