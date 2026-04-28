# User Stories

## Epic 1: Authentication & Onboarding

### US-001: Trainer Registration (Email)
**As a** personal trainer
**I want to** register with my email and password
**So that** I can access the platform and manage my clients

**Acceptance Criteria**:
- [ ] Given a valid email and password, when I submit the registration form, then an account is created and a confirmation email is sent
- [ ] Given I have not confirmed my email, when I try to log in, then I am blocked with a prompt to verify my email
- [ ] Given a duplicate email, when I try to register, then I see a clear error message
- [ ] Given a confirmed account, when I log in with correct credentials, then I receive a JWT and am redirected to the trainer dashboard

**Priority**: P0
**Estimate**: M

---

### US-002: Trainer Login via Google
**As a** personal trainer
**I want to** sign in with my Google account
**So that** I can access the platform without managing a separate password

**Acceptance Criteria**:
- [ ] Given I click "Sign in with Google", when I complete Google OAuth flow, then I am logged in and redirected to the trainer dashboard
- [ ] Given a new Google account, when I complete OAuth, then a trainer account is auto-created
- [ ] Given an existing email-registered account, when I sign in with the same Google email, then accounts are linked

**Priority**: P0
**Estimate**: M

---

### US-003: Client Login
**As a** client
**I want to** log in to my account
**So that** I can view my past workouts

**Acceptance Criteria**:
- [ ] Given a trainer has added me as a client, when I access the client portal URL, then I can log in with email/password or Google
- [ ] Given valid credentials, when I log in, then I see only my own workout history
- [ ] Given invalid credentials, when I log in, then I see a clear error message

**Priority**: P0
**Estimate**: S

---

## Epic 2: Client Management

### US-004: Add Client
**As a** trainer
**I want to** add a new client to my roster
**So that** I can create training plans for them

**Acceptance Criteria**:
- [ ] Given I am on the client management page, when I fill in client details (name, email) and submit, then the client is created and an invitation email is sent
- [ ] Given the client already exists in the system under another trainer, when I add them, then a new client profile is created scoped to my tenant
- [ ] Given I have added a client, when I view my client list, then the new client appears

**Priority**: P0
**Estimate**: S

---

### US-005: View Client List
**As a** trainer
**I want to** see all my clients in one place
**So that** I can quickly navigate to a client's plan or history

**Acceptance Criteria**:
- [ ] Given I am on the dashboard, when I navigate to clients, then I see a list of all my clients with their names
- [ ] Given I click on a client, when the client detail page loads, then I see their workout calendar and history

**Priority**: P0
**Estimate**: S

---

## Epic 3: Workout Scheduling

### US-006: Schedule a Workout Session
**As a** trainer
**I want to** click on a calendar day and time slot to schedule a workout for a client
**So that** I can plan their training week

**Acceptance Criteria**:
- [ ] Given I am on a client's calendar view, when I click a day and select an hour, then a new workout session form opens
- [ ] Given the form is open, when I fill in the session details (date, time, exercises) and save, then the session appears on the calendar
- [ ] Given a session is saved, when I view the calendar, then the session is shown on the correct day and time slot

**Priority**: P0
**Estimate**: L

---

### US-007: Define Exercises for a Session
**As a** trainer
**I want to** add exercises with sets, reps, and weight to a scheduled session
**So that** the client knows exactly what to do and I can track progress over time

**Acceptance Criteria**:
- [ ] Given I am creating or editing a session, when I add an exercise, then I can specify: exercise name, number of sets, reps per set, and target weight (optional for bodyweight exercises)
- [ ] Given I add multiple exercises, when I save the session, then all exercises are saved in order
- [ ] Given a session is saved, when I reopen it, then I see all exercises with their configured values

**Priority**: P0
**Estimate**: M

---

### US-008: Edit or Delete a Scheduled Session
**As a** trainer
**I want to** edit or remove a scheduled session
**So that** I can adjust plans when needed

**Acceptance Criteria**:
- [ ] Given an existing session on the calendar, when I click it and choose edit, then I can modify all session details
- [ ] Given an existing session, when I choose delete and confirm, then the session is removed from the calendar
- [ ] Given a session in progress (live mode active), when I try to delete it, then I am warned before proceeding

**Priority**: P1
**Estimate**: S

---

## Epic 4: Live Training Session

### US-009: Start a Live Training Session
**As a** trainer
**I want to** start a live session mode for a scheduled workout
**So that** I can track what actually happened during training in real time

**Acceptance Criteria**:
- [ ] Given a scheduled session exists, when I click "Start Session", then the session enters live mode showing all planned exercises
- [ ] Given the session is in live mode, when I view it on my phone, then it is fully usable on mobile
- [ ] Given live mode is active, when I navigate away and return, then the session state is preserved

**Priority**: P0
**Estimate**: M

---

### US-010: Log Unexpected Progress During Session
**As a** trainer
**I want to** note that a client lifted more weight than planned (or exceeded reps/sets)
**So that** the actual achievement is recorded and counts toward progress tracking

**Acceptance Criteria**:
- [ ] Given a live session is active, when I tap an exercise, then I can override the planned weight/reps/sets with actual values
- [ ] Given I enter an actual value higher than planned, then it is visually flagged as unexpected progress
- [ ] Given the session ends, when I save it, then actual values are stored separately from planned values

**Priority**: P0
**Estimate**: M

---

### US-011: Complete a Session
**As a** trainer
**I want to** mark a session as completed
**So that** it moves to the client's workout history

**Acceptance Criteria**:
- [ ] Given a live session is active, when I tap "Finish Session", then I am prompted to confirm
- [ ] Given I confirm, when the session is saved, then it appears in the client's workout history with actual values
- [ ] Given the session is completed, when the client logs in, then they can see the completed session

**Priority**: P0
**Estimate**: S

---

## Epic 5: Client Portal

### US-012: View Past Workouts
**As a** client
**I want to** see my completed training sessions
**So that** I can review what I've done and understand my training history

**Acceptance Criteria**:
- [ ] Given I am logged in as a client, when I access my portal, then I see a list of completed sessions sorted by date (newest first)
- [ ] Given I click on a session, when the detail view opens, then I see all exercises with planned vs actual values
- [ ] Given I have no completed sessions, when I access the portal, then I see an empty state with a motivational message

**Priority**: P0
**Estimate**: M

---
