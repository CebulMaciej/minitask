import { apiClient } from './client'

export type SessionStatus = 'PLANNED' | 'IN_PROGRESS' | 'COMPLETED'

export interface ExercisePlanned {
  id: string
  name: string
  sets: number
  reps: number
  targetWeight?: number | null
}

export interface ExerciseActual extends ExercisePlanned {
  actualSets?: number | null
  actualReps?: number | null
  actualWeight?: number | null
  unexpectedProgress: boolean
}

export interface WorkoutSession {
  id: string
  clientId: string
  trainerId: string
  scheduledAt: string
  status: SessionStatus
  exercises: ExerciseActual[]
  startedAt?: string
  completedAt?: string
}

export interface CreateSessionInput {
  scheduledAt: string
  exercises: { name: string; sets: number; reps: number; targetWeight?: number | null }[]
}

export const sessionsApi = {
  list: (clientId: string, from?: string, to?: string) =>
    apiClient
      .get<WorkoutSession[]>(`/clients/${clientId}/sessions`, { params: { from, to } })
      .then((r) => r.data),

  get: (clientId: string, sessionId: string) =>
    apiClient.get<WorkoutSession>(`/clients/${clientId}/sessions/${sessionId}`).then((r) => r.data),

  create: (clientId: string, data: CreateSessionInput) =>
    apiClient.post<WorkoutSession>(`/clients/${clientId}/sessions`, data).then((r) => r.data),

  update: (clientId: string, sessionId: string, data: Partial<CreateSessionInput>) =>
    apiClient
      .put<WorkoutSession>(`/clients/${clientId}/sessions/${sessionId}`, data)
      .then((r) => r.data),

  delete: (clientId: string, sessionId: string) =>
    apiClient.delete(`/clients/${clientId}/sessions/${sessionId}`),

  start: (clientId: string, sessionId: string) =>
    apiClient
      .post<WorkoutSession>(`/clients/${clientId}/sessions/${sessionId}/start`)
      .then((r) => r.data),

  logExercise: (
    clientId: string,
    sessionId: string,
    exerciseId: string,
    data: { actualSets?: number; actualReps?: number; actualWeight?: number }
  ) =>
    apiClient
      .patch<WorkoutSession>(
        `/clients/${clientId}/sessions/${sessionId}/exercises/${exerciseId}`,
        data
      )
      .then((r) => r.data),

  complete: (clientId: string, sessionId: string) =>
    apiClient
      .post<WorkoutSession>(`/clients/${clientId}/sessions/${sessionId}/complete`)
      .then((r) => r.data)
}
