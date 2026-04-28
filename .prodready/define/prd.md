# Product Requirements Document (PRD)

## 1. Executive Summary

**Product**: FitPlan (working title — multi-tenant trainer platform)
**Problem**: Personal trainers have no purpose-built tool to schedule structured workout plans for clients and capture real-time session performance. They rely on spreadsheets and notes apps that don't support live tracking or client history.
**Solution**: A web app where trainers create calendar-based workout plans, run live sessions with real-time progress logging, and clients can review their completed workout history.
**Target Users**: Personal trainers (primary); their clients (secondary viewers)
**Success Metric**: Active paying trainer subscriptions (post-billing) and weekly session activity

---

## 2. Goals & Non-Goals

### Goals
- Enable trainers to schedule workouts on a calendar (day + hour) with fully defined exercises (sets, reps, weight)
- Support a live session mode where trainers can override planned values and flag unexpected progress in real time
- Allow clients to log in and review their completed workout history
- Multi-tenant: each trainer's data is fully isolated from other trainers
- Mobile-first, dark mode UI with premium typography

### Non-Goals (v1)
- No subscription billing (free at launch)
- No client feedback / side-effect reporting
- No progress charts or analytics
- No AI workout suggestions
- No in-app chat
- No wearable / fitness tracker integrations
- No video uploads

---

## 3. User Personas

### Persona 1: The Trainer
- **Context**: Personal trainer with 15–25 active clients, plans sessions weekly, conducts in-person workouts
- **Pain Point**: Tracking planned vs actual performance across many clients is manual and error-prone
- **Desired Outcome**: A single place to plan sessions, run them on a phone during training, and know the history is saved automatically

### Persona 2: The Client
- **Context**: Gym-goer working with a personal trainer, motivated by seeing progress over time
- **Pain Point**: Has no visibility into their own training history unless the trainer shares notes
- **Desired Outcome**: Can log in and see exactly what was done in each session, including when they exceeded their targets

---

## 4. Functional Requirements

### FR-1: Authentication
- Trainer and client registration via email + password with email confirmation
- Google OAuth login for both trainers and clients
- JWT-based sessions with refresh tokens
- Tenant isolation enforced at the API level — trainers cannot access each other's data

### FR-2: Client Management
- Trainer can add clients (name + email); invitation email is sent automatically
- Trainer sees a list of their own clients only
- Client scoped to trainer tenant — same email can be a client of multiple trainers independently

### FR-3: Workout Scheduling
- Calendar view per client; trainer clicks a day and hour to create a session
- Each session contains an ordered list of exercises (name, sets, reps, target weight)
- Bodyweight exercises supported (weight = null)
- Sessions can be edited or deleted (with confirmation if in progress)

### FR-4: Live Session Mode
- Trainer taps "Start Session" to enter live mode on any PLANNED session
- Each exercise shows planned values; trainer can enter actual values inline
- Actual weight/reps/sets > planned → auto-flagged as unexpected progress with visual indicator
- Session state persists if trainer navigates away
- Trainer taps "Finish Session" to complete — actual values saved alongside planned values

### FR-5: Client Portal
- Client logs in and sees all their COMPLETED sessions, newest first
- Session detail shows each exercise with planned vs actual values
- Unexpected progress exercises are highlighted
- Empty state shown if no completed sessions

---

## 5. Non-Functional Requirements

- **Performance**: Support ~100 trainers × ~25 clients; single VPS deployment is sufficient
- **Mobile**: All trainer-facing and client-facing flows must work on mobile (375px+)
- **Security**: JWT auth, multi-tenant data isolation, HTTPS enforced, MongoDB credentials never exposed to frontend
- **Availability**: Self-hosted on VPS with Docker; basic uptime, no SLA defined for v1
- **Budget**: VPS fixed cost; no cloud managed services

---

## 6. Data Model Summary

### Entities
- **Trainer**: Auth account for the trainer; owns all client and session data (tenant root)
- **Client**: Scoped to a trainer; has auth credentials to access client portal
- **WorkoutSession**: Scheduled session for a client — contains ordered exercises array, status lifecycle (PLANNED → IN_PROGRESS → COMPLETED)
- **Exercise** (embedded in WorkoutSession): Planned values (sets, reps, weight) + actual values logged during live session; `unexpectedProgress` flag

### Key Relationships
- Trainer → Clients: 1:N, tenant boundary
- Trainer/Client → WorkoutSessions: 1:N
- WorkoutSession → Exercises: embedded array (no separate collection)

---

## 7. Scope & Timeline

**MVP Features**: Trainer auth (email + Google), client management, calendar scheduling, exercise builder, live session mode with unexpected progress logging, client portal (history view)

**Future Features**: Client feedback forms, progress charts, subscription billing, AI suggestions, chat, wearable integrations

**Timeline**: No hard deadline
**Team**: Solo developer

---

## 8. Open Questions & Risks

- **Exercise library vs free-text**: Should exercise names be free-text or selected from a reusable library? Free-text is simpler for v1 but harder to aggregate for future analytics.
- **Session notifications**: Should clients be notified when a session is scheduled for them? Not in scope for v1 but will become a common ask.
- **Trainer password reset flow**: Not explicitly designed yet — needed before launch.
- **Risk — solo dev scope**: Auth (email + Google), live session state, and calendar UI are each non-trivial. Prioritize in this order: scheduling → live session → client portal → Google OAuth.

---

## 9. References

- Detailed user stories: `requirements/user-stories.md`
- Data model details: `data-model/entities.md`
- MongoDB schema: `data-model/schema.js`
- Test scenarios: `test-scenarios/*.feature`
- Technical constraints: `constraints.md`
- Vision: `vision.md`
- Constitution: `constitution.md`
