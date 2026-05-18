\set ON_ERROR_STOP on

-- TODO: full restore procedure
-- TODO: full virtual summary decrement/rebuild
-- TODO: subtree move procedure
-- TODO: JSON Schema validation
-- TODO: full category/type rule validation

create or replace function app.touch_updated_at()
returns trigger
language plpgsql
as $$
begin
  new.updated_at := now();
  return new;
end;
$$;

create or replace function app.assert_active_graph_object(
  p_tenant_id uuid,
  p_object_id uuid
)
returns void
language plpgsql
as $$
begin
  if not exists (
    select 1
    from app.graph_object go
    where go.tenant_id = p_tenant_id
      and go.object_id = p_object_id
      and go.object_status = 'active'
  ) then
    raise exception 'Active graph_object not found. tenant_id=%, object_id=%', p_tenant_id, p_object_id;
  end if;
end;
$$;

create or replace function app.assert_object_kind(
  p_tenant_id uuid,
  p_object_id uuid,
  p_expected_kind app.object_kind
)
returns void
language plpgsql
as $$
declare
  v_object_kind app.object_kind;
begin
  select oc.object_kind
  into v_object_kind
  from app.graph_object go
  join app.object_category oc
    on oc.tenant_id = go.tenant_id
   and oc.category_id = go.category_id
  where go.tenant_id = p_tenant_id
    and go.object_id = p_object_id;

  if v_object_kind is null then
    raise exception 'Graph object not found for kind check. tenant_id=%, object_id=%', p_tenant_id, p_object_id;
  end if;

  if v_object_kind <> p_expected_kind then
    raise exception 'Invalid object kind. tenant_id=%, object_id=%, expected=%, actual=%',
      p_tenant_id, p_object_id, p_expected_kind, v_object_kind;
  end if;
end;
$$;

create or replace function app.after_layer_node_relation_insert()
returns trigger
language plpgsql
as $$
begin
  if new.relation_status <> 'active' then
    return new;
  end if;

  insert into app.node_closure (
    tenant_id,
    layer_id,
    ancestor_node_id,
    descendant_node_id,
    depth
  )
  values (
    new.tenant_id,
    new.layer_id,
    new.node_id,
    new.node_id,
    0
  )
  on conflict do nothing;

  return new;
end;
$$;

create or replace function app.before_net_layer_relation_insert_validate()
returns trigger
language plpgsql
as $$
begin
  if new.relation_status <> 'active' then
    return new;
  end if;

  perform app.assert_active_graph_object(new.tenant_id, new.net_id);
  perform app.assert_object_kind(new.tenant_id, new.net_id, 'net');

  perform app.assert_active_graph_object(new.tenant_id, new.layer_id);
  perform app.assert_object_kind(new.tenant_id, new.layer_id, 'layer');

  return new;
end;
$$;

create or replace function app.before_layer_tree_relation_insert_validate()
returns trigger
language plpgsql
as $$
begin
  if new.relation_status <> 'active' then
    return new;
  end if;

  perform app.assert_active_graph_object(new.tenant_id, new.net_id);
  perform app.assert_object_kind(new.tenant_id, new.net_id, 'net');

  perform app.assert_active_graph_object(new.tenant_id, new.parent_layer_id);
  perform app.assert_object_kind(new.tenant_id, new.parent_layer_id, 'layer');

  perform app.assert_active_graph_object(new.tenant_id, new.child_layer_id);
  perform app.assert_object_kind(new.tenant_id, new.child_layer_id, 'layer');

  if new.parent_layer_id = new.child_layer_id then
    raise exception 'Parent layer and child layer must be different. tenant_id=%, net_id=%, layer_id=%',
      new.tenant_id, new.net_id, new.parent_layer_id;
  end if;

  if not exists (
    select 1
    from app.net_layer_relation nlr
    where nlr.tenant_id = new.tenant_id
      and nlr.net_id = new.net_id
      and nlr.layer_id = new.parent_layer_id
      and nlr.relation_status = 'active'
  ) then
    raise exception 'Parent layer is not an active member of the specified net. tenant_id=%, net_id=%, layer_id=%',
      new.tenant_id, new.net_id, new.parent_layer_id;
  end if;

  if not exists (
    select 1
    from app.net_layer_relation nlr
    where nlr.tenant_id = new.tenant_id
      and nlr.net_id = new.net_id
      and nlr.layer_id = new.child_layer_id
      and nlr.relation_status = 'active'
  ) then
    raise exception 'Child layer is not an active member of the specified net. tenant_id=%, net_id=%, layer_id=%',
      new.tenant_id, new.net_id, new.child_layer_id;
  end if;

  return new;
end;
$$;

create or replace function app.before_layer_node_relation_insert_validate()
returns trigger
language plpgsql
as $$
begin
  if new.relation_status <> 'active' then
    return new;
  end if;

  perform app.assert_active_graph_object(new.tenant_id, new.layer_id);
  perform app.assert_object_kind(new.tenant_id, new.layer_id, 'layer');

  perform app.assert_active_graph_object(new.tenant_id, new.node_id);
  perform app.assert_object_kind(new.tenant_id, new.node_id, 'node');

  return new;
end;
$$;

create or replace function app.before_node_tree_relation_insert_validate()
returns trigger
language plpgsql
as $$
begin
  if new.relation_status <> 'active' then
    return new;
  end if;

  perform app.assert_active_graph_object(new.tenant_id, new.layer_id);
  perform app.assert_object_kind(new.tenant_id, new.layer_id, 'layer');

  perform app.assert_active_graph_object(new.tenant_id, new.parent_node_id);
  perform app.assert_object_kind(new.tenant_id, new.parent_node_id, 'node');

  perform app.assert_active_graph_object(new.tenant_id, new.child_node_id);
  perform app.assert_object_kind(new.tenant_id, new.child_node_id, 'node');

  if new.parent_node_id = new.child_node_id then
    raise exception 'Parent node and child node must be different. tenant_id=%, layer_id=%, node_id=%',
      new.tenant_id, new.layer_id, new.parent_node_id;
  end if;

  if not exists (
    select 1
    from app.layer_node_relation lnr
    where lnr.tenant_id = new.tenant_id
      and lnr.layer_id = new.layer_id
      and lnr.node_id = new.parent_node_id
      and lnr.relation_status = 'active'
  ) then
    raise exception 'Parent node is not an active member of the specified layer. tenant_id=%, layer_id=%, node_id=%',
      new.tenant_id, new.layer_id, new.parent_node_id;
  end if;

  if not exists (
    select 1
    from app.layer_node_relation lnr
    where lnr.tenant_id = new.tenant_id
      and lnr.layer_id = new.layer_id
      and lnr.node_id = new.child_node_id
      and lnr.relation_status = 'active'
  ) then
    raise exception 'Child node is not an active member of the specified layer. tenant_id=%, layer_id=%, node_id=%',
      new.tenant_id, new.layer_id, new.child_node_id;
  end if;

  if exists (
    select 1
    from app.node_closure nc
    where nc.tenant_id = new.tenant_id
      and nc.layer_id = new.layer_id
      and nc.ancestor_node_id = new.child_node_id
      and nc.descendant_node_id = new.parent_node_id
  ) then
    raise exception 'Node tree cycle detected. tenant_id=%, layer_id=%, parent_node_id=%, child_node_id=%',
      new.tenant_id, new.layer_id, new.parent_node_id, new.child_node_id;
  end if;

  return new;
end;
$$;

create or replace function app.after_node_tree_relation_insert_update_closure()
returns trigger
language plpgsql
as $$
begin
  if new.relation_status <> 'active' then
    return new;
  end if;

  insert into app.node_closure (
    tenant_id,
    layer_id,
    ancestor_node_id,
    descendant_node_id,
    depth
  )
  select
    p.tenant_id,
    p.layer_id,
    p.ancestor_node_id,
    c.descendant_node_id,
    p.depth + c.depth + 1
  from app.node_closure p
  join app.node_closure c
    on c.tenant_id = p.tenant_id
   and c.layer_id = p.layer_id
  where p.tenant_id = new.tenant_id
    and p.layer_id = new.layer_id
    and p.descendant_node_id = new.parent_node_id
    and c.ancestor_node_id = new.child_node_id
  on conflict do nothing;

  return new;
end;
$$;

create or replace function app.before_edge_endpoint_relation_insert_validate()
returns trigger
language plpgsql
as $$
begin
  if new.relation_status <> 'active' then
    return new;
  end if;

  perform app.assert_active_graph_object(new.tenant_id, new.layer_id);
  perform app.assert_object_kind(new.tenant_id, new.layer_id, 'layer');

  perform app.assert_active_graph_object(new.tenant_id, new.edge_id);
  perform app.assert_object_kind(new.tenant_id, new.edge_id, 'edge');

  perform app.assert_active_graph_object(new.tenant_id, new.source_node_id);
  perform app.assert_object_kind(new.tenant_id, new.source_node_id, 'node');

  perform app.assert_active_graph_object(new.tenant_id, new.target_node_id);
  perform app.assert_object_kind(new.tenant_id, new.target_node_id, 'node');

  if not exists (
    select 1
    from app.layer_node_relation lnr
    where lnr.tenant_id = new.tenant_id
      and lnr.layer_id = new.layer_id
      and lnr.node_id = new.source_node_id
      and lnr.relation_status = 'active'
  ) then
    raise exception 'Source node is not an active member of the specified layer. tenant_id=%, layer_id=%, node_id=%',
      new.tenant_id, new.layer_id, new.source_node_id;
  end if;

  if not exists (
    select 1
    from app.layer_node_relation lnr
    where lnr.tenant_id = new.tenant_id
      and lnr.layer_id = new.layer_id
      and lnr.node_id = new.target_node_id
      and lnr.relation_status = 'active'
  ) then
    raise exception 'Target node is not an active member of the specified layer. tenant_id=%, layer_id=%, node_id=%',
      new.tenant_id, new.layer_id, new.target_node_id;
  end if;

  -- TODO: validate rule_edge_endpoint_type

  return new;
end;
$$;

create or replace function app.after_edge_endpoint_relation_insert_virtual_index()
returns trigger
language plpgsql
as $$
begin
  if new.relation_status <> 'active' then
    return new;
  end if;

  insert into app.virtual_edge_index (
    tenant_id,
    layer_id,
    edge_id,
    source_node_id,
    target_node_id,
    source_ancestor_node_id,
    target_ancestor_node_id,
    depth_from_source_ancestor,
    depth_from_target_ancestor,
    virtual_edge_status
  )
  select
    new.tenant_id,
    new.layer_id,
    new.edge_id,
    new.source_node_id,
    new.target_node_id,
    s.ancestor_node_id,
    t.ancestor_node_id,
    s.depth,
    t.depth,
    'active'
  from app.node_closure s
  join app.node_closure t
    on t.tenant_id = s.tenant_id
   and t.layer_id = s.layer_id
  where s.tenant_id = new.tenant_id
    and s.layer_id = new.layer_id
    and s.descendant_node_id = new.source_node_id
    and t.descendant_node_id = new.target_node_id
  on conflict (tenant_id, layer_id, edge_id, source_ancestor_node_id, target_ancestor_node_id)
  do update
    set source_node_id = excluded.source_node_id,
        target_node_id = excluded.target_node_id,
        depth_from_source_ancestor = excluded.depth_from_source_ancestor,
        depth_from_target_ancestor = excluded.depth_from_target_ancestor,
        virtual_edge_status = 'active';

  insert into app.virtual_connection_summary (
    tenant_id,
    layer_id,
    source_ancestor_node_id,
    target_ancestor_node_id,
    edge_count,
    direct_edge_count,
    updated_at
  )
  select
    new.tenant_id,
    new.layer_id,
    s.ancestor_node_id,
    t.ancestor_node_id,
    1,
    case
      when s.ancestor_node_id = new.source_node_id
       and t.ancestor_node_id = new.target_node_id
      then 1
      else 0
    end,
    now()
  from app.node_closure s
  join app.node_closure t
    on t.tenant_id = s.tenant_id
   and t.layer_id = s.layer_id
  where s.tenant_id = new.tenant_id
    and s.layer_id = new.layer_id
    and s.descendant_node_id = new.source_node_id
    and t.descendant_node_id = new.target_node_id
  on conflict (tenant_id, layer_id, source_ancestor_node_id, target_ancestor_node_id)
  do update
    set edge_count = app.virtual_connection_summary.edge_count + 1,
        direct_edge_count = app.virtual_connection_summary.direct_edge_count + excluded.direct_edge_count,
        updated_at = now();

  return new;
end;
$$;

create or replace function app.before_contain_endpoint_relation_insert_validate()
returns trigger
language plpgsql
as $$
begin
  if new.relation_status <> 'active' then
    return new;
  end if;

  perform app.assert_active_graph_object(new.tenant_id, new.contain_id);
  perform app.assert_object_kind(new.tenant_id, new.contain_id, 'contain');

  perform app.assert_active_graph_object(new.tenant_id, new.source_layer_id);
  perform app.assert_object_kind(new.tenant_id, new.source_layer_id, 'layer');

  perform app.assert_active_graph_object(new.tenant_id, new.target_layer_id);
  perform app.assert_object_kind(new.tenant_id, new.target_layer_id, 'layer');

  perform app.assert_active_graph_object(new.tenant_id, new.source_object_id);
  perform app.assert_active_graph_object(new.tenant_id, new.target_object_id);

  if new.source_layer_id = new.target_layer_id then
    raise exception 'Contain endpoints must be on different layers. tenant_id=%, source_layer_id=%, target_layer_id=%',
      new.tenant_id, new.source_layer_id, new.target_layer_id;
  end if;

  -- TODO: validate source/target layer membership by object kind
  -- TODO: validate rule_contain_endpoint_type

  return new;
end;
$$;

create or replace function app.soft_delete_object(
  p_tenant_id uuid,
  p_object_id uuid,
  p_actor_user_id uuid default null
)
returns uuid
language plpgsql
as $$
declare
  v_operation_id uuid := gen_random_uuid();
  v_now timestamptz := now();
  v_old_data jsonb;
  v_new_data jsonb;
begin
  perform app.assert_active_graph_object(p_tenant_id, p_object_id);

  select to_jsonb(go)
  into v_old_data
  from app.graph_object go
  where go.tenant_id = p_tenant_id
    and go.object_id = p_object_id;

  update app.graph_object
  set object_status = 'deleted',
      updated_by = p_actor_user_id,
      deleted_at = v_now,
      deleted_by = p_actor_user_id,
      delete_operation_id = v_operation_id
  where tenant_id = p_tenant_id
    and object_id = p_object_id
    and object_status = 'active';

  select to_jsonb(go)
  into v_new_data
  from app.graph_object go
  where go.tenant_id = p_tenant_id
    and go.object_id = p_object_id;

  update app.net_layer_relation
  set relation_status = 'deleted',
      deleted_at = v_now,
      deleted_by = p_actor_user_id,
      delete_operation_id = v_operation_id
  where tenant_id = p_tenant_id
    and relation_status = 'active'
    and (net_id = p_object_id or layer_id = p_object_id);

  update app.layer_tree_relation
  set relation_status = 'deleted',
      deleted_at = v_now,
      deleted_by = p_actor_user_id,
      delete_operation_id = v_operation_id
  where tenant_id = p_tenant_id
    and relation_status = 'active'
    and (
      net_id = p_object_id
      or parent_layer_id = p_object_id
      or child_layer_id = p_object_id
    );

  update app.layer_node_relation
  set relation_status = 'deleted',
      deleted_at = v_now,
      deleted_by = p_actor_user_id,
      delete_operation_id = v_operation_id
  where tenant_id = p_tenant_id
    and relation_status = 'active'
    and (layer_id = p_object_id or node_id = p_object_id);

  update app.node_tree_relation
  set relation_status = 'deleted',
      deleted_at = v_now,
      deleted_by = p_actor_user_id,
      delete_operation_id = v_operation_id
  where tenant_id = p_tenant_id
    and relation_status = 'active'
    and (
      layer_id = p_object_id
      or parent_node_id = p_object_id
      or child_node_id = p_object_id
    );

  update app.edge_endpoint_relation
  set relation_status = 'deleted',
      deleted_at = v_now,
      deleted_by = p_actor_user_id,
      delete_operation_id = v_operation_id
  where tenant_id = p_tenant_id
    and relation_status = 'active'
    and (
      layer_id = p_object_id
      or edge_id = p_object_id
      or source_node_id = p_object_id
      or target_node_id = p_object_id
    );

  update app.contain_endpoint_relation
  set relation_status = 'deleted',
      deleted_at = v_now,
      deleted_by = p_actor_user_id,
      delete_operation_id = v_operation_id
  where tenant_id = p_tenant_id
    and relation_status = 'active'
    and (
      contain_id = p_object_id
      or source_layer_id = p_object_id
      or source_object_id = p_object_id
      or target_layer_id = p_object_id
      or target_object_id = p_object_id
    );

  update app.virtual_edge_index
  set virtual_edge_status = 'deleted'
  where tenant_id = p_tenant_id
    and edge_id = p_object_id
    and virtual_edge_status = 'active';

  return v_operation_id;
end;
$$;

create trigger trg_tenant_touch_updated_at
before update on app.tenant
for each row
execute function app.touch_updated_at();

create trigger trg_object_category_touch_updated_at
before update on app.object_category
for each row
execute function app.touch_updated_at();

create trigger trg_object_type_touch_updated_at
before update on app.object_type
for each row
execute function app.touch_updated_at();

create trigger trg_graph_object_touch_updated_at
before update on app.graph_object
for each row
execute function app.touch_updated_at();

create trigger trg_auth_user_touch_updated_at
before update on app.auth_user
for each row
execute function app.touch_updated_at();

create trigger trg_external_identity_touch_updated_at
before update on app.external_identity
for each row
execute function app.touch_updated_at();

create trigger trg_auth_group_touch_updated_at
before update on app.auth_group
for each row
execute function app.touch_updated_at();

create trigger trg_auth_permission_touch_updated_at
before update on app.auth_permission
for each row
execute function app.touch_updated_at();

create trigger trg_user_group_membership_touch_updated_at
before update on app.user_group_membership
for each row
execute function app.touch_updated_at();

create trigger trg_group_permission_touch_updated_at
before update on app.group_permission
for each row
execute function app.touch_updated_at();

create trigger trg_net_layer_relation_before_insert
before insert on app.net_layer_relation
for each row
execute function app.before_net_layer_relation_insert_validate();

create trigger trg_layer_tree_relation_before_insert
before insert on app.layer_tree_relation
for each row
execute function app.before_layer_tree_relation_insert_validate();

create trigger trg_layer_node_relation_before_insert
before insert on app.layer_node_relation
for each row
execute function app.before_layer_node_relation_insert_validate();

create trigger trg_layer_node_relation_after_insert
after insert on app.layer_node_relation
for each row
execute function app.after_layer_node_relation_insert();

create trigger trg_node_tree_relation_before_insert
before insert on app.node_tree_relation
for each row
execute function app.before_node_tree_relation_insert_validate();

create trigger trg_node_tree_relation_after_insert
after insert on app.node_tree_relation
for each row
execute function app.after_node_tree_relation_insert_update_closure();

create trigger trg_edge_endpoint_relation_before_insert
before insert on app.edge_endpoint_relation
for each row
execute function app.before_edge_endpoint_relation_insert_validate();

create trigger trg_edge_endpoint_relation_after_insert
after insert on app.edge_endpoint_relation
for each row
execute function app.after_edge_endpoint_relation_insert_virtual_index();

create trigger trg_contain_endpoint_relation_before_insert
before insert on app.contain_endpoint_relation
for each row
execute function app.before_contain_endpoint_relation_insert_validate();
