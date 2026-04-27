# Implementation Backlog

---

## Sprint 1: Foundation

### TASK-001: .NET Solution Scaffolding
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Create the .NET 9 solution with 4 Clean Architecture projects and wire up DI.

**Acceptance Criteria**:
- [ ] Solution file `FitPlan.sln` with 4 projects: `FitPlan.Api`, `FitPlan.Application`, `FitPlan.Domain`, `FitPlan.Infrastructure`
- [ ] Project references wired correctly (Api → Application → Domain; Infrastructure → Application)
- [ ] `FitPlan.Api` runs via `dotnet run` and returns 200 on `GET /health`
- [ ] Serilog configured for structured console logging
- [ ] `.editorconfig` + `.gitignore` committed

**Blocked by**: None
**Blocks**: TASK-002, TASK-003, TASK-004

---

### TASK-002: MongoDB Infrastructure
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Configure MongoDB driver, connection, and base repository pattern in the Infrastructure layer.

**Acceptance Criteria**:
- [ ] `IMongoContext` interface in Application layer
- [ ] `MongoContext` implementation in Infrastructure (injects `IConfiguration` for connection string)
- [ ] `IRepository<T>` generic interface with FindById, FindAll, Insert, Update, Delete
- [ ] `MongoRepository<T>` base implementation
- [ ] MongoDB collection name convention established (e.g. `[CollectionName]` attribute or naming convention)
- [ ] Connection verified in integration test

**Blocked by**: TASK-001
**Blocks**: TASK-004, TASK-005

---

### TASK-003: Docker Dev Environment
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Create Docker configuration for local development with MongoDB.

**Acceptance Criteria**:
- [ ] `docker-compose.dev.yml` with .NET API + MongoDB containers
- [ ] `Dockerfile` (multi-stage: build + runtime)
- [ ] `.dockerignore` configured
- [ ] `.env.example` with all required env vars (no real secrets)
- [ ] `docker-compose up` starts the API and MongoDB successfully
- [ ] API can connect to MongoDB inside Docker network

**Blocked by**: TASK-001
**Blocks**: None

---

### TASK-004: JWT Service
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Implement JWT access token and refresh token generation and validation.

**Acceptance Criteria**:
- [ ] `IJwtService` interface in Application with `GenerateAccessToken(userId, userType, trainerId?)` and `ValidateAccessToken(token)`
- [ ] `JwtService` implementation in Infrastructure (HS256, configurable secret + TTL)
- [ ] Access token TTL: 15 minutes; claims include `userId`, `userType`, `trainerId`
- [ ] `IRefreshTokenService` interface + implementation (generate, store in MongoDB, rotate on use, revoke on logout)
- [ ] Refresh token TTL: 7 days with MongoDB TTL index
- [ ] Unit tests for token generation and validation

**Blocked by**: TASK-002
**Blocks**: TASK-005

---

### TASK-005: Trainer Auth — Register + Email Confirmation
**Priority**: P0 | **Estimate**: 3h | **Status**: Done

**Description**:
Implement trainer registration with email confirmation via CQRS handlers.

**Acceptance Criteria**:
- [ ] `Trainer` domain entity in Domain layer (id, email, passwordHash, emailConfirmed, name)
- [ ] `RegisterTrainerCommand` + handler in Application (BCrypt hash, create trainer, send confirmation email)
- [ ] `ConfirmEmailCommand` + handler (validate token, mark emailConfirmed = true)
- [ ] `ITrainerRepository` interface + `TrainerRepository` MongoDB implementation
- [ ] `IEmailService` interface + stub implementation (logs to console in dev)
- [ ] `POST /api/auth/register` and `POST /api/auth/confirm-email` controllers
- [ ] Integration test: register → confirm → confirmed in DB

**Blocked by**: TASK-002, TASK-004
**Blocks**: TASK-006

---

### TASK-006: Trainer Auth — Login + Refresh + Logout
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Implement trainer login (email/password), token refresh, and logout.

**Acceptance Criteria**:
- [ ] `LoginTrainerQuery` + handler (validate email/password/emailConfirmed, return JWT)
- [ ] `RefreshTokenCommand` + handler (validate refresh token, rotate, return new access token)
- [ ] `LogoutCommand` + handler (revoke refresh token)
- [ ] `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/logout` endpoints
- [ ] Refresh token set as HttpOnly Secure `SameSite=Strict` cookie
- [ ] Integration test: login with unconfirmed email returns 401

**Blocked by**: TASK-005
**Blocks**: TASK-007, TASK-009

---

### TASK-007: Password Reset Flow
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Implement forgot password and reset password flows.

**Acceptance Criteria**:
- [ ] `ForgotPasswordCommand` + handler (generate token, send email; always returns 200)
- [ ] `ResetPasswordCommand` + handler (validate token, hash new password, invalidate token)
- [ ] `POST /api/auth/forgot-password` and `POST /api/auth/reset-password` endpoints
- [ ] Token stored in MongoDB with 1h TTL index
- [ ] Integration test: expired token returns 400

**Blocked by**: TASK-005
**Blocks**: None

---

## Sprint 2: Tenant Isolation + Client Management

### TASK-008: Tenant Isolation Behavior
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Implement `ICurrentTrainerAccessor` and a MediatR pipeline behavior that enforces trainer context on all commands/queries.

**Acceptance Criteria**:
- [ ] `ICurrentTrainerAccessor` interface in Application (gets `trainerId` from JWT claims)
- [ ] `CurrentTrainerAccessor` implementation in Infrastructure (reads from `IHttpContextAccessor`)
- [ ] `ITenantScopedRequest` marker interface — all trainer-scoped commands/queries implement it
- [ ] MediatR `TenantValidationBehavior<TRequest, TResponse>` — throws 403 if trainerId missing on scoped requests
- [ ] `TrainerRepository` base queries automatically append `trainerId` filter
- [ ] Integration test: request with another trainer's token cannot access first trainer's data

**Blocked by**: TASK-006
**Blocks**: TASK-009, TASK-010

---

### TASK-009: Client Management — Backend
**Priority**: P0 | **Estimate**: 3h | **Status**: Done

**Description**:
Implement client CRUD with tenant isolation via CQRS.

**Acceptance Criteria**:
- [ ] `Client` domain entity in Domain layer
- [ ] `AddClientCommand` + handler (create client, send invitation email, generate temp password)
- [ ] `GetClientsQuery` + handler (list trainer's clients only)
- [ ] `GetClientQuery` + handler (get by id, enforce trainerId match)
- [ ] `RemoveClientCommand` + handler
- [ ] `IClientRepository` + `ClientRepository` MongoDB implementation
- [ ] `POST /api/clients`, `GET /api/clients`, `GET /api/clients/{id}`, `DELETE /api/clients/{id}` endpoints
- [ ] Integration tests: trainer B cannot GET trainer A's client

**Blocked by**: TASK-008
**Blocks**: TASK-010, TASK-011

---

### TASK-010: Client Auth (Portal Login)
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Implement client login so clients can access the portal.

**Acceptance Criteria**:
- [ ] `LoginClientQuery` + handler (validates against Client collection, returns JWT with `userType=CLIENT` and `trainerId`)
- [ ] `POST /api/auth/login` extended to handle `userType: CLIENT`
- [ ] Client JWT contains `trainerId` for scoping portal queries
- [ ] Integration test: client cannot access `/api/clients` (trainer-only route)

**Blocked by**: TASK-009
**Blocks**: TASK-015

---

## Sprint 3: Workout Session — Backend

### TASK-011: WorkoutSession Domain
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Define the WorkoutSession aggregate and Exercise value object in the Domain layer.

**Acceptance Criteria**:
- [ ] `WorkoutSession` aggregate root with status lifecycle methods: `Start()`, `LogExerciseActuals(exerciseId, actuals)`, `Complete()`
- [ ] `Exercise` value object with planned fields (sets, reps, targetWeight) and actual fields + `UnexpectedProgress` flag
- [ ] Domain rule: `Start()` throws if status ≠ PLANNED
- [ ] Domain rule: `Complete()` throws if status ≠ IN_PROGRESS
- [ ] Domain rule: `UnexpectedProgress = true` when any actual value > planned value
- [ ] Unit tests for domain rules (status transitions, unexpected progress logic)

**Blocked by**: TASK-001
**Blocks**: TASK-012

---

### TASK-012: Session Scheduling — Backend
**Priority**: P0 | **Estimate**: 3h | **Status**: Done

**Description**:
Implement session CRUD commands/queries and API endpoints.

**Acceptance Criteria**:
- [ ] `CreateSessionCommand` + handler (create PLANNED session with exercises)
- [ ] `UpdateSessionCommand` + handler (update time + exercises, only if PLANNED)
- [ ] `DeleteSessionCommand` + handler (soft-check: return error if IN_PROGRESS)
- [ ] `GetSessionsQuery` + handler (filter by clientId + date range + optional status)
- [ ] `GetSessionQuery` + handler (get by id with tenant check)
- [ ] `ISessionRepository` + `SessionRepository` MongoDB implementation (indexes: clientId+scheduledAt, trainerId+scheduledAt)
- [ ] All session endpoints from OpenAPI spec implemented
- [ ] Integration tests for CRUD + tenant isolation

**Blocked by**: TASK-011, TASK-009
**Blocks**: TASK-013

---

### TASK-013: Live Session — Backend
**Priority**: P0 | **Estimate**: 3h | **Status**: Done

**Description**:
Implement start session, log exercise actuals, and complete session commands.

**Acceptance Criteria**:
- [ ] `StartSessionCommand` + handler (PLANNED → IN_PROGRESS, set `startedAt`)
- [ ] `LogExerciseActualsCommand` + handler (update actual values, compute `unexpectedProgress`, session must be IN_PROGRESS)
- [ ] `CompleteSessionCommand` + handler (IN_PROGRESS → COMPLETED, set `completedAt`)
- [ ] `POST /api/clients/{clientId}/sessions/{sessionId}/start`
- [ ] `PATCH /api/clients/{clientId}/sessions/{sessionId}/exercises/{exerciseId}`
- [ ] `POST /api/clients/{clientId}/sessions/{sessionId}/complete`
- [ ] Integration tests: status transition rules enforced; unexpected progress flag set correctly

**Blocked by**: TASK-012
**Blocks**: TASK-014

---

### TASK-014: Client Portal — Backend
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Implement client portal queries — list completed sessions and get session detail.

**Acceptance Criteria**:
- [ ] `GetClientPortalSessionsQuery` + handler (list COMPLETED sessions for logged-in client, paginated, newest first)
- [ ] `GetClientPortalSessionDetailQuery` + handler (get session by id; enforce clientId = current client from JWT)
- [ ] `GET /api/portal/sessions` and `GET /api/portal/sessions/{sessionId}` endpoints (requires `userType=CLIENT` JWT)
- [ ] Integration test: client A cannot access client B's portal session

**Blocked by**: TASK-013, TASK-010
**Blocks**: None (backend complete)

---

## Sprint 4: Google OAuth + Email Service

### TASK-015: SMTP Email Service
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Implement real SMTP email service (replaces dev console stub).

**Acceptance Criteria**:
- [ ] `SmtpEmailService` implementation in Infrastructure using `MailKit`
- [ ] HTML email templates for: email confirmation, client invitation, password reset
- [ ] SMTP config via env vars (`SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASS`, `SMTP_FROM`)
- [ ] Mailtrap integration tested in dev environment

**Blocked by**: TASK-005
**Blocks**: None

---

### TASK-016: Google OAuth2
**Priority**: P1 | **Estimate**: 3h | **Status**: Done

**Description**:
Implement Google OAuth2 Authorization Code flow — backend-handled.

**Acceptance Criteria**:
- [ ] `GET /api/auth/google` redirects to Google consent screen with correct scopes (email, profile)
- [ ] `GET /api/auth/google/callback` exchanges code, fetches Google profile, creates or links trainer account
- [ ] If Google email matches existing trainer account → link `googleId`, return JWT
- [ ] If new email → create trainer account (emailConfirmed = true, passwordHash = null)
- [ ] State parameter used for CSRF protection on callback
- [ ] Integration tested with mock Google token endpoint

**Blocked by**: TASK-006
**Blocks**: None

---

## Sprint 5: Frontend Foundation

### TASK-017: Vue 3 Project Setup
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Scaffold Vue 3 + Vite + TypeScript + Pinia + Vue Router + Tailwind CSS project.

**Acceptance Criteria**:
- [ ] `pnpm create vue@latest` with TypeScript, Vue Router, Pinia, ESLint, Prettier
- [ ] Tailwind CSS v3 configured with PostCSS
- [ ] Vite proxy config: `/api` → `http://localhost:5000`
- [ ] `tsconfig.json` strict mode enabled
- [ ] ESLint + Prettier + Husky + lint-staged configured
- [ ] `pnpm dev` starts with no errors; `pnpm build` produces dist

**Blocked by**: TASK-001
**Blocks**: TASK-018, TASK-019

---

### TASK-018: FitPlan Dark Theme
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Configure Tailwind with FitPlan design tokens and load custom fonts.

**Acceptance Criteria**:
- [ ] `tailwind.config.js` extends colors with FitPlan token palette (bg-base, bg-surface, primary lime, accent orange, etc.)
- [ ] `Bebas Neue` + `Inter` + `JetBrains Mono` loaded via Google Fonts or local files
- [ ] `index.css` sets `body` background to `bg-base`, default text to `text-primary`
- [ ] Dark mode class set to `class` strategy (always dark, no toggle needed for v1)
- [ ] Storybook or dev page showing color swatches and typography scale

**Blocked by**: TASK-017
**Blocks**: TASK-019

---

### TASK-019: API Client + Auth Store
**Priority**: P0 | **Estimate**: 3h | **Status**: Done

**Description**:
Implement Axios API client with JWT refresh interceptor and Pinia auth store.

**Acceptance Criteria**:
- [ ] `src/api/client.ts`: Axios instance with base URL, request interceptor adds `Authorization: Bearer <accessToken>`
- [ ] Response interceptor: on 401 → call `/api/auth/refresh` (cookie-based) → retry original request → if refresh fails, redirect to login
- [ ] `useAuthStore` (Pinia): `accessToken`, `userType`, `isAuthenticated`; actions: `login`, `logout`, `refreshToken`
- [ ] Access token stored in Pinia memory only (never localStorage)
- [ ] On app mount: attempt silent refresh via `/api/auth/refresh` to restore session
- [ ] Unit tests for retry interceptor logic

**Blocked by**: TASK-018
**Blocks**: TASK-020

---

### TASK-020: Auth Pages + Route Guards
**Priority**: P0 | **Estimate**: 3h | **Status**: Done

**Description**:
Build login/register pages and Vue Router navigation guards.

**Acceptance Criteria**:
- [ ] `LoginPage.vue`: email/password form + Google OAuth button (redirect to `/api/auth/google`)
- [ ] `RegisterPage.vue`: name + email + password form (trainer only)
- [ ] `EmailConfirmationPage.vue`: token extracted from URL query param, auto-submits
- [ ] Navigation guard: unauthenticated → redirect to `/login`; authenticated on `/login` → redirect to `/dashboard`
- [ ] Role guard: `userType=CLIENT` → redirect to `/portal`; `userType=TRAINER` → redirect to `/dashboard`
- [ ] Form validation (email format, password min 8 chars) with inline error messages

**Blocked by**: TASK-019
**Blocks**: TASK-021

---

### TASK-021: AppShell Layout
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Build the main application shell with sidebar nav (desktop) and bottom nav (mobile).

**Acceptance Criteria**:
- [ ] `AppShell.vue`: sidebar on desktop (≥768px), bottom tab bar on mobile
- [ ] Trainer nav items: Clients, Dashboard
- [ ] Logout action in nav
- [ ] Active route highlighted in nav
- [ ] Main content area with proper padding and scroll
- [ ] Smooth transition between routes

**Blocked by**: TASK-020
**Blocks**: TASK-022, TASK-026

---

## Sprint 6: Trainer UI

### TASK-022: Client List Page
**Priority**: P0 | **Estimate**: 3h | **Status**: Done

**Description**:
Build the trainer's client list page with add client functionality.

**Acceptance Criteria**:
- [ ] `ClientsPage.vue`: fetches and renders client list with `ClientListItem.vue` (avatar initials, name, last session date)
- [ ] "Add Client" button opens `AddClientModal.vue` (name + email form)
- [ ] On submit: calls `POST /api/clients`; modal closes; list refreshes
- [ ] Empty state component shown when no clients
- [ ] Loading skeleton during fetch
- [ ] Click on client → navigate to `ClientCalendarPage`

**Blocked by**: TASK-021
**Blocks**: TASK-023

---

### TASK-023: Calendar View
**Priority**: P0 | **Estimate**: 4h | **Status**: Done

**Description**:
Build the per-client calendar view for scheduling workouts.

**Acceptance Criteria**:
- [ ] `ClientCalendarPage.vue`: shows monthly calendar grid for the selected client
- [ ] Sessions fetched via `GET /api/clients/{clientId}/sessions?from=&to=` for the visible month range
- [ ] Each session shown as a `SessionCard.vue` on its calendar day (time, exercise count, status badge)
- [ ] Click empty day slot → opens `SessionFormModal.vue` pre-filled with that date
- [ ] Click existing session card → opens same form in edit mode
- [ ] Month navigation (prev/next)
- [ ] Mobile: scrollable week strip instead of full month grid

**Blocked by**: TASK-022
**Blocks**: TASK-024

---

### TASK-024: Session Form (Create/Edit + Exercise Builder)
**Priority**: P0 | **Estimate**: 4h | **Status**: Done

**Description**:
Build the session creation and editing form with an ordered exercise list builder.

**Acceptance Criteria**:
- [ ] `SessionFormModal.vue`: date/time picker + exercise list
- [ ] `ExerciseRow.vue`: name input, sets/reps/weight number inputs (large tap targets), delete button
- [ ] "Add exercise" button appends new empty exercise row
- [ ] Drag-to-reorder exercises (or up/down arrows for mobile simplicity)
- [ ] Weight field shows placeholder "bodyweight" when empty/null
- [ ] Save: `POST /api/clients/{clientId}/sessions` or `PUT .../sessions/{id}`
- [ ] Delete session button with `ConfirmDialog.vue`
- [ ] Form validation: at least 1 exercise required, sets/reps must be positive integers

**Blocked by**: TASK-023
**Blocks**: TASK-025

---

## Sprint 7: Live Session UI

### TASK-025: Live Session Page
**Priority**: P0 | **Estimate**: 4h | **Status**: Done

**Description**:
Build the full-screen mobile-first live session interface.

**Acceptance Criteria**:
- [ ] `LiveSessionPage.vue`: full-screen, dark, no sidebar — just session content
- [ ] Sticky `LiveSessionHeader.vue`: client name, session date, "Finish Session" button
- [ ] "Start Session" button calls `POST .../start`; page enters live mode
- [ ] Each exercise rendered as `ExerciseInputRow.vue`: planned values shown (muted), actual value inputs prominent (large font, JetBrains Mono)
- [ ] Actual inputs: sets, reps, weight — pre-filled with planned values
- [ ] On actual value change → debounced `PATCH .../exercises/{id}` call
- [ ] Works on 375px viewport with no horizontal scroll

**Blocked by**: TASK-024
**Blocks**: TASK-026

---

### TASK-026: Unexpected Progress Indicator + Session Persistence
**Priority**: P0 | **Estimate**: 2h | **Status**: Done

**Description**:
Visual feedback for unexpected progress and session state persistence across navigation.

**Acceptance Criteria**:
- [ ] `UnexpectedProgressBadge.vue`: lime/accent glow border on `ExerciseInputRow` when `unexpectedProgress = true`
- [ ] Badge text: "PR" or lightning bolt icon
- [ ] In-progress session state (sessionId, clientId) stored in Pinia + `sessionStorage`
- [ ] On app load: if in-progress session found in sessionStorage → show "Resume Session" banner
- [ ] "Finish Session" → `ConfirmDialog` → calls `POST .../complete` → redirects to client calendar

**Blocked by**: TASK-025
**Blocks**: None

---

## Sprint 8: Client Portal UI

### TASK-027: Client Portal Pages
**Priority**: P0 | **Estimate**: 3h | **Status**: Done

**Description**:
Build the client-facing workout history portal.

**Acceptance Criteria**:
- [ ] `ClientPortalPage.vue`: paginated list of completed sessions (newest first)
- [ ] `SessionHistoryCard.vue`: date, exercise count, PR badge if any unexpected progress
- [ ] `SessionHistoryDetailPage.vue`: all exercises with `ExerciseResultRow.vue` (planned vs actual columns)
- [ ] Unexpected progress exercises highlighted with lime border/badge
- [ ] Empty state: motivational message when no sessions
- [ ] `ClientPortalLayout.vue`: minimal nav (just logo + logout) — no trainer sidebar
- [ ] Loading skeletons for list and detail

**Blocked by**: TASK-020
**Blocks**: None

---

## Sprint 9: Integration + Quality

### TASK-028: Global Error Handling
**Priority**: P1 | **Estimate**: 2h | **Status**: Done

**Description**:
Implement consistent error handling across frontend and backend.

**Acceptance Criteria**:
- [ ] Backend: `GlobalExceptionMiddleware` in API layer returns consistent `{ code, message }` JSON for all unhandled exceptions
- [ ] Backend: `ValidationBehavior<TRequest, TResponse>` (MediatR pipeline) returns 400 with FluentValidation errors
- [ ] Frontend: `ToastService` (Pinia store) with success/error/warning toast component
- [ ] Axios interceptor catches non-401 errors → triggers error toast with API message
- [ ] 404 page for unknown routes

**Blocked by**: TASK-021
**Blocks**: None

---

### TASK-029: Loading + Empty States
**Priority**: P1 | **Estimate**: 2h | **Status**: Done

**Description**:
Add loading skeletons and empty state components across all list views.

**Acceptance Criteria**:
- [ ] `SkeletonCard.vue` and `SkeletonList.vue` reusable components
- [ ] `EmptyState.vue` with icon, message, optional CTA button
- [ ] Applied to: client list, calendar (no sessions month), portal history list
- [ ] Loading state triggers during all API fetch calls

**Blocked by**: TASK-022, TASK-027
**Blocks**: None

---

### TASK-030: E2E Tests (Critical Paths)
**Priority**: P1 | **Estimate**: 3h | **Status**: Done

**Description**:
Playwright E2E tests covering the two critical user flows.

**Acceptance Criteria**:
- [ ] Playwright configured with `playwright.config.ts` (baseURL, 2 projects: chromium + mobile safari viewport)
- [ ] E2E test: Trainer registers → confirms email → adds client → schedules session → starts live session → logs progress → completes session
- [ ] E2E test: Client logs in → views completed session detail → sees unexpected progress highlighted
- [ ] Tests run against local `docker-compose.dev.yml` environment
- [ ] `pnpm test:e2e` script in `package.json`

**Blocked by**: TASK-026, TASK-027
**Blocks**: None

---

### TASK-031: GitHub Actions CI Pipeline
**Priority**: P1 | **Estimate**: 2h | **Status**: Done

**Description**:
Set up CI pipeline that runs on every push and PR.

**Acceptance Criteria**:
- [ ] `.github/workflows/ci.yml`: triggers on push to `main` and all PRs
- [ ] Jobs: (1) .NET build + unit tests, (2) .NET integration tests with MongoDB service container, (3) Vue type-check + lint + unit tests, (4) Docker build validation
- [ ] All jobs must pass before merge
- [ ] Caches: NuGet packages, pnpm store

**Blocked by**: TASK-003, TASK-017
**Blocks**: None

---

## Task Summary

| Sprint | Tasks | Total Estimate |
|--------|-------|----------------|
| Sprint 1: Foundation | TASK-001 to TASK-007 | 16h |
| Sprint 2: Tenant + Clients | TASK-008 to TASK-010 | 7h |
| Sprint 3: Session Backend | TASK-011 to TASK-014 | 10h |
| Sprint 4: OAuth + Email | TASK-015 to TASK-016 | 5h |
| Sprint 5: Frontend Foundation | TASK-017 to TASK-021 | 12h |
| Sprint 6: Trainer UI | TASK-022 to TASK-024 | 11h |
| Sprint 7: Live Session UI | TASK-025 to TASK-026 | 6h |
| Sprint 8: Client Portal UI | TASK-027 | 3h |
| Sprint 9: Quality | TASK-028 to TASK-031 | 9h |
| **Total** | **31 tasks** | **~79h** |
