# Constitution

## Non-Negotiables
- App must work on mobile (responsive, mobile-first)
- Data must never be lost — training history is critical for tracking client progress
- Each trainer's data must be fully isolated from other trainers (multi-tenant security)

## Explicit Non-Goals (v1)
- No video uploads
- No AI-generated workout suggestions
- No in-app chat between trainer and client
- No wearable / fitness tracker integrations
- No subscription billing (free for now)
- No client feedback / side-effect reporting forms
- No progress charts

## Technical Constraints
- Backend: .NET (Clean Architecture, CQRS pattern)
  - 4 projects: API, Application, Domain, Infrastructure
  - API layer: request models, controllers, dispatches commands/queries
  - Application layer: handlers, interfaces, use cases
  - Domain layer: domain entities and business logic
  - Infrastructure layer: implements Application interfaces (DB, services)
- Frontend: Vue 3 + Vite
- Database: MongoDB
- Authentication: Custom JWT (no Keycloak or third-party auth providers)

## Timeline & Resources
- Timeline: No hard deadline
- Team: Solo developer
- Constraints: None specified
