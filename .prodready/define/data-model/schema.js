// MongoDB schema definitions (no ORM — MongoDB driver with repository pattern)
// These are reference schema shapes used in Infrastructure layer repositories.

// Collection: trainers
const TrainerSchema = {
  _id: "UUID (string)",
  email: "string (unique index)",
  passwordHash: "string | null",
  googleId: "string | null",
  emailConfirmed: "boolean",
  name: "string",
  createdAt: "Date",
  updatedAt: "Date"
};

// Collection: clients
const ClientSchema = {
  _id: "UUID (string)",
  trainerId: "string (ref: trainers._id)",  // tenant boundary
  email: "string",
  passwordHash: "string | null",
  googleId: "string | null",
  emailConfirmed: "boolean",
  name: "string",
  createdAt: "Date",
  updatedAt: "Date"
  // Compound unique index: { trainerId, email }
};

// Collection: workout_sessions
const WorkoutSessionSchema = {
  _id: "UUID (string)",
  trainerId: "string (ref: trainers._id)",
  clientId: "string (ref: clients._id)",
  scheduledAt: "Date",
  status: "enum: PLANNED | IN_PROGRESS | COMPLETED | CANCELLED",
  startedAt: "Date | null",
  completedAt: "Date | null",
  createdAt: "Date",
  updatedAt: "Date",
  exercises: [
    {
      _id: "UUID (string)",
      name: "string",
      order: "number",
      sets: "number",
      reps: "number",
      targetWeight: "number | null",   // null = bodyweight exercise
      actualSets: "number | null",     // filled during live session
      actualReps: "number | null",
      actualWeight: "number | null",
      unexpectedProgress: "boolean",
      notes: "string | null"
    }
  ]
};

// Collection: email_confirmation_tokens
const EmailConfirmationTokenSchema = {
  _id: "UUID (string)",
  userId: "string",
  userType: "enum: TRAINER | CLIENT",
  token: "string (unique index)",
  expiresAt: "Date",
  usedAt: "Date | null"
};

// Collection: refresh_tokens
const RefreshTokenSchema = {
  _id: "UUID (string)",
  userId: "string",
  userType: "enum: TRAINER | CLIENT",
  token: "string (unique index)",
  expiresAt: "Date",
  revokedAt: "Date | null"
};

// MongoDB indexes (to be created in Infrastructure/Data/MongoIndexes.cs)
const Indexes = {
  trainers: [
    { fields: { email: 1 }, options: { unique: true } }
  ],
  clients: [
    { fields: { trainerId: 1, email: 1 }, options: { unique: true } },
    { fields: { trainerId: 1 } }
  ],
  workout_sessions: [
    { fields: { clientId: 1, scheduledAt: -1 } },
    { fields: { trainerId: 1, scheduledAt: -1 } },
    { fields: { status: 1 } }
  ],
  refresh_tokens: [
    { fields: { token: 1 }, options: { unique: true } },
    { fields: { expiresAt: 1 }, options: { expireAfterSeconds: 0 } } // TTL index
  ],
  email_confirmation_tokens: [
    { fields: { token: 1 }, options: { unique: true } },
    { fields: { expiresAt: 1 }, options: { expireAfterSeconds: 0 } } // TTL index
  ]
};
