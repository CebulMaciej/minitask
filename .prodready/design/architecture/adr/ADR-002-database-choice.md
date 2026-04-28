# ADR-002: Database Choice

## Status
Accepted

## Date
2026-04-23

## Context
The data model has a mix of relational data (Trainer → Client → Session) and document-style data (embedded exercises array within a session). The developer prefers MongoDB and is familiar with it. Scale is modest (~100 trainers, ~2,500 clients).

## Decision
We will use MongoDB 7 (self-hosted via Docker) with the official C# driver and a repository pattern (no ORM) because:
- Developer-specified and familiar with it
- Embedded exercises array is a natural document model — no JOINs needed to load a complete session with its exercises
- Self-hosting on VPS keeps costs fixed with no per-read/write charges
- Repository pattern in the Infrastructure layer keeps MongoDB details fully encapsulated behind interfaces

## Consequences

### Positive
- Session reads are a single document fetch (no multi-table JOIN)
- Exercise ordering and embedding is trivial in a document model
- Schema-less allows future exercise fields without migrations

### Negative
- Multi-tenant isolation must be enforced in code (always filter by `trainerId`) — no database-level tenant isolation
- Aggregations across sessions (future analytics) are more complex than SQL GROUP BY
- No transactions by default across multiple collections (MongoDB 4+ supports multi-doc transactions but adds complexity)

### Risks
- **Tenant data leak risk**: If a handler forgets to include `trainerId` in a query filter, data from other trainers is exposed. Mitigation: `ICurrentTrainerAccessor` is injected into all session/client handlers and enforced via an Application-layer base class or pipeline behavior.

## Alternatives Considered
1. **PostgreSQL**: Better for relational queries and tenant isolation via row-level security, but exercises-as-embedded-array would require a separate table + JOIN. Developer preference overrides.
2. **MongoDB Atlas (managed)**: Easier ops but adds recurring cost. Self-hosted on VPS aligns with budget constraint.
