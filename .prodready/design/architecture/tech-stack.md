# Tech Stack

## Core

| Layer | Technology | Rationale |
|-------|------------|-----------|
| Language (Backend) | C# 12 / .NET 9 | User-specified; latest LTS with strong async support |
| Framework (Backend) | ASP.NET Core 9 (Minimal API + Controllers) | Native to .NET, mature ecosystem |
| CQRS Dispatcher | MediatR 12 | De-facto standard for CQRS in .NET |
| Validation | FluentValidation | Clean validation pipeline, integrates with MediatR behaviors |
| Language (Frontend) | TypeScript 5 | Type safety across Vue 3 components |
| Framework (Frontend) | Vue 3 + Vite 5 | User-specified; Composition API, fast HMR |
| State Management | Pinia | User-specified; official Vue state manager |
| CSS | Tailwind CSS v3 | User-specified; utility-first, no custom CSS files |
| Database | MongoDB 7 | User-specified; flexible document model suits embedded exercises |
| DB Driver | MongoDB.Driver (official C# driver) | No ORM — repository pattern in Infrastructure layer |

## Infrastructure

| Component | Technology | Rationale |
|-----------|------------|-----------|
| Container | Docker + Docker Compose | User-specified; portability, consistent environments |
| Reverse Proxy | Caddy | Auto HTTPS via Let's Encrypt, simpler config than Nginx |
| CI/CD | GitHub Actions | Free tier, tight GitHub integration |

## Development

| Tool | Purpose |
|------|---------|
| xUnit | Unit and integration testing (.NET) |
| Moq | Mocking in .NET tests |
| Vitest | Unit testing (Vue 3 / TypeScript) |
| Playwright | E2E tests |
| ESLint + Prettier | Frontend linting and formatting |
| Husky + lint-staged | Git hooks (block commits with lint errors) |

## Versions

```json
{
  "dotnet": "9.0",
  "csharp": "12",
  "mediatr": "12.x",
  "fluentvalidation": "11.x",
  "mongodb-driver": "2.x",
  "node": "20.x (LTS)",
  "typescript": "5.x",
  "vue": "3.x",
  "vite": "5.x",
  "pinia": "2.x",
  "tailwindcss": "3.x",
  "vitest": "1.x",
  "playwright": "1.x"
}
```

## Authentication

- **Strategy**: JWT (access token) + Refresh token rotation
- **Access token TTL**: 15 minutes
- **Refresh token TTL**: 7 days (stored in MongoDB with TTL index)
- **Google OAuth**: OAuth2 Authorization Code flow → exchange for JWT on backend
- **Storage**: Access token in memory (Pinia store); refresh token in HttpOnly cookie
- **Tenant context**: `trainerId` claim embedded in JWT; extracted via `ICurrentTrainerAccessor` in Application layer

## Email

- **Provider**: SMTP (self-hosted or any SMTP relay — configured via env vars)
- **Purpose**: Email confirmation on registration, client invitation

## Monitoring

- **Logging**: Serilog (structured logging to console + file)
- **Error tracking**: None for v1
- **Analytics**: None for v1
