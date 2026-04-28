# Implementation Plan

## Overview

**Project**: FitPlan — Multi-tenant personal trainer platform
**Pattern**: Modular Monolith (.NET Clean Architecture) + SPA (Vue 3)
**Stack**: .NET 9 / ASP.NET Core, MediatR, MongoDB, Vue 3 + Vite + Pinia + Tailwind CSS

---

## Phases

### Phase 1: Foundation (Sprint 1)
**Goal**: Working .NET solution with MongoDB, JWT auth (email/password), Docker

The highest-risk items come first. Auth and MongoDB infrastructure must be solid before any feature work begins.

- .NET solution with 4-project Clean Architecture structure
- MongoDB driver + base repository pattern
- JWT service (access token + refresh token rotation)
- Trainer registration, email confirmation, login
- Docker dev environment

### Phase 2: Core Backend — Client + Session (Sprints 2–4)
**Goal**: All REST API endpoints working and tested

- Tenant isolation enforced via `ICurrentTrainerAccessor` pipeline behavior
- Client management (add, list, get)
- Workout session scheduling (CRUD + calendar range queries)
- Live session mode (start → log actuals → complete)
- Client portal queries
- Google OAuth2 + email service (SMTP) + password reset

### Phase 3: Frontend Foundation (Sprint 5–6)
**Goal**: Vue 3 SPA with auth, routing, API client, dark-mode design system

- Vue 3 + Vite + TypeScript + Pinia + Tailwind setup
- FitPlan dark-mode theme (Bebas Neue, electric lime tokens)
- Auth pages (login, register) + Pinia auth store
- JWT refresh token rotation (silent refresh on 401)
- Protected route guards + AppShell layout

### Phase 4: Trainer UI (Sprints 7–8)
**Goal**: Full trainer workflow functional end-to-end

- Client list + add client modal
- Calendar view (monthly/weekly, click-to-create sessions)
- Session create/edit form with exercise builder
- Live session page (mobile-first, actual value inputs, progress flags)

### Phase 5: Client Portal + Polish (Sprint 9–10)
**Goal**: Client-facing portal + production-ready quality

- Client portal: session history list + detail (planned vs actual)
- Global error handling, loading/empty states
- E2E tests (Playwright) covering critical flows
- GitHub Actions CI pipeline

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Google OAuth backend flow complexity | High | Implement after email auth is solid; use `AspNet.Security.OAuth.Google` package |
| MongoDB tenant isolation (missing trainerId filter) | High | Enforce via base repository — all queries require trainerId parameter; add integration test coverage |
| Calendar UI complexity (click-to-create + drag) | Medium | Use existing Vue calendar library (vue-cal or FullCalendar) rather than building from scratch |
| JWT refresh rotation on mobile/SPA | Medium | Implement silent refresh (on-load + 401 interceptor) before any protected pages |
| Live session state lost on navigation | Medium | Pinia + sessionStorage persistence for in-progress session state |

## Dependencies

External dependencies:
- [ ] Google Cloud Console project (OAuth2 client ID + secret)
- [ ] SMTP credentials (e.g. Mailtrap for dev, any SMTP relay for prod)
- [ ] VPS with Docker + Caddy configured
- [ ] Domain name + SSL certificate (Caddy handles Let's Encrypt automatically)
