#!/bin/bash
set -e

echo "FitPlan — Development Setup"
echo "==========================="
echo ""

# Check Docker
if ! command -v docker &> /dev/null; then
    echo "ERROR: Docker not found. Install Docker Desktop or Docker Engine first."
    echo "       https://docs.docker.com/get-docker/"
    exit 1
fi

# Check Docker is running
if ! docker info &> /dev/null; then
    echo "ERROR: Docker daemon is not running. Start Docker and try again."
    exit 1
fi

# Check docker compose
if ! docker compose version &> /dev/null; then
    echo "ERROR: Docker Compose v2 not found. Upgrade Docker Desktop or install the plugin."
    exit 1
fi

echo "[1/3] Configuring environment..."
if [ ! -f .env ]; then
    cp .env.example .env
    echo "      Created .env from .env.example"
    echo "      Review .env and set SMTP / Google OAuth values before running in production."
else
    echo "      .env already exists — skipping."
fi

echo "[2/3] Pulling base images..."
docker compose pull mongodb mailhog 2>&1 | grep -v "^#" | grep "Pull" || true

echo "[3/3] Starting development environment..."
docker compose up -d

echo ""
echo "Done. Services:"
echo ""
echo "  Frontend   http://localhost:5173   (Vite HMR)"
echo "  API        http://localhost:5001"
echo "  MailHog    http://localhost:8025   (catch-all email)"
echo "  MongoDB    localhost:27017"
echo ""
echo "Useful commands:"
echo "  make dev            restart full stack"
echo "  make test-dotnet    run all .NET tests"
echo "  make test-frontend  run Vue unit tests"
echo "  make logs           tail all container logs"
echo "  make help           list all targets"
