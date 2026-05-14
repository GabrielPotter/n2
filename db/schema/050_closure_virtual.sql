\set ON_ERROR_STOP on

-- TODO in 070_functions_triggers.sql: rebuild_node_closure_for_layer
-- TODO in 070_functions_triggers.sql: rebuild_virtual_connections_for_layer
-- TODO in 070_functions_triggers.sql: rebuild_virtual_connections_for_edge

create table app.node_closure (
  tenant_id uuid not null,
  layer_id uuid not null,
  ancestor_node_id uuid not null,
  descendant_node_id uuid not null,
  depth integer not null,
  primary key (tenant_id, layer_id, ancestor_node_id, descendant_node_id),
  constraint chk_node_closure_depth_non_negative
    check (depth >= 0)
);

comment on table app.node_closure is
  'Closure table for fast node subtree queries. Each node must have a reflexive row where ancestor_node_id = descendant_node_id and depth = 0.';

create index ix_node_closure_descendant_lookup
  on app.node_closure (tenant_id, layer_id, descendant_node_id, ancestor_node_id);

create index ix_node_closure_ancestor_depth_lookup
  on app.node_closure (tenant_id, layer_id, ancestor_node_id, depth, descendant_node_id);

create table app.virtual_edge_index (
  tenant_id uuid not null,
  layer_id uuid not null,
  edge_id uuid not null,
  source_node_id uuid not null,
  target_node_id uuid not null,
  source_ancestor_node_id uuid not null,
  target_ancestor_node_id uuid not null,
  depth_from_source_ancestor integer not null,
  depth_from_target_ancestor integer not null,
  status app.record_status not null default 'active',
  primary key (
    tenant_id,
    layer_id,
    edge_id,
    source_ancestor_node_id,
    target_ancestor_node_id
  ),
  constraint chk_virtual_edge_index_source_depth_non_negative
    check (depth_from_source_ancestor >= 0),
  constraint chk_virtual_edge_index_target_depth_non_negative
    check (depth_from_target_ancestor >= 0)
);

create index ix_virtual_edge_index_active_source_ancestor_lookup
  on app.virtual_edge_index (tenant_id, layer_id, source_ancestor_node_id, target_ancestor_node_id)
  where status = 'active';

create index ix_virtual_edge_index_active_target_ancestor_lookup
  on app.virtual_edge_index (tenant_id, layer_id, target_ancestor_node_id, source_ancestor_node_id)
  where status = 'active';

create index ix_virtual_edge_index_edge_lookup
  on app.virtual_edge_index (tenant_id, layer_id, edge_id);

create table app.virtual_connection_summary (
  tenant_id uuid not null,
  layer_id uuid not null,
  source_ancestor_node_id uuid not null,
  target_ancestor_node_id uuid not null,
  edge_count integer not null,
  direct_edge_count integer not null default 0,
  updated_at timestamptz not null default now(),
  primary key (
    tenant_id,
    layer_id,
    source_ancestor_node_id,
    target_ancestor_node_id
  ),
  constraint chk_virtual_connection_summary_edge_count_non_negative
    check (edge_count >= 0),
  constraint chk_virtual_connection_summary_direct_edge_count_non_negative
    check (direct_edge_count >= 0)
);

comment on table app.virtual_connection_summary is
  'A and B are virtually connected if any descendant of A, including A itself, has an edge to any descendant of B, including B itself.';

create index ix_virtual_connection_summary_source_lookup
  on app.virtual_connection_summary (tenant_id, layer_id, source_ancestor_node_id, target_ancestor_node_id);

create index ix_virtual_connection_summary_target_lookup
  on app.virtual_connection_summary (tenant_id, layer_id, target_ancestor_node_id, source_ancestor_node_id);
