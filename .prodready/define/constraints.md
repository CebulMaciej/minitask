# Constraints

## Deployment
- Target: VPS with Docker
- All services self-hosted (API, frontend, MongoDB)

## Scale
- Launch: ~100 trainers, ~2,000 clients, low-to-moderate request volume
- 6 months: Same order of magnitude — single VPS sufficient

## Budget
- Infrastructure: Fixed VPS cost, no managed cloud services
- Tooling: No specific budget constraints

## Compliance & Security
- Authentication: Google OAuth2 + email/password with email confirmation
- Sessions managed via JWT
- Multi-tenant data isolation (trainer data must never leak across tenants)
- GDPR: Basic compliance (user data stored on self-hosted VPS, no third-party data processors beyond Google OAuth)

## Tech Stack Preferences
- Language: C# (.NET) — backend; TypeScript — frontend
- Framework: .NET (Clean Architecture, CQRS) — API; Vue 3 + Vite — frontend
- Database: MongoDB (self-hosted)
- ORM: None specified (MongoDB driver / repository pattern via Infrastructure layer)
- State Management: Pinia
- CSS: Tailwind CSS (utility-first classes)
- Auth: Google OAuth2 + custom JWT (email/password + email confirmation)
- Containerization: Docker
