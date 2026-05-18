\set ON_ERROR_STOP on

create table app.tenant (
  tenant_id uuid primary key default gen_random_uuid(),
  tenant_name text not null,
  tenant_status app.record_status not null default 'active',
  properties jsonb not null default '{}'::jsonb,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  deleted_at timestamptz null,
  constraint uq_tenant_name
    unique (tenant_name)
);
