# Data Model

## Entities

### Trainer
- id: UUID
- email: String (unique)
- passwordHash: String (nullable — null if Google-only)
- googleId: String (nullable)
- emailConfirmed: Boolean
- name: String
- createdAt: DateTime
- updatedAt: DateTime

### Client
- id: UUID
- trainerId: UUID (ref: Trainer) — tenant scope
- name: String
- email: String
- passwordHash: String (nullable)
- googleId: String (nullable)
- emailConfirmed: Boolean
- createdAt: DateTime
- updatedAt: DateTime

### WorkoutSession
- id: UUID
- trainerId: UUID (ref: Trainer)
- clientId: UUID (ref: Client)
- scheduledAt: DateTime (date + time of session)
- status: Enum [PLANNED, IN_PROGRESS, COMPLETED, CANCELLED]
- exercises: Exercise[] (embedded array)
- startedAt: DateTime (nullable)
- completedAt: DateTime (nullable)
- createdAt: DateTime
- updatedAt: DateTime

### Exercise (embedded in WorkoutSession)
- id: UUID
- name: String
- order: Int
- sets: Int
- reps: Int
- targetWeight: Float (nullable — null for bodyweight)
- actualSets: Int (nullable — filled during live session)
- actualReps: Int (nullable)
- actualWeight: Float (nullable)
- unexpectedProgress: Boolean (default: false)
- notes: String (nullable)

### EmailConfirmationToken
- id: UUID
- userId: UUID
- userType: Enum [TRAINER, CLIENT]
- token: String (unique)
- expiresAt: DateTime
- usedAt: DateTime (nullable)

### RefreshToken
- id: UUID
- userId: UUID
- userType: Enum [TRAINER, CLIENT]
- token: String (unique)
- expiresAt: DateTime
- revokedAt: DateTime (nullable)

---

## Relationships
- Trainer 1:N Client (trainer owns their clients — tenant boundary)
- Trainer 1:N WorkoutSession
- Client 1:N WorkoutSession
- WorkoutSession 1:N Exercise (embedded — no separate collection)

---

## Indexes
- Trainer.email (unique)
- Client.email + trainerId (unique together — same email can be client of multiple trainers)
- WorkoutSession.clientId + scheduledAt (for calendar queries)
- WorkoutSession.trainerId + scheduledAt (for trainer calendar view)
- WorkoutSession.status (for filtering active sessions)
- RefreshToken.token (unique)
- EmailConfirmationToken.token (unique)
