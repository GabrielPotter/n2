# Logging

## Overview

The project uses a Docker-based logging stack built from:

- `grafana`
- `loki`
- `grafana-alloy`

Log flow:

`container stdout/stderr -> grafana-alloy -> loki -> grafana`

In the current setup:

- .NET services write structured single-line JSON logs to stdout/stderr
- Keycloak writes its container logs to stdout/stderr
- Grafana Alloy collects logs from Docker containers with `logging: "enabled"`
- Loki stores the logs
- Grafana is the UI for searching and viewing them

## Services

Relevant Docker Compose services:

- `grafana`
- `loki`
- `grafana-alloy`

Persistent volumes:

- `platform-grafana-data`
- `platform-loki-data`

## Grafana Access

- URL: `http://localhost:3000`
- user: `admin`
- password: `admin`

## Start Logging Stack

Start only the logging stack:

```bash
docker compose up -d loki grafana grafana-alloy
```

Check status:

```bash
docker compose ps loki grafana grafana-alloy
```

## Stop Logging Stack

```bash
docker compose stop loki grafana grafana-alloy
```

## Remove Containers Without Deleting Data

```bash
docker compose rm -sf loki grafana grafana-alloy
```

This removes the containers but keeps the Loki and Grafana volumes.

## Full Logging Data Reset And Restart

Use this when you want to delete all persisted Grafana and Loki data and start from a clean state:

```bash
docker compose rm -sf loki grafana grafana-alloy
docker volume rm n2_platform-loki-data n2_platform-grafana-data
docker compose up -d loki grafana grafana-alloy
docker compose ps loki grafana grafana-alloy
```

This deletes:

- Loki stored logs
- Grafana persisted state

This does not delete:

- application PostgreSQL data
- Keycloak PostgreSQL data

## Common Loki Queries

```logql
{service="gateway"}
{service="keycloak"}
{environment="Development"}
{service="core-editor"} | json
{environment="Development"} | json | correlationId="some-correlation-id"
```

## Labels

The current logging setup forwards these low-cardinality labels:

- `service`
- `environment`
- `container`
- `runtime`
- `host`

## Notes

- Keycloak logs are collected from the Keycloak container stdout/stderr stream
- there is no separate direct Loki sink in application code
- Grafana Alloy performs Docker discovery and forwards matching container logs to Loki
