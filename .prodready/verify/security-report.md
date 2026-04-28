# Security Audit Report

Generated: 2026-04-27

## Dependency Scan

**Frontend (npm audit):**

| Severity | Count |
|----------|-------|
| Critical | 0 |
| High | 0 |
| Moderate | 0 |
| Low | 0 |

**Backend (dotnet list package --vulnerable):**

All 7 projects (4 src + 3 test): no vulnerable packages.

## Secrets Detection

Manual scan of `backend/src/` and `frontend/src/` source files — no hardcoded secrets, API keys, or credentials found. All secrets are read from:
- `IConfiguration` (ASP.NET Core) — populated from environment variables
- `.env` file at runtime (excluded from git via `.gitignore`)

JWT test secrets in integration tests are clearly labeled (`ci-test-secret-32-characters-long`, `integration-test-secret-key-must-be-long!`) and only exist in test code.

## OWASP Top 10 Checklist

| # | Vulnerability | Status | Notes |
|---|---------------|--------|-------|
| A01 | Broken Access Control | ✓ Pass | JWT Bearer on all protected routes; `TenantValidationBehavior` enforces `trainerId` claim; client portal returns only caller's sessions |
| A02 | Cryptographic Failures | ✓ Pass | BCrypt.Net (work factor default 10) for passwords; JWT HS256 with 256-bit secret from env; refresh tokens stored as opaque UUIDs with TTL |
| A03 | Injection | ✓ Pass | MongoDB C# driver uses typed LINQ/BSON expressions — no string-interpolated queries anywhere |
| A04 | Insecure Design | ✓ Pass | Email confirmation required before login; OAuth CSRF protection via state cookie; refresh token rotation on every use |
| A05 | Security Misconfiguration | ⚠ Note | No explicit security response headers (X-Frame-Options, X-Content-Type-Options, Referrer-Policy) added; ASP.NET Core does not set these by default. Recommend adding `app.UseHsts()` and security header middleware in production. |
| A06 | Vulnerable Components | ✓ Pass | 0 known vulnerabilities in all packages (npm audit + dotnet --vulnerable) |
| A07 | Identification & Auth Failures | ✓ Pass | Email confirmation gates login; access tokens expire in 15 min; refresh tokens have 7-day TTL and are revoked on logout |
| A08 | Software & Data Integrity | ✓ Pass | FluentValidation on all MediatR commands; typed request DTOs; no unsafe deserialization |
| A09 | Security Logging & Monitoring | ✓ Pass | `GlobalExceptionMiddleware` returns generic `"An unexpected error occurred."` for 500s; no stack traces or sensitive data in responses; Serilog structured logs |
| A10 | SSRF | ✓ Pass | `GoogleOAuthService` only calls `accounts.google.com` (hardcoded base URL); no user-supplied URLs are fetched |

## Code Review

- [x] No hardcoded secrets in source code
- [x] Passwords hashed with BCrypt (cost factor 10)
- [x] JWT secret read from `IConfiguration` (environment variable)
- [x] `PasswordHash` field is `private set` — never exposed in API responses (only DTOs returned)
- [x] HttpOnly cookies for refresh tokens (`SameSite=Strict`) and OAuth state (`SameSite=Lax`)
- [x] FluentValidation on all write commands with MediatR pipeline behavior
- [x] CORS configured with specific origin allowlist (not `*`)
- [x] Tenant isolation: `TenantValidationBehavior` prevents cross-trainer data access
- [x] `GlobalExceptionMiddleware` never leaks internal error details to clients

## Issues Found

| Severity | Finding | Recommendation |
|----------|---------|---------------|
| Low | No HTTP security response headers | Add `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer` via middleware or nginx |
| Info | `GET /trainer/me` endpoint missing | Implement or remove from OpenAPI spec |

## Result

**Status: PASSED — No Critical or High issues**

- Critical: 0
- High: 0
- Medium: 0
- Low: 1 (missing security headers)
- Informational: 1
