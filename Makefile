.PHONY: dev dev-build dev-down logs \
        test-dotnet test-dotnet-unit test-dotnet-integration test-dotnet-watch \
        test-frontend test-frontend-watch test-frontend-coverage \
        test-e2e lint format typecheck \
        db-shell db-logs \
        build prod-up prod-down prod-logs prod-restart \
        clean help

# =============================================================
# Development
# =============================================================

dev: ## Start full dev environment (API + Frontend + MongoDB + MailHog)
	docker compose up

dev-build: ## Rebuild all images and start
	docker compose up --build

dev-down: ## Stop all containers
	docker compose down

dev-down-volumes: ## Stop all containers and remove volumes (wipes DB)
	docker compose down -v

logs: ## Tail all container logs
	docker compose logs -f

logs-api: ## Tail API logs only
	docker compose logs -f api

logs-frontend: ## Tail frontend logs only
	docker compose logs -f frontend

# =============================================================
# Testing — .NET
# =============================================================

test-dotnet: ## Run all .NET tests
	cd backend && dotnet test

test-dotnet-unit: ## Run .NET unit tests only
	cd backend && dotnet test tests/FitPlan.Domain.Tests tests/FitPlan.Application.Tests

test-dotnet-integration: ## Run .NET integration tests (requires running MongoDB)
	cd backend && dotnet test tests/FitPlan.Api.IntegrationTests

test-dotnet-watch: ## Run .NET tests in watch mode
	cd backend && dotnet watch test

# =============================================================
# Testing — Frontend
# =============================================================

test-frontend: ## Run frontend unit tests
	cd frontend && pnpm test:unit

test-frontend-watch: ## Run frontend tests in watch mode
	cd frontend && pnpm test:unit:watch

test-frontend-coverage: ## Run frontend tests with coverage report
	cd frontend && pnpm test:unit:coverage

test-e2e: ## Run Playwright E2E tests (requires running app)
	cd frontend && pnpm test:e2e

# =============================================================
# Code Quality
# =============================================================

lint: ## Run all linters
	cd backend && dotnet format --verify-no-changes
	cd frontend && pnpm lint:check

lint-fix: ## Fix lint issues where possible
	cd frontend && pnpm lint

format: ## Format all code
	cd backend && dotnet format
	cd frontend && pnpm format

typecheck: ## Run TypeScript type check
	cd frontend && pnpm typecheck

# =============================================================
# Database
# =============================================================

db-shell: ## Open MongoDB shell (requires running MongoDB container)
	docker compose exec mongodb mongosh -u root -p $${MONGO_PASSWORD:-mongopassword} fitplan_dev

db-logs: ## Tail MongoDB logs
	docker compose logs -f mongodb

# =============================================================
# Email Dev
# =============================================================

mailhog: ## Open MailHog web UI in browser
	open http://localhost:8025

# =============================================================
# Production
# =============================================================

build: ## Build production Docker images
	docker build -t fitplan-api:latest -f Dockerfile .
	docker build -t fitplan-frontend:latest -f Dockerfile.frontend .

prod-up: ## Start production stack (requires .env with production values)
	docker compose -f docker-compose.prod.yml up -d

prod-down: ## Stop production stack
	docker compose -f docker-compose.prod.yml down

prod-logs: ## Tail production logs
	docker compose -f docker-compose.prod.yml logs -f

prod-restart: ## Restart production stack
	docker compose -f docker-compose.prod.yml restart

# =============================================================
# Cleanup
# =============================================================

clean: ## Remove build artifacts and containers
	docker compose down -v
	rm -rf backend/**/bin backend/**/obj
	rm -rf frontend/node_modules frontend/dist frontend/coverage

# =============================================================
# Help
# =============================================================

help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-25s\033[0m %s\n", $$1, $$2}'

.DEFAULT_GOAL := help
