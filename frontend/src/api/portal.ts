import { apiClient } from './client'
import type { WorkoutSession } from './sessions'

export interface PortalSessionSummary {
  id: string
  scheduledAt: string
  completedAt: string
  exerciseCount: number
  hasUnexpectedProgress: boolean
}

export const portalApi = {
  listSessions: (page = 1, pageSize = 20) =>
    apiClient
      .get<PortalSessionSummary[]>('/portal/sessions', { params: { page, pageSize } })
      .then((r) => r.data),

  getSession: (sessionId: string) =>
    apiClient.get<WorkoutSession>(`/portal/sessions/${sessionId}`).then((r) => r.data)
}
