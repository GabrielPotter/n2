\set ON_ERROR_STOP on

-- TODO: active object validation
-- TODO: same-layer edge validation
-- TODO: different-layer contain validation
-- TODO: cycle prevention
-- TODO: rule validation

create table app.net_layer_relation (
  relation_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  net_id uuid not null,
  net_category_id uuid not null,
  net_kind app.object_kind not null default 'net',
  layer_id uuid not null,
  layer_category_id uuid not null,
  layer_kind app.object_kind not null default 'layer',
  relation_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  created_by uuid,
  deleted_at timestamptz,
  deleted_by uuid,
  delete_operation_id uuid,
  constraint chk_net_layer_relation_net_kind
    check (net_kind = 'net'),
  constraint chk_net_layer_relation_layer_kind
    check (layer_kind = 'layer'),
  constraint fk_net_layer_relation_net_object
    foreign key (tenant_id, net_id, net_category_id)
    references app.graph_object (tenant_id, object_id, category_id),
  constraint fk_net_layer_relation_net_category_kind
    foreign key (tenant_id, net_category_id, net_kind)
    references app.object_category (tenant_id, category_id, object_kind),
  constraint fk_net_layer_relation_layer_object
    foreign key (tenant_id, layer_id, layer_category_id)
    references app.graph_object (tenant_id, object_id, category_id),
  constraint fk_net_layer_relation_layer_category_kind
    foreign key (tenant_id, layer_category_id, layer_kind)
    references app.object_category (tenant_id, category_id, object_kind)
);

create unique index uq_net_layer_relation_active_layer_once
  on app.net_layer_relation (tenant_id, layer_id)
  where relation_status = 'active';

create index ix_net_layer_relation_active_net_layers
  on app.net_layer_relation (tenant_id, net_id)
  where relation_status = 'active';

create table app.layer_tree_relation (
  relation_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  net_id uuid not null,
  parent_layer_id uuid not null,
  parent_layer_category_id uuid not null,
  parent_layer_kind app.object_kind not null default 'layer',
  child_layer_id uuid not null,
  child_layer_category_id uuid not null,
  child_layer_kind app.object_kind not null default 'layer',
  relation_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  created_by uuid,
  deleted_at timestamptz,
  deleted_by uuid,
  delete_operation_id uuid,
  constraint chk_layer_tree_relation_parent_kind
    check (parent_layer_kind = 'layer'),
  constraint chk_layer_tree_relation_child_kind
    check (child_layer_kind = 'layer'),
  constraint chk_layer_tree_relation_distinct_layers
    check (parent_layer_id <> child_layer_id),
  constraint fk_layer_tree_relation_net_object
    foreign key (tenant_id, net_id)
    references app.graph_object (tenant_id, object_id),
  constraint fk_layer_tree_relation_parent_object
    foreign key (tenant_id, parent_layer_id, parent_layer_category_id)
    references app.graph_object (tenant_id, object_id, category_id),
  constraint fk_layer_tree_relation_parent_category_kind
    foreign key (tenant_id, parent_layer_category_id, parent_layer_kind)
    references app.object_category (tenant_id, category_id, object_kind),
  constraint fk_layer_tree_relation_child_object
    foreign key (tenant_id, child_layer_id, child_layer_category_id)
    references app.graph_object (tenant_id, object_id, category_id),
  constraint fk_layer_tree_relation_child_category_kind
    foreign key (tenant_id, child_layer_category_id, child_layer_kind)
    references app.object_category (tenant_id, category_id, object_kind)
);

create unique index uq_layer_tree_relation_active_child_once
  on app.layer_tree_relation (tenant_id, net_id, child_layer_id)
  where relation_status = 'active';

create index ix_layer_tree_relation_active_parent_lookup
  on app.layer_tree_relation (tenant_id, net_id, parent_layer_id)
  where relation_status = 'active';

create index ix_layer_tree_relation_active_child_lookup
  on app.layer_tree_relation (tenant_id, net_id, child_layer_id)
  where relation_status = 'active';

create table app.layer_node_relation (
  relation_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  layer_id uuid not null,
  layer_category_id uuid not null,
  layer_kind app.object_kind not null default 'layer',
  node_id uuid not null,
  node_category_id uuid not null,
  node_kind app.object_kind not null default 'node',
  relation_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  created_by uuid,
  deleted_at timestamptz,
  deleted_by uuid,
  delete_operation_id uuid,
  constraint chk_layer_node_relation_layer_kind
    check (layer_kind = 'layer'),
  constraint chk_layer_node_relation_node_kind
    check (node_kind = 'node'),
  constraint fk_layer_node_relation_layer_object
    foreign key (tenant_id, layer_id, layer_category_id)
    references app.graph_object (tenant_id, object_id, category_id),
  constraint fk_layer_node_relation_layer_category_kind
    foreign key (tenant_id, layer_category_id, layer_kind)
    references app.object_category (tenant_id, category_id, object_kind),
  constraint fk_layer_node_relation_node_object
    foreign key (tenant_id, node_id, node_category_id)
    references app.graph_object (tenant_id, object_id, category_id),
  constraint fk_layer_node_relation_node_category_kind
    foreign key (tenant_id, node_category_id, node_kind)
    references app.object_category (tenant_id, category_id, object_kind)
);

create unique index uq_layer_node_relation_active_node_once
  on app.layer_node_relation (tenant_id, node_id)
  where relation_status = 'active';

create index ix_layer_node_relation_active_layer_nodes
  on app.layer_node_relation (tenant_id, layer_id)
  where relation_status = 'active';

create table app.node_tree_relation (
  relation_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  layer_id uuid not null,
  parent_node_id uuid not null,
  parent_node_category_id uuid not null,
  parent_node_kind app.object_kind not null default 'node',
  child_node_id uuid not null,
  child_node_category_id uuid not null,
  child_node_kind app.object_kind not null default 'node',
  relation_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  created_by uuid,
  deleted_at timestamptz,
  deleted_by uuid,
  delete_operation_id uuid,
  constraint chk_node_tree_relation_parent_kind
    check (parent_node_kind = 'node'),
  constraint chk_node_tree_relation_child_kind
    check (child_node_kind = 'node'),
  constraint chk_node_tree_relation_distinct_nodes
    check (parent_node_id <> child_node_id),
  constraint fk_node_tree_relation_layer_object
    foreign key (tenant_id, layer_id)
    references app.graph_object (tenant_id, object_id),
  constraint fk_node_tree_relation_parent_object
    foreign key (tenant_id, parent_node_id, parent_node_category_id)
    references app.graph_object (tenant_id, object_id, category_id),
  constraint fk_node_tree_relation_parent_category_kind
    foreign key (tenant_id, parent_node_category_id, parent_node_kind)
    references app.object_category (tenant_id, category_id, object_kind),
  constraint fk_node_tree_relation_child_object
    foreign key (tenant_id, child_node_id, child_node_category_id)
    references app.graph_object (tenant_id, object_id, category_id),
  constraint fk_node_tree_relation_child_category_kind
    foreign key (tenant_id, child_node_category_id, child_node_kind)
    references app.object_category (tenant_id, category_id, object_kind)
);

create unique index uq_node_tree_relation_active_child_once
  on app.node_tree_relation (tenant_id, layer_id, child_node_id)
  where relation_status = 'active';

create index ix_node_tree_relation_active_parent_lookup
  on app.node_tree_relation (tenant_id, layer_id, parent_node_id)
  where relation_status = 'active';

create index ix_node_tree_relation_active_child_lookup
  on app.node_tree_relation (tenant_id, layer_id, child_node_id)
  where relation_status = 'active';

create table app.edge_endpoint_relation (
  relation_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  layer_id uuid not null,
  edge_id uuid not null,
  edge_category_id uuid not null,
  edge_type_id uuid not null,
  edge_kind app.object_kind not null default 'edge',
  source_node_id uuid not null,
  source_node_category_id uuid not null,
  source_node_type_id uuid not null,
  source_node_kind app.object_kind not null default 'node',
  target_node_id uuid not null,
  target_node_category_id uuid not null,
  target_node_type_id uuid not null,
  target_node_kind app.object_kind not null default 'node',
  is_directed boolean not null default false,
  relation_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  created_by uuid,
  deleted_at timestamptz,
  deleted_by uuid,
  delete_operation_id uuid,
  constraint chk_edge_endpoint_relation_edge_kind
    check (edge_kind = 'edge'),
  constraint chk_edge_endpoint_relation_source_kind
    check (source_node_kind = 'node'),
  constraint chk_edge_endpoint_relation_target_kind
    check (target_node_kind = 'node'),
  constraint fk_edge_endpoint_relation_layer_object
    foreign key (tenant_id, layer_id)
    references app.graph_object (tenant_id, object_id),
  constraint fk_edge_endpoint_relation_edge_object
    foreign key (tenant_id, edge_id, edge_category_id, edge_type_id)
    references app.graph_object (tenant_id, object_id, category_id, type_id),
  constraint fk_edge_endpoint_relation_edge_category_kind
    foreign key (tenant_id, edge_category_id, edge_kind)
    references app.object_category (tenant_id, category_id, object_kind),
  constraint fk_edge_endpoint_relation_source_object
    foreign key (tenant_id, source_node_id, source_node_category_id, source_node_type_id)
    references app.graph_object (tenant_id, object_id, category_id, type_id),
  constraint fk_edge_endpoint_relation_source_category_kind
    foreign key (tenant_id, source_node_category_id, source_node_kind)
    references app.object_category (tenant_id, category_id, object_kind),
  constraint fk_edge_endpoint_relation_target_object
    foreign key (tenant_id, target_node_id, target_node_category_id, target_node_type_id)
    references app.graph_object (tenant_id, object_id, category_id, type_id),
  constraint fk_edge_endpoint_relation_target_category_kind
    foreign key (tenant_id, target_node_category_id, target_node_kind)
    references app.object_category (tenant_id, category_id, object_kind)
);

create unique index uq_edge_endpoint_relation_active_edge_once
  on app.edge_endpoint_relation (tenant_id, edge_id)
  where relation_status = 'active';

create index ix_edge_endpoint_relation_active_source_lookup
  on app.edge_endpoint_relation (tenant_id, source_node_id)
  where relation_status = 'active';

create index ix_edge_endpoint_relation_active_target_lookup
  on app.edge_endpoint_relation (tenant_id, target_node_id)
  where relation_status = 'active';

create index ix_edge_endpoint_relation_active_source_target_lookup
  on app.edge_endpoint_relation (tenant_id, source_node_id, target_node_id)
  where relation_status = 'active';

create table app.contain_endpoint_relation (
  relation_id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null,
  contain_id uuid not null,
  contain_category_id uuid not null,
  contain_type_id uuid not null,
  contain_kind app.object_kind not null default 'contain',
  source_layer_id uuid not null,
  source_object_id uuid not null,
  source_object_category_id uuid not null,
  source_object_type_id uuid not null,
  target_layer_id uuid not null,
  target_object_id uuid not null,
  target_object_category_id uuid not null,
  target_object_type_id uuid not null,
  relation_status app.record_status not null default 'active',
  created_at timestamptz not null default now(),
  created_by uuid,
  deleted_at timestamptz,
  deleted_by uuid,
  delete_operation_id uuid,
  constraint chk_contain_endpoint_relation_contain_kind
    check (contain_kind = 'contain'),
  constraint chk_contain_endpoint_relation_distinct_layers
    check (source_layer_id <> target_layer_id),
  constraint fk_contain_endpoint_relation_contain_object
    foreign key (tenant_id, contain_id, contain_category_id, contain_type_id)
    references app.graph_object (tenant_id, object_id, category_id, type_id),
  constraint fk_contain_endpoint_relation_contain_category_kind
    foreign key (tenant_id, contain_category_id, contain_kind)
    references app.object_category (tenant_id, category_id, object_kind),
  constraint fk_contain_endpoint_relation_source_layer_object
    foreign key (tenant_id, source_layer_id)
    references app.graph_object (tenant_id, object_id),
  constraint fk_contain_endpoint_relation_source_object
    foreign key (tenant_id, source_object_id, source_object_category_id, source_object_type_id)
    references app.graph_object (tenant_id, object_id, category_id, type_id),
  constraint fk_contain_endpoint_relation_target_layer_object
    foreign key (tenant_id, target_layer_id)
    references app.graph_object (tenant_id, object_id),
  constraint fk_contain_endpoint_relation_target_object
    foreign key (tenant_id, target_object_id, target_object_category_id, target_object_type_id)
    references app.graph_object (tenant_id, object_id, category_id, type_id)
);

create unique index uq_contain_endpoint_relation_active_contain_once
  on app.contain_endpoint_relation (tenant_id, contain_id)
  where relation_status = 'active';

create index ix_contain_endpoint_relation_active_source_lookup
  on app.contain_endpoint_relation (tenant_id, source_layer_id, source_object_id)
  where relation_status = 'active';

create index ix_contain_endpoint_relation_active_target_lookup
  on app.contain_endpoint_relation (tenant_id, target_layer_id, target_object_id)
  where relation_status = 'active';
