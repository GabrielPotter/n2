\set ON_ERROR_STOP on

create table app.rule_net_category_layer_category (
  tenant_id uuid not null,
  net_category_id uuid not null,
  layer_category_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, net_category_id, layer_category_id),
  constraint fk_rule_net_category_layer_category_net_category
    foreign key (tenant_id, net_category_id)
    references app.object_category (tenant_id, category_id),
  constraint fk_rule_net_category_layer_category_layer_category
    foreign key (tenant_id, layer_category_id)
    references app.object_category (tenant_id, category_id)
);

comment on table app.rule_net_category_layer_category is
  'Tenant-specific, ID-only rule table. Defines which layer categories may exist inside a net category.';

create index ix_rule_net_category_layer_category_active_lookup
  on app.rule_net_category_layer_category (tenant_id, net_category_id)
  where status = 'active';

create table app.rule_net_type_layer_type (
  tenant_id uuid not null,
  net_category_id uuid not null,
  net_type_id uuid not null,
  layer_category_id uuid not null,
  layer_type_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, net_category_id, net_type_id, layer_category_id, layer_type_id),
  constraint fk_rule_net_type_layer_type_net_type
    foreign key (tenant_id, net_category_id, net_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_net_type_layer_type_layer_type
    foreign key (tenant_id, layer_category_id, layer_type_id)
    references app.object_type (tenant_id, category_id, type_id)
);

comment on table app.rule_net_type_layer_type is
  'Tenant-specific, ID-only rule table. Defines which layer types may exist inside a net type.';

create index ix_rule_net_type_layer_type_active_lookup
  on app.rule_net_type_layer_type (tenant_id, net_category_id, net_type_id)
  where status = 'active';

create table app.rule_net_category_contain_category (
  tenant_id uuid not null,
  net_category_id uuid not null,
  contain_category_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, net_category_id, contain_category_id),
  constraint fk_rule_net_category_contain_category_net_category
    foreign key (tenant_id, net_category_id)
    references app.object_category (tenant_id, category_id),
  constraint fk_rule_net_category_contain_category_contain_category
    foreign key (tenant_id, contain_category_id)
    references app.object_category (tenant_id, category_id)
);

comment on table app.rule_net_category_contain_category is
  'Tenant-specific, ID-only rule table. Defines which contain categories may exist inside a net category.';

create index ix_rule_net_category_contain_category_active_lookup
  on app.rule_net_category_contain_category (tenant_id, net_category_id)
  where status = 'active';

create table app.rule_net_type_contain_type (
  tenant_id uuid not null,
  net_category_id uuid not null,
  net_type_id uuid not null,
  contain_category_id uuid not null,
  contain_type_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, net_category_id, net_type_id, contain_category_id, contain_type_id),
  constraint fk_rule_net_type_contain_type_net_type
    foreign key (tenant_id, net_category_id, net_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_net_type_contain_type_contain_type
    foreign key (tenant_id, contain_category_id, contain_type_id)
    references app.object_type (tenant_id, category_id, type_id)
);

comment on table app.rule_net_type_contain_type is
  'Tenant-specific, ID-only rule table. Defines which contain types may exist inside a net type.';

create index ix_rule_net_type_contain_type_active_lookup
  on app.rule_net_type_contain_type (tenant_id, net_category_id, net_type_id)
  where status = 'active';

create table app.rule_layer_category_child_layer_category (
  tenant_id uuid not null,
  parent_layer_category_id uuid not null,
  child_layer_category_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, parent_layer_category_id, child_layer_category_id),
  constraint fk_rule_layer_category_child_layer_category_parent
    foreign key (tenant_id, parent_layer_category_id)
    references app.object_category (tenant_id, category_id),
  constraint fk_rule_layer_category_child_layer_category_child
    foreign key (tenant_id, child_layer_category_id)
    references app.object_category (tenant_id, category_id)
);

comment on table app.rule_layer_category_child_layer_category is
  'Tenant-specific, ID-only rule table. Defines which child layer categories may exist under a parent layer category.';

create index ix_rule_layer_category_child_layer_category_active_lookup
  on app.rule_layer_category_child_layer_category (tenant_id, parent_layer_category_id)
  where status = 'active';

create table app.rule_layer_type_child_layer_type (
  tenant_id uuid not null,
  parent_layer_category_id uuid not null,
  parent_layer_type_id uuid not null,
  child_layer_category_id uuid not null,
  child_layer_type_id uuid not null,
  status app.record_status not null default 'active',
  primary key (
    tenant_id,
    parent_layer_category_id,
    parent_layer_type_id,
    child_layer_category_id,
    child_layer_type_id
  ),
  constraint fk_rule_layer_type_child_layer_type_parent
    foreign key (tenant_id, parent_layer_category_id, parent_layer_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_layer_type_child_layer_type_child
    foreign key (tenant_id, child_layer_category_id, child_layer_type_id)
    references app.object_type (tenant_id, category_id, type_id)
);

comment on table app.rule_layer_type_child_layer_type is
  'Tenant-specific, ID-only rule table. Defines which child layer types may exist under a parent layer type.';

create index ix_rule_layer_type_child_layer_type_active_lookup
  on app.rule_layer_type_child_layer_type (tenant_id, parent_layer_category_id, parent_layer_type_id)
  where status = 'active';

create table app.rule_layer_category_node_category (
  tenant_id uuid not null,
  layer_category_id uuid not null,
  node_category_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, layer_category_id, node_category_id),
  constraint fk_rule_layer_category_node_category_layer
    foreign key (tenant_id, layer_category_id)
    references app.object_category (tenant_id, category_id),
  constraint fk_rule_layer_category_node_category_node
    foreign key (tenant_id, node_category_id)
    references app.object_category (tenant_id, category_id)
);

comment on table app.rule_layer_category_node_category is
  'Tenant-specific, ID-only rule table. Defines which node categories may exist inside a layer category.';

create index ix_rule_layer_category_node_category_active_lookup
  on app.rule_layer_category_node_category (tenant_id, layer_category_id)
  where status = 'active';

create table app.rule_layer_type_node_type (
  tenant_id uuid not null,
  layer_category_id uuid not null,
  layer_type_id uuid not null,
  node_category_id uuid not null,
  node_type_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, layer_category_id, layer_type_id, node_category_id, node_type_id),
  constraint fk_rule_layer_type_node_type_layer
    foreign key (tenant_id, layer_category_id, layer_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_layer_type_node_type_node
    foreign key (tenant_id, node_category_id, node_type_id)
    references app.object_type (tenant_id, category_id, type_id)
);

comment on table app.rule_layer_type_node_type is
  'Tenant-specific, ID-only rule table. Defines which node types may exist inside a layer type.';

create index ix_rule_layer_type_node_type_active_lookup
  on app.rule_layer_type_node_type (tenant_id, layer_category_id, layer_type_id)
  where status = 'active';

create table app.rule_layer_category_edge_category (
  tenant_id uuid not null,
  layer_category_id uuid not null,
  edge_category_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, layer_category_id, edge_category_id),
  constraint fk_rule_layer_category_edge_category_layer
    foreign key (tenant_id, layer_category_id)
    references app.object_category (tenant_id, category_id),
  constraint fk_rule_layer_category_edge_category_edge
    foreign key (tenant_id, edge_category_id)
    references app.object_category (tenant_id, category_id)
);

comment on table app.rule_layer_category_edge_category is
  'Tenant-specific, ID-only rule table. Defines which edge categories may exist inside a layer category.';

create index ix_rule_layer_category_edge_category_active_lookup
  on app.rule_layer_category_edge_category (tenant_id, layer_category_id)
  where status = 'active';

create table app.rule_layer_type_edge_type (
  tenant_id uuid not null,
  layer_category_id uuid not null,
  layer_type_id uuid not null,
  edge_category_id uuid not null,
  edge_type_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, layer_category_id, layer_type_id, edge_category_id, edge_type_id),
  constraint fk_rule_layer_type_edge_type_layer
    foreign key (tenant_id, layer_category_id, layer_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_layer_type_edge_type_edge
    foreign key (tenant_id, edge_category_id, edge_type_id)
    references app.object_type (tenant_id, category_id, type_id)
);

comment on table app.rule_layer_type_edge_type is
  'Tenant-specific, ID-only rule table. Defines which edge types may exist inside a layer type.';

create index ix_rule_layer_type_edge_type_active_lookup
  on app.rule_layer_type_edge_type (tenant_id, layer_category_id, layer_type_id)
  where status = 'active';

create table app.rule_node_category_child_node_category (
  tenant_id uuid not null,
  parent_node_category_id uuid not null,
  child_node_category_id uuid not null,
  status app.record_status not null default 'active',
  primary key (tenant_id, parent_node_category_id, child_node_category_id),
  constraint fk_rule_node_category_child_node_category_parent
    foreign key (tenant_id, parent_node_category_id)
    references app.object_category (tenant_id, category_id),
  constraint fk_rule_node_category_child_node_category_child
    foreign key (tenant_id, child_node_category_id)
    references app.object_category (tenant_id, category_id)
);

comment on table app.rule_node_category_child_node_category is
  'Tenant-specific, ID-only rule table. Defines which child node categories may exist under a parent node category.';

create index ix_rule_node_category_child_node_category_active_lookup
  on app.rule_node_category_child_node_category (tenant_id, parent_node_category_id)
  where status = 'active';

create table app.rule_node_type_child_node_type (
  tenant_id uuid not null,
  parent_node_category_id uuid not null,
  parent_node_type_id uuid not null,
  child_node_category_id uuid not null,
  child_node_type_id uuid not null,
  status app.record_status not null default 'active',
  primary key (
    tenant_id,
    parent_node_category_id,
    parent_node_type_id,
    child_node_category_id,
    child_node_type_id
  ),
  constraint fk_rule_node_type_child_node_type_parent
    foreign key (tenant_id, parent_node_category_id, parent_node_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_node_type_child_node_type_child
    foreign key (tenant_id, child_node_category_id, child_node_type_id)
    references app.object_type (tenant_id, category_id, type_id)
);

comment on table app.rule_node_type_child_node_type is
  'Tenant-specific, ID-only rule table. Defines which child node types may exist under a parent node type.';

create index ix_rule_node_type_child_node_type_active_lookup
  on app.rule_node_type_child_node_type (tenant_id, parent_node_category_id, parent_node_type_id)
  where status = 'active';

create table app.rule_edge_endpoint_type (
  tenant_id uuid not null,
  edge_category_id uuid not null,
  edge_type_id uuid not null,
  source_node_category_id uuid not null,
  source_node_type_id uuid not null,
  target_node_category_id uuid not null,
  target_node_type_id uuid not null,
  is_directed boolean not null default false,
  status app.record_status not null default 'active',
  primary key (
    tenant_id,
    edge_category_id,
    edge_type_id,
    source_node_category_id,
    source_node_type_id,
    target_node_category_id,
    target_node_type_id,
    is_directed
  ),
  constraint fk_rule_edge_endpoint_type_edge
    foreign key (tenant_id, edge_category_id, edge_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_edge_endpoint_type_source_node
    foreign key (tenant_id, source_node_category_id, source_node_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_edge_endpoint_type_target_node
    foreign key (tenant_id, target_node_category_id, target_node_type_id)
    references app.object_type (tenant_id, category_id, type_id)
);

comment on table app.rule_edge_endpoint_type is
  'Tenant-specific, ID-only rule table. Defines which source and target node types are allowed for an edge type.';

create index ix_rule_edge_endpoint_type_active_lookup
  on app.rule_edge_endpoint_type (tenant_id, edge_category_id, edge_type_id)
  where status = 'active';

create table app.rule_contain_endpoint_type (
  tenant_id uuid not null,
  contain_category_id uuid not null,
  contain_type_id uuid not null,
  source_object_category_id uuid not null,
  source_object_type_id uuid not null,
  target_object_category_id uuid not null,
  target_object_type_id uuid not null,
  status app.record_status not null default 'active',
  primary key (
    tenant_id,
    contain_category_id,
    contain_type_id,
    source_object_category_id,
    source_object_type_id,
    target_object_category_id,
    target_object_type_id
  ),
  constraint fk_rule_contain_endpoint_type_contain
    foreign key (tenant_id, contain_category_id, contain_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_contain_endpoint_type_source_object
    foreign key (tenant_id, source_object_category_id, source_object_type_id)
    references app.object_type (tenant_id, category_id, type_id),
  constraint fk_rule_contain_endpoint_type_target_object
    foreign key (tenant_id, target_object_category_id, target_object_type_id)
    references app.object_type (tenant_id, category_id, type_id)
);

comment on table app.rule_contain_endpoint_type is
  'Tenant-specific, ID-only rule table. Defines which source and target object types are allowed for a contain type.';

create index ix_rule_contain_endpoint_type_active_lookup
  on app.rule_contain_endpoint_type (tenant_id, contain_category_id, contain_type_id)
  where status = 'active';
