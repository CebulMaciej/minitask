# Architecture Pattern

## Selected Pattern: Modular Monolith (Backend) + SPA (Frontend)

## Rationale
Based on:
- Deployment: Single VPS with Docker
- Scale: ~100 trainers, ~2,500 clients — low-to-moderate load
- Team: Solo developer

Two separately deployed units — a .NET API and a Vue 3 SPA — but the backend is a single deployable service with strict internal layering (Clean Architecture + CQRS). Not microservices: no network hops between domain areas, no distributed tracing overhead, no service mesh.

Selected because:
- Solo dev benefits from a single backend codebase to debug, deploy, and reason about
- Clean Architecture + CQRS enforces separation of concerns without the operational overhead of microservices
- SPA + REST API is a natural boundary that allows the frontend to evolve independently
- VPS Docker deployment is trivial for two containers (API + frontend served via Nginx)

## Structure

```
┌─────────────────────────────────────────────────────────┐
│                        VPS (Docker)                      │
│                                                         │
│  ┌──────────────────┐      ┌──────────────────────────┐ │
│  │   Nginx / Caddy  │      │      MongoDB              │ │
│  │  (reverse proxy) │      │   (self-hosted)           │ │
│  └────────┬─────────┘      └──────────────────────────┘ │
│           │                          ▲                   │
│    ┌──────┴──────┐                   │                   │
│    │             │                   │                   │
│  ┌─▼──────────┐ ┌▼──────────────────┴──┐               │
│  │  Vue 3 SPA │ │    .NET API           │               │
│  │  (Nginx)   │ │  ┌────────────────┐   │               │
│  └────────────┘ │  │  API Layer     │   │               │
│                 │  ├────────────────┤   │               │
│                 │  │  Application   │   │               │
│                 │  ├────────────────┤   │               │
│                 │  │  Domain        │   │               │
│                 │  ├────────────────┤   │               │
│                 │  │ Infrastructure │   │               │
│                 │  └────────────────┘   │               │
│                 └──────────────────────┘               │
└─────────────────────────────────────────────────────────┘
```

## .NET Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|---------------|
| API | `FitPlan.Api` | Controllers, request/response DTOs, middleware, dispatches commands/queries via MediatR |
| Application | `FitPlan.Application` | CQRS handlers, interfaces (IRepository, IEmailService, etc.), validators |
| Domain | `FitPlan.Domain` | Entities, value objects, domain events, business rules |
| Infrastructure | `FitPlan.Infrastructure` | MongoDB repositories, email service, JWT service, Google OAuth |

## Key Decisions
- MediatR for CQRS dispatch in the Application layer
- No direct DB access from API layer — all reads/writes go through Application handlers
- Tenant isolation enforced in Application layer via `ICurrentTrainerAccessor` — no handler executes without a verified trainer context
- Frontend served as static files from a separate Nginx container

## Future Considerations
- If traffic grows significantly, the API can be extracted into separate services per domain (Auth, Sessions, Portal) without changing the domain logic — the Clean Architecture boundary already separates concerns
- MongoDB Atlas migration is straightforward from self-hosted since connection string is the only change
