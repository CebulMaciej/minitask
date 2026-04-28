# Test Plan

## Testing Strategy

### Test Pyramid

```
          /\
         /  \   E2E (10%) — Playwright, 2 critical flows
        /----\
       /      \  Integration (35%) — xUnit + MongoDB, API endpoint tests
      /--------\
     /          \  Unit (55%) — xUnit (domain), Vitest (Vue components)
    /------------\
```

**Coverage Target**: 80%+ on Application + Domain layers (.NET); 70%+ on Vue components

---

## Unit Tests

### Backend — Domain Layer (xUnit)
| Module | What to Test |
|--------|--------------|
| `WorkoutSession` | Status transition rules (Start, Complete), UnexpectedProgress flag logic |
| `Exercise` | Value object construction, actual vs planned comparison |
| `JwtService` | Token generation, claims, expiry |
| `RefreshTokenService` | Token rotation, revocation |

**Location**: `FitPlan.Domain.Tests/`, `FitPlan.Application.Tests/`
**Framework**: xUnit + Moq

### Frontend — Vue Components (Vitest)
| Module | What to Test |
|--------|--------------|
| `useAuthStore` | login, logout, silent refresh, token state |
| `apiClient` | 401 retry interceptor, refresh rotation |
| `ExerciseInputRow` | Renders planned values, emits actuals on change, progress badge |
| `SessionFormModal` | Validation rules, exercise add/remove/reorder |
| `ClientListItem` | Renders client name, navigates on click |

**Location**: `src/__tests__/`
**Framework**: Vitest + Vue Test Utils

---

## Integration Tests

### Backend API (xUnit + Testcontainers for MongoDB)

Each integration test spins up a real MongoDB container (Testcontainers.MongoDB) and tests the full request → handler → DB → response path.

#### Auth Endpoints
- [ ] `POST /api/auth/register` — success, duplicate email (409), invalid email (400)
- [ ] `POST /api/auth/confirm-email` — valid token, expired token (400), already confirmed (400)
- [ ] `POST /api/auth/login` — success, unconfirmed email (401), wrong password (401)
- [ ] `POST /api/auth/refresh` — valid cookie, missing cookie (401), revoked token (401)
- [ ] `POST /api/auth/logout` — token revoked in DB
- [ ] `POST /api/auth/forgot-password` — always 200 (no email enumeration)
- [ ] `POST /api/auth/reset-password` — valid token, expired token (400)

#### Client Management Endpoints
- [ ] `GET /api/clients` — returns only trainer's clients (not other trainer's)
- [ ] `POST /api/clients` — success, duplicate email within trainer (409)
- [ ] `GET /api/clients/{id}` — success, wrong trainer's client (404)
- [ ] `DELETE /api/clients/{id}` — success, wrong trainer's client (404)

#### Session Endpoints
- [ ] `POST /api/clients/{clientId}/sessions` — success, client not found (404), invalid exercises (400)
- [ ] `GET /api/clients/{clientId}/sessions` — date range filter works, status filter works
- [ ] `PUT /api/clients/{clientId}/sessions/{id}` — updates correctly
- [ ] `DELETE /api/clients/{clientId}/sessions/{id}` — PLANNED deletes; IN_PROGRESS returns 400
- [ ] `POST .../start` — PLANNED → IN_PROGRESS; already IN_PROGRESS → 400
- [ ] `PATCH .../exercises/{exerciseId}` — actual values stored; `unexpectedProgress` flag set when actual > planned
- [ ] `POST .../complete` — IN_PROGRESS → COMPLETED; sets `completedAt`

#### Client Portal Endpoints
- [ ] `GET /api/portal/sessions` — client JWT required; returns only COMPLETED sessions for that client
- [ ] `GET /api/portal/sessions/{id}` — client A cannot access client B's session (403)
- [ ] `GET /api/portal/sessions/{id}` — trainer JWT returns 403 (portal is client-only)

**Location**: `FitPlan.Api.IntegrationTests/`
**Framework**: xUnit + `Microsoft.AspNetCore.Mvc.Testing` + `Testcontainers.MongoDb`

---

## E2E Tests

### Critical Flow 1: Trainer creates and runs a session

```
Trainer registers → confirms email → logs in
→ Adds client "Jane Smith"
→ Navigates to Jane's calendar → clicks a date
→ Creates session with 2 exercises (Bench Press 3×10 @60kg, Pull-ups 3×8 bodyweight)
→ Clicks "Start Session"
→ Logs actual weight 70kg for Bench Press (unexpected progress)
→ Completes session
→ Verifies session status = COMPLETED on calendar
→ Verifies unexpectedProgress = true on Bench Press
```

### Critical Flow 2: Client views workout history

```
Client logs in (invited by trainer above)
→ Sees completed session in portal list
→ Clicks session
→ Sees Bench Press with planned (60kg) vs actual (70kg)
→ PR badge visible on Bench Press row
```

**Location**: `tests/e2e/`
**Framework**: Playwright
**Targets**: Chromium (desktop) + Mobile Safari viewport (375px)

---

## Test Data

### Backend Fixtures (xUnit)
```csharp
// FitPlan.Api.IntegrationTests/Fixtures/TrainerFixtures.cs
public static class TrainerFixtures
{
    public static Trainer DefaultTrainer() => new Trainer(
        id: Guid.NewGuid().ToString(),
        email: "trainer@test.com",
        name: "Test Trainer",
        passwordHash: BCrypt.Net.BCrypt.HashPassword("Password123!"),
        emailConfirmed: true
    );
}
```

### Frontend Fixtures (Vitest)
```typescript
// src/__tests__/fixtures/sessions.ts
export const completedSession = {
  id: 'session-1',
  scheduledAt: '2026-04-20T10:00:00Z',
  status: 'COMPLETED',
  exercises: [
    { id: 'ex-1', name: 'Bench Press', sets: 3, reps: 10, targetWeight: 60,
      actualSets: 3, actualReps: 10, actualWeight: 70, unexpectedProgress: true }
  ]
}
```

---

## CI Integration

Tests run on every push via GitHub Actions (`.github/workflows/ci.yml`):

1. **Lint** — `dotnet format --verify-no-changes` + `pnpm lint`
2. **.NET unit tests** — `dotnet test FitPlan.Domain.Tests FitPlan.Application.Tests`
3. **.NET integration tests** — `dotnet test FitPlan.Api.IntegrationTests` (MongoDB via Testcontainers)
4. **Vue type-check** — `pnpm vue-tsc --noEmit`
5. **Vue unit tests** — `pnpm vitest run`
6. **Docker build** — `docker build -t fitplan-ci .`

E2E tests run only on `main` branch pushes (not PRs) to avoid long feedback cycles.

---

## Traceability

| User Story | Feature File | Integration Test | E2E Test |
|------------|--------------|-----------------|----------|
| US-001 Trainer register | auth.feature | AuthTests.cs | e2e/auth.spec.ts |
| US-002 Google login | auth.feature | AuthGoogleTests.cs | — |
| US-003 Client login | auth.feature | ClientAuthTests.cs | e2e/client-portal.spec.ts |
| US-004 Add client | client-management.feature | ClientTests.cs | e2e/trainer-flow.spec.ts |
| US-006 Schedule session | workout-scheduling.feature | SessionTests.cs | e2e/trainer-flow.spec.ts |
| US-007 Define exercises | workout-scheduling.feature | SessionTests.cs | e2e/trainer-flow.spec.ts |
| US-009 Start live session | live-session.feature | LiveSessionTests.cs | e2e/trainer-flow.spec.ts |
| US-010 Log unexpected progress | live-session.feature | LiveSessionTests.cs | e2e/trainer-flow.spec.ts |
| US-011 Complete session | live-session.feature | LiveSessionTests.cs | e2e/trainer-flow.spec.ts |
| US-012 View past workouts | client-portal.feature | PortalTests.cs | e2e/client-portal.spec.ts |
