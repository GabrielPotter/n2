.PHONY: build test db-reset db-psql keycloak-reset loki-reset grafana-reset reset-all

build:
	./scripts/build.sh

test:
	./scripts/test.sh

db-reset:
	./scripts/reset-db.sh

keycloak-reset:
	./scripts/reset-keycloak.sh

loki-reset:
	./scripts/reset-loki.sh

grafana-reset:
	./scripts/reset-grafana.sh

reset-all:
	./scripts/reset-db.sh
	./scripts/reset-keycloak.sh
	./scripts/reset-loki.sh
	./scripts/reset-grafana.sh

db-psql:
	docker compose exec postgres psql -U platform -d platformdb
