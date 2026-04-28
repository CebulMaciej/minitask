# ADR-001: Backend Framework Selection

## Status
Accepted

## Date
2026-04-23

## Context
The project requires a REST API backend with Clean Architecture and CQRS. The developer is building a multi-tenant SaaS platform solo, with a preference for strong structure and long-term maintainability over rapid prototyping speed.

## Decision
We will use ASP.NET Core 9 with MediatR for CQRS dispatch because:
- Developer-specified stack (.NET)
- ASP.NET Core 9 is the current LTS with excellent performance and minimal API support
- MediatR is the de-facto .NET standard for CQRS — thin controllers dispatch commands/queries, keeping API layer logic-free
- FluentValidation integrates cleanly as a MediatR pipeline behavior for request validation

## Consequences

### Positive
- Strict separation: controllers never contain business logic
- Pipeline behaviors (logging, validation, auth) are cross-cutting and reusable
- Strong typing across all layers via C# 12 records for commands/queries/DTOs
- .NET DI container is first-class — no additional IoC library needed

### Negative
- More boilerplate per feature (command + handler + response + validator) vs a simple controller method
- MediatR indirection makes tracing a request harder for newcomers

### Risks
- MediatR v12 introduced breaking changes from v9 — pin version and review changelog on upgrade

## Alternatives Considered
1. **Minimal API only (no MediatR)**: Simpler for small projects, but loses the CQRS boundary the developer explicitly wants
2. **FastEndpoints**: Good CQRS-like structure but less community familiarity than MediatR
