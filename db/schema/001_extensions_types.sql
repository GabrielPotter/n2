\set ON_ERROR_STOP on

create extension if not exists pgcrypto;

do $$
begin
  if not exists (
    select 1
    from pg_type t
    join pg_namespace n on n.oid = t.typnamespace
    where n.nspname = 'app'
      and t.typname = 'record_status'
  ) then
    create type app.record_status as enum (
      'active',
      'inactive',
      'deleted'
    );
  end if;
end
$$;

do $$
begin
  if not exists (
    select 1
    from pg_type t
    join pg_namespace n on n.oid = t.typnamespace
    where n.nspname = 'app'
      and t.typname = 'object_kind'
  ) then
    create type app.object_kind as enum (
      'net',
      'layer',
      'node',
      'edge',
      'contain'
    );
  end if;
end
$$;
