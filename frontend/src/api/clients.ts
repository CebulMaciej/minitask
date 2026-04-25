import { apiClient } from './client'

export interface Client {
  id: string
  name: string
  email: string
  trainerId: string
  createdAt: string
  lastSessionDate?: string
}

export const clientsApi = {
  list: () => apiClient.get<Client[]>('/clients').then((r) => r.data),
  get: (id: string) => apiClient.get<Client>(`/clients/${id}`).then((r) => r.data),
  add: (data: { name: string; email: string }) =>
    apiClient.post<Client>('/clients', data).then((r) => r.data),
  remove: (id: string) => apiClient.delete(`/clients/${id}`)
}
