\set ON_ERROR_STOP on

create table app.object_category (
  category_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  object_kind app.object_kind not null,
  category_name text not null,
  json_schema jsonb not null default '{}'::jsonb,
  category_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  created_by uuid,
  updated_at timestamptz not null default now(),
  updated_by uuid,
  deleted_at timestamptz,
  deleted_by uuid,
  constraint fk_object_category_tenant
    foreign key (tenant_id)
    references app.tenant (tenant_id),
  constraint uq_object_category_tenant_category
    unique (tenant_id, category_id),
  constraint uq_object_category_tenant_category_kind
    unique (tenant_id, category_id, object_kind)
);

comment on table app.object_category is
  'Defines tenant-owned object categories. object_kind lives here so kind is modeled by category, not duplicated on graph_object.';

comment on column app.object_category.object_kind is
  'The object kind is defined only at category level. graph_object must derive kind through category_id and must not store object_kind directly.';

create table app.object_type (
  type_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  category_id uuid not null,
  type_name text not null,
  json_schema jsonb not null default '{}'::jsonb,
  type_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  created_by uuid,
  updated_at timestamptz not null default now(),
  updated_by uuid,
  deleted_at timestamptz,
  deleted_by uuid,
  constraint fk_object_type_category
    foreign key (tenant_id, category_id)
    references app.object_category (tenant_id, category_id),
  constraint uq_object_type_tenant_type
    unique (tenant_id, type_id),
  constraint uq_object_type_tenant_category_type
    unique (tenant_id, category_id, type_id)
);

comment on table app.object_type is
  'Defines concrete types inside a category. object_kind is inherited from the referenced object_category.';

create table app.graph_object (
  object_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  object_name text not null,
  category_id uuid not null,
  type_id uuid not null,
  properties jsonb not null default '{}'::jsonb,
  object_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  created_by uuid,
  updated_at timestamptz not null default now(),
  updated_by uuid,
  deleted_at timestamptz,
  deleted_by uuid,
  delete_operation_id uuid,
  constraint fk_graph_object_tenant
    foreign key (tenant_id)
    references app.tenant (tenant_id),
  constraint fk_graph_object_category
    foreign key (tenant_id, category_id)
    references app.object_category (tenant_id, category_id),
  constraint fk_graph_object_type
    foreign key (tenant_id, category_id, type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint uq_graph_object_tenant_object
    unique (tenant_id, object_id),
  constraint uq_graph_object_tenant_object_category
    unique (tenant_id, object_id, category_id),
  constraint uq_graph_object_tenant_object_category_type
    unique (tenant_id, object_id, category_id, type_id)
);

comment on table app.graph_object is
  'Stores tenant-owned graph objects. object_kind is intentionally absent and must be derived through graph_object.category_id -> object_category.object_kind.';

create index ix_object_category_tenant_object_kind
  on app.object_category (tenant_id, object_kind);

create index ix_object_type_tenant_category
  on app.object_type (tenant_id, category_id);

create index ix_graph_object_tenant_category_type
  on app.graph_object (tenant_id, category_id, type_id);

create index ix_graph_object_tenant_status
  on app.graph_object (tenant_id, object_status);

create index ix_graph_object_properties_gin
  on app.graph_object
  using gin (properties jsonb_path_ops);
