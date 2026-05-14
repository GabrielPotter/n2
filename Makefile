.PHONY: dev build test db-reset db-psql

dev:
	./scripts/dev.sh

build:
	./scripts/build.sh

test:
	./scripts/test.sh

db-reset:
	./scripts/reset-db.sh

db-psql:
	docker compose exec postgres psql -U platform -d platformdb
