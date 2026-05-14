\set ON_ERROR_STOP on

-- This file is for local/dev schema recreation only.
-- Do not use it as production seed logic.

do $$
declare
  v_dev_tenant_id uuid := '10000000-0000-0000-0000-000000000001';
  v_ops_tenant_id uuid := '10000000-0000-0000-0000-000000000002';

  v_net_category_id uuid := '20000000-0000-0000-0000-000000000001';
  v_layer_category_id uuid := '20000000-0000-0000-0000-000000000002';
  v_node_category_id uuid := '20000000-0000-0000-0000-000000000003';
  v_edge_category_id uuid := '20000000-0000-0000-0000-000000000004';
  v_contain_category_id uuid := '20000000-0000-0000-0000-000000000005';

  v_net_type_id uuid := '30000000-0000-0000-0000-000000000001';
  v_layer_type_id uuid := '30000000-0000-0000-0000-000000000002';
  v_node_type_id uuid := '30000000-0000-0000-0000-000000000003';
  v_edge_type_id uuid := '30000000-0000-0000-0000-000000000004';
  v_contain_type_id uuid := '30000000-0000-0000-0000-000000000005';

  v_admin_user_id uuid := '40000000-0000-0000-0000-000000000001';
  v_editor_user_id uuid := '40000000-0000-0000-0000-000000000002';
  v_viewer_user_id uuid := '40000000-0000-0000-0000-000000000003';

  v_admin_identity_id uuid := '41000000-0000-0000-0000-000000000001';
  v_editor_identity_id uuid := '41000000-0000-0000-0000-000000000002';
  v_viewer_identity_id uuid := '41000000-0000-0000-0000-000000000003';

  v_dev_admin_group_id uuid := '43000000-0000-0000-0000-000000000001';
  v_dev_editor_group_id uuid := '43000000-0000-0000-0000-000000000002';
  v_dev_viewer_group_id uuid := '43000000-0000-0000-0000-000000000003';

  v_network_read_permission_id uuid := '44000000-0000-0000-0000-000000000001';
  v_network_write_permission_id uuid := '44000000-0000-0000-0000-000000000002';
  v_tenant_admin_permission_id uuid := '44000000-0000-0000-0000-000000000003';
begin
  insert into app.tenant (
    tenant_id,
    code,
    name,
    status
  )
  values
    (v_dev_tenant_id, 'dev-tenant', 'Development Tenant', 'active'),
    (v_ops_tenant_id, 'ops-tenant', 'Operations Tenant', 'active');

  insert into app.object_category (
    category_id,
    tenant_id,
    object_kind,
    name,
    json_schema,
    status
  )
  values
    (v_net_category_id, v_dev_tenant_id, 'net', 'Net Category', '{}'::jsonb, 'active'),
    (v_layer_category_id, v_dev_tenant_id, 'layer', 'Layer Category', '{}'::jsonb, 'active'),
    (v_node_category_id, v_dev_tenant_id, 'node', 'Node Category', '{}'::jsonb, 'active'),
    (v_edge_category_id, v_dev_tenant_id, 'edge', 'Edge Category', '{}'::jsonb, 'active'),
    (v_contain_category_id, v_dev_tenant_id, 'contain', 'Contain Category', '{}'::jsonb, 'active');

  insert into app.object_type (
    type_id,
    tenant_id,
    category_id,
    name,
    json_schema,
    status
  )
  values
    (v_net_type_id, v_dev_tenant_id, v_net_category_id, 'Net Type', '{}'::jsonb, 'active'),
    (v_layer_type_id, v_dev_tenant_id, v_layer_category_id, 'Layer Type', '{}'::jsonb, 'active'),
    (v_node_type_id, v_dev_tenant_id, v_node_category_id, 'Node Type', '{}'::jsonb, 'active'),
    (v_edge_type_id, v_dev_tenant_id, v_edge_category_id, 'Edge Type', '{}'::jsonb, 'active'),
    (v_contain_type_id, v_dev_tenant_id, v_contain_category_id, 'Contain Type', '{}'::jsonb, 'active');

  insert into app.auth_user (
    auth_user_id,
    tenant_id,
    display_name,
    email,
    is_enabled,
    authz_version,
    status
  )
  values
    (v_admin_user_id, v_dev_tenant_id, 'Admin Developer', 'admin@example.com', true, 1, 'active'),
    (v_editor_user_id, v_dev_tenant_id, 'Editor Developer', 'editor@example.com', true, 1, 'active'),
    (v_viewer_user_id, v_dev_tenant_id, 'Viewer Developer', 'viewer@example.com', true, 1, 'active');

  insert into app.external_identity (
    external_identity_id,
    provider,
    issuer,
    external_subject,
    auth_user_id,
    status
  )
  values
    (v_admin_identity_id, 'keycloak', 'http://localhost:8081/realms/n2-users', '51000000-0000-0000-0000-000000000001', v_admin_user_id, 'active'),
    (v_editor_identity_id, 'keycloak', 'http://localhost:8081/realms/n2-users', '51000000-0000-0000-0000-000000000002', v_editor_user_id, 'active'),
    (v_viewer_identity_id, 'keycloak', 'http://localhost:8081/realms/n2-users', '51000000-0000-0000-0000-000000000003', v_viewer_user_id, 'active');

  insert into app.auth_group (
    group_id,
    tenant_id,
    name,
    status
  )
  values
    (v_dev_admin_group_id, v_dev_tenant_id, 'tenant-admin', 'active'),
    (v_dev_editor_group_id, v_dev_tenant_id, 'editor', 'active'),
    (v_dev_viewer_group_id, v_dev_tenant_id, 'viewer', 'active');

  insert into app.auth_permission (
    permission_id,
    code,
    name,
    status
  )
  values
    (v_network_read_permission_id, 'network.read', 'Network Read', 'active'),
    (v_network_write_permission_id, 'network.write', 'Network Write', 'active'),
    (v_tenant_admin_permission_id, 'tenant.admin', 'Tenant Administration', 'active');

  insert into app.user_group_membership (
    user_group_membership_id,
    tenant_id,
    auth_user_id,
    group_id,
    status
  )
  values
    ('45000000-0000-0000-0000-000000000001', v_dev_tenant_id, v_admin_user_id, v_dev_admin_group_id, 'active'),
    ('45000000-0000-0000-0000-000000000003', v_dev_tenant_id, v_editor_user_id, v_dev_editor_group_id, 'active'),
    ('45000000-0000-0000-0000-000000000004', v_dev_tenant_id, v_viewer_user_id, v_dev_viewer_group_id, 'active');

  insert into app.group_permission (
    group_permission_id,
    tenant_id,
    group_id,
    permission_id,
    status
  )
  values
    ('46000000-0000-0000-0000-000000000001', v_dev_tenant_id, v_dev_admin_group_id, v_network_read_permission_id, 'active'),
    ('46000000-0000-0000-0000-000000000002', v_dev_tenant_id, v_dev_admin_group_id, v_network_write_permission_id, 'active'),
    ('46000000-0000-0000-0000-000000000003', v_dev_tenant_id, v_dev_admin_group_id, v_tenant_admin_permission_id, 'active'),
    ('46000000-0000-0000-0000-000000000007', v_dev_tenant_id, v_dev_editor_group_id, v_network_read_permission_id, 'active'),
    ('46000000-0000-0000-0000-000000000008', v_dev_tenant_id, v_dev_editor_group_id, v_network_write_permission_id, 'active'),
    ('46000000-0000-0000-0000-000000000009', v_dev_tenant_id, v_dev_viewer_group_id, v_network_read_permission_id, 'active');
end
$$;
