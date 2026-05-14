\set ON_ERROR_STOP on

do $$
declare
  v_tenant_id uuid := gen_random_uuid();

  v_net_category_id uuid := gen_random_uuid();
  v_layer_category_id uuid := gen_random_uuid();
  v_node_category_id uuid := gen_random_uuid();
  v_edge_category_id uuid := gen_random_uuid();
  v_contain_category_id uuid := gen_random_uuid();

  v_net_type_id uuid := gen_random_uuid();
  v_layer_type_id uuid := gen_random_uuid();
  v_node_type_id uuid := gen_random_uuid();
  v_edge_type_id uuid := gen_random_uuid();
  v_contain_type_id uuid := gen_random_uuid();

  v_net_id uuid := gen_random_uuid();
  v_layer_1_id uuid := gen_random_uuid();
  v_layer_2_id uuid := gen_random_uuid();
  v_node_a_id uuid := gen_random_uuid();
  v_node_b_id uuid := gen_random_uuid();
  v_node_c_id uuid := gen_random_uuid();
  v_node_d_id uuid := gen_random_uuid();
  v_edge_id uuid := gen_random_uuid();
  v_edge_2_id uuid := gen_random_uuid();

  v_operation_id uuid;
begin
  insert into app.tenant (
    tenant_id,
    code,
    name,
    status
  )
  values (
    v_tenant_id,
    'smoke-tenant',
    'Smoke Tenant',
    'active'
  );

  insert into app.object_category (
    category_id,
    tenant_id,
    object_kind,
    name,
    json_schema,
    status
  )
  values
    (v_net_category_id, v_tenant_id, 'net', 'Net Category', '{}'::jsonb, 'active'),
    (v_layer_category_id, v_tenant_id, 'layer', 'Layer Category', '{}'::jsonb, 'active'),
    (v_node_category_id, v_tenant_id, 'node', 'Node Category', '{}'::jsonb, 'active'),
    (v_edge_category_id, v_tenant_id, 'edge', 'Edge Category', '{}'::jsonb, 'active'),
    (v_contain_category_id, v_tenant_id, 'contain', 'Contain Category', '{}'::jsonb, 'active');

  insert into app.object_type (
    type_id,
    tenant_id,
    category_id,
    name,
    json_schema,
    status
  )
  values
    (v_net_type_id, v_tenant_id, v_net_category_id, 'Net Type', '{}'::jsonb, 'active'),
    (v_layer_type_id, v_tenant_id, v_layer_category_id, 'Layer Type', '{}'::jsonb, 'active'),
    (v_node_type_id, v_tenant_id, v_node_category_id, 'Node Type', '{}'::jsonb, 'active'),
    (v_edge_type_id, v_tenant_id, v_edge_category_id, 'Edge Type', '{}'::jsonb, 'active'),
    (v_contain_type_id, v_tenant_id, v_contain_category_id, 'Contain Type', '{}'::jsonb, 'active');

  insert into app.graph_object (
    object_id,
    tenant_id,
    name,
    category_id,
    type_id,
    properties,
    status
  )
  values
    (v_net_id, v_tenant_id, 'Net 1', v_net_category_id, v_net_type_id, '{}'::jsonb, 'active'),
    (v_layer_1_id, v_tenant_id, 'Layer 1', v_layer_category_id, v_layer_type_id, '{}'::jsonb, 'active'),
    (v_layer_2_id, v_tenant_id, 'Layer 2', v_layer_category_id, v_layer_type_id, '{}'::jsonb, 'active'),
    (v_node_a_id, v_tenant_id, 'Node A', v_node_category_id, v_node_type_id, '{}'::jsonb, 'active'),
    (v_node_b_id, v_tenant_id, 'Node B', v_node_category_id, v_node_type_id, '{}'::jsonb, 'active'),
    (v_node_c_id, v_tenant_id, 'Node C', v_node_category_id, v_node_type_id, '{}'::jsonb, 'active'),
    (v_node_d_id, v_tenant_id, 'Node D', v_node_category_id, v_node_type_id, '{}'::jsonb, 'active'),
    (v_edge_id, v_tenant_id, 'Edge 1', v_edge_category_id, v_edge_type_id, '{}'::jsonb, 'active'),
    (v_edge_2_id, v_tenant_id, 'Edge 2', v_edge_category_id, v_edge_type_id, '{}'::jsonb, 'active');

  insert into app.net_layer_relation (
    tenant_id,
    net_id,
    net_category_id,
    layer_id,
    layer_category_id,
    status
  )
  values
    (v_tenant_id, v_net_id, v_net_category_id, v_layer_1_id, v_layer_category_id, 'active'),
    (v_tenant_id, v_net_id, v_net_category_id, v_layer_2_id, v_layer_category_id, 'active');

  insert into app.layer_node_relation (
    tenant_id,
    layer_id,
    layer_category_id,
    node_id,
    node_category_id,
    status
  )
  values
    (v_tenant_id, v_layer_1_id, v_layer_category_id, v_node_a_id, v_node_category_id, 'active'),
    (v_tenant_id, v_layer_1_id, v_layer_category_id, v_node_b_id, v_node_category_id, 'active'),
    (v_tenant_id, v_layer_1_id, v_layer_category_id, v_node_c_id, v_node_category_id, 'active'),
    (v_tenant_id, v_layer_2_id, v_layer_category_id, v_node_d_id, v_node_category_id, 'active');

  insert into app.node_tree_relation (
    tenant_id,
    layer_id,
    parent_node_id,
    parent_node_category_id,
    child_node_id,
    child_node_category_id,
    status
  )
  values
    (v_tenant_id, v_layer_1_id, v_node_a_id, v_node_category_id, v_node_b_id, v_node_category_id, 'active'),
    (v_tenant_id, v_layer_1_id, v_node_b_id, v_node_category_id, v_node_c_id, v_node_category_id, 'active');

  if not exists (
    select 1
    from app.node_closure nc
    where nc.tenant_id = v_tenant_id
      and nc.layer_id = v_layer_1_id
      and nc.ancestor_node_id = v_node_a_id
      and nc.descendant_node_id = v_node_a_id
      and nc.depth = 0
  ) then
    raise exception 'Missing node_closure row A -> A depth 0';
  end if;

  if not exists (
    select 1
    from app.node_closure nc
    where nc.tenant_id = v_tenant_id
      and nc.layer_id = v_layer_1_id
      and nc.ancestor_node_id = v_node_a_id
      and nc.descendant_node_id = v_node_b_id
      and nc.depth = 1
  ) then
    raise exception 'Missing node_closure row A -> B depth 1';
  end if;

  if not exists (
    select 1
    from app.node_closure nc
    where nc.tenant_id = v_tenant_id
      and nc.layer_id = v_layer_1_id
      and nc.ancestor_node_id = v_node_a_id
      and nc.descendant_node_id = v_node_c_id
      and nc.depth = 2
  ) then
    raise exception 'Missing node_closure row A -> C depth 2';
  end if;

  if not exists (
    select 1
    from app.node_closure nc
    where nc.tenant_id = v_tenant_id
      and nc.layer_id = v_layer_1_id
      and nc.ancestor_node_id = v_node_b_id
      and nc.descendant_node_id = v_node_c_id
      and nc.depth = 1
  ) then
    raise exception 'Missing node_closure row B -> C depth 1';
  end if;

  insert into app.edge_endpoint_relation (
    tenant_id,
    layer_id,
    edge_id,
    edge_category_id,
    edge_type_id,
    source_node_id,
    source_node_category_id,
    source_node_type_id,
    target_node_id,
    target_node_category_id,
    target_node_type_id,
    is_directed,
    status
  )
  values (
    v_tenant_id,
    v_layer_1_id,
    v_edge_id,
    v_edge_category_id,
    v_edge_type_id,
    v_node_b_id,
    v_node_category_id,
    v_node_type_id,
    v_node_c_id,
    v_node_category_id,
    v_node_type_id,
    false,
    'active'
  );

  if not exists (
    select 1
    from app.virtual_edge_index vei
    where vei.tenant_id = v_tenant_id
      and vei.layer_id = v_layer_1_id
      and vei.edge_id = v_edge_id
      and vei.source_node_id = v_node_b_id
      and vei.target_node_id = v_node_c_id
      and vei.source_ancestor_node_id = v_node_a_id
      and vei.target_ancestor_node_id = v_node_a_id
      and vei.depth_from_source_ancestor = 1
      and vei.depth_from_target_ancestor = 2
      and vei.status = 'active'
  ) then
    raise exception 'Missing virtual_edge_index row proving ancestor A is virtually connected through descendants B and C';
  end if;

  if not exists (
    select 1
    from app.virtual_connection_summary vcs
    where vcs.tenant_id = v_tenant_id
      and vcs.layer_id = v_layer_1_id
      and vcs.source_ancestor_node_id = v_node_a_id
      and vcs.target_ancestor_node_id = v_node_a_id
      and vcs.edge_count = 1
      and vcs.direct_edge_count = 0
  ) then
    raise exception 'Missing virtual_connection_summary row proving A is virtually connected through descendants';
  end if;

  begin
    insert into app.node_tree_relation (
      tenant_id,
      layer_id,
      parent_node_id,
      parent_node_category_id,
      child_node_id,
      child_node_category_id,
      status
    )
    values (
      v_tenant_id,
      v_layer_1_id,
      v_node_c_id,
      v_node_category_id,
      v_node_a_id,
      v_node_category_id,
      'active'
    );

    raise exception 'Node cycle insert should have failed';
  exception
    when others then
      if position('cycle' in lower(sqlerrm)) = 0 then
        raise exception 'Node cycle failed for unexpected reason: %', sqlerrm;
      end if;
  end;

  begin
    insert into app.edge_endpoint_relation (
      tenant_id,
      layer_id,
      edge_id,
      edge_category_id,
      edge_type_id,
      source_node_id,
      source_node_category_id,
      source_node_type_id,
      target_node_id,
      target_node_category_id,
      target_node_type_id,
      is_directed,
      status
    )
    values (
      v_tenant_id,
      v_layer_1_id,
      v_edge_2_id,
      v_edge_category_id,
      v_edge_type_id,
      v_node_b_id,
      v_node_category_id,
      v_node_type_id,
      v_node_d_id,
      v_node_category_id,
      v_node_type_id,
      false,
      'active'
    );

    raise exception 'Cross-layer edge insert should have failed';
  exception
    when others then
      if position('layer' in lower(sqlerrm)) = 0 then
        raise exception 'Cross-layer edge failed for unexpected reason: %', sqlerrm;
      end if;
  end;

  v_operation_id := app.soft_delete_object(v_tenant_id, v_node_c_id, null);

  if v_operation_id is null then
    raise exception 'soft_delete_object returned null operation_id';
  end if;

  if not exists (
    select 1
    from app.graph_object go
    where go.tenant_id = v_tenant_id
      and go.object_id = v_node_c_id
      and go.status = 'deleted'
  ) then
    raise exception 'graph_object was not soft-deleted';
  end if;

  if not exists (
    select 1
    from app.layer_node_relation lnr
    where lnr.tenant_id = v_tenant_id
      and lnr.layer_id = v_layer_1_id
      and lnr.node_id = v_node_c_id
      and lnr.status = 'deleted'
  ) then
    raise exception 'layer_node_relation was not soft-deleted';
  end if;

  if not exists (
    select 1
    from app.node_tree_relation ntr
    where ntr.tenant_id = v_tenant_id
      and ntr.layer_id = v_layer_1_id
      and ntr.parent_node_id = v_node_b_id
      and ntr.child_node_id = v_node_c_id
      and ntr.status = 'deleted'
  ) then
    raise exception 'node_tree_relation was not soft-deleted';
  end if;

  if not exists (
    select 1
    from app.edge_endpoint_relation eer
    where eer.tenant_id = v_tenant_id
      and eer.edge_id = v_edge_id
      and eer.target_node_id = v_node_c_id
      and eer.status = 'deleted'
  ) then
    raise exception 'edge_endpoint_relation was not soft-deleted';
  end if;

  raise notice 'Smoke test completed successfully';
end
$$;
