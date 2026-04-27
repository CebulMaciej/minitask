# FitPlan

Personal trainer platform for scheduling workouts, managing clients, and running live training sessions with real-time progress capture.

## Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 9 / ASP.NET Core, MediatR (CQRS), FluentValidation |
| Frontend | Vue 3 + TypeScript, Vite, Pinia, Tailwind CSS |
| Database | MongoDB 7 |
| Auth | JWT + Refresh tokens, Google OAuth2 |
| Container | Docker + Docker Compose |

## Quick Start

### Prerequisites

- Docker & Docker Compose
- Node.js 20+ with pnpm (local frontend development)
- .NET 9 SDK (local backend development)

### Development

```bash
# Clone and configure
git clone <repo-url>
cd fitplan
cp .env.example .env          # edit values as needed

# Start everything (API on :5001, frontend on :5173, MailHog on :8025)
make dev

# Or rebuild images first
make dev-build
```

| Service | URL |
|---------|-----|
| Frontend (Vite HMR) | http://localhost:5173 |
| API | http://localhost:5001 |
| MailHog (dev email) | http://localhost:8025 |
| MongoDB | localhost:27017 |

### Running Tests

```bash
make test-dotnet             # all .NET tests (unit + integration)
make test-dotnet-unit        # domain + application unit tests
make test-dotnet-integration # integration tests (Testcontainers, no running DB needed)
make test-frontend           # Vue unit tests (Vitest)
make test-e2e                # Playwright E2E (requires running app)
```

### Code Quality

```bash
make lint        # dotnet format --verify-no-changes + ESLint
make lint-fix    # fix ESLint issues
make typecheck   # vue-tsc --noEmit
make format      # dotnet format + Prettier
```

## Environment Variables

Copy `.env.example` to `.env`. All variables with a default can be left as-is for local development.

| Variable | Required (prod) | Description |
|----------|-----------------|-------------|
| `MONGO_PASSWORD` | Yes | MongoDB root password |
| `JWT_SECRET` | Yes | JWT signing secret (min 32 chars) |
| `SMTP_HOST` | Yes | SMTP server hostname |
| `SMTP_PORT` | No (587) | SMTP port |
| `SMTP_USERNAME` | Yes | SMTP auth username |
| `SMTP_PASSWORD` | Yes | SMTP auth password |
| `SMTP_FROM` | Yes | From address for emails |
| `GOOGLE_CLIENT_ID` | No | Google OAuth2 client ID |
| `GOOGLE_CLIENT_SECRET` | No | Google OAuth2 client secret |
| `GOOGLE_REDIRECT_URI` | No | OAuth2 callback URL |
| `FRONTEND_URL` | No | Full URL of the frontend (for OAuth redirect) |
| `PORT` | No (80) | Nginx listen port (production) |

## Architecture

Clean Architecture with CQRS:

```
frontend/        Vue 3 SPA (Vite)
backend/
  src/
    FitPlan.Api            HTTP layer (controllers, middleware)
    FitPlan.Application    Use cases, CQRS handlers, interfaces
    FitPlan.Domain         Entities, value objects, domain logic
    FitPlan.Infrastructure Repositories, services, MongoDB context
  tests/
    FitPlan.Domain.Tests
    FitPlan.Application.Tests
    FitPlan.Api.IntegrationTests  (Testcontainers, real MongoDB)
```

Requests flow: `Controller → MediatR → Handler (Application) → Repository (Infrastructure) → MongoDB`

## API Documentation

OpenAPI spec: `.prodready/design/api/openapi.yaml`

Key endpoints:

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/register` | Trainer registration |
| POST | `/api/auth/login` | Email/password login |
| GET | `/api/auth/google` | Initiate Google OAuth |
| GET | `/api/auth/google/callback` | Google OAuth callback |
| POST | `/api/auth/refresh` | Refresh access token |
| GET | `/api/clients` | List trainer's clients |
| POST | `/api/clients` | Add a client |
| GET | `/api/clients/{id}/sessions` | Client session history |
| POST | `/api/sessions` | Schedule a session |
| PUT | `/api/sessions/{id}/start` | Start a session |
| PUT | `/api/sessions/{id}/complete` | Complete a session |
| GET | `/api/portal/sessions` | Client portal: session list |

## Deployment

See [DEPLOYMENT.md](./DEPLOYMENT.md) for VPS deployment with Docker Compose.
