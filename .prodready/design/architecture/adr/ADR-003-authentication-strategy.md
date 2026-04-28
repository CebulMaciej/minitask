# ADR-003: Authentication Strategy

## Status
Accepted

## Date
2026-04-23

## Context
The platform has two user types (Trainer, Client) that must authenticate independently. The developer wants Google OAuth as the primary method with email/password as a fallback. No third-party auth providers (Keycloak, Auth0) are permitted. The app must work on mobile, ruling out session cookies as the primary mechanism.

## Decision
We will use custom JWT (access + refresh token) with Google OAuth2 for social login:

- **Access token**: Short-lived JWT (15 min), signed with RS256 or HS256, contains `userId`, `userType` (TRAINER | CLIENT), `trainerId` (for clients: their trainer's ID for tenant scoping)
- **Refresh token**: Long-lived (7 days), stored in MongoDB with TTL index, rotated on each use
- **Google OAuth**: Backend-handled Authorization Code flow — frontend redirects to `/api/auth/google`, backend exchanges code for Google profile, issues JWT
- **Email/password**: BCrypt password hashing, email confirmation required before first login
- **Token storage**: Access token in Pinia memory store (never localStorage); refresh token in HttpOnly Secure cookie

## Consequences

### Positive
- No external auth dependency — no vendor lock-in, no per-MAU cost
- HttpOnly cookie for refresh token prevents XSS token theft
- Short access token TTL limits blast radius of a leaked token
- `trainerId` in client JWT enables fast tenant scoping without a DB lookup per request

### Negative
- Password reset flow must be built from scratch (email + token)
- Google OAuth backend flow is more complex than a frontend SDK approach
- Refresh token rotation means mobile clients must handle 401 → refresh → retry transparently

### Risks
- **Token leakage**: Access token in memory means SPA refresh loses auth state — mitigated by silent refresh via refresh token cookie on app load
- **CSRF on cookie**: Refresh token cookie must use `SameSite=Strict` + CSRF token on the refresh endpoint

## Alternatives Considered
1. **Keycloak**: Feature-rich but explicitly ruled out by developer
2. **Auth0 / Clerk**: Managed, less code — but adds external dependency and per-MAU cost
3. **Session cookies only**: Simpler but doesn't work well with SPA + mobile-first requirement
