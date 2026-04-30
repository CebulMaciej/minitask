import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiClient, setAccessToken } from '@/api/client'

type UserType = 'TRAINER' | 'CLIENT'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const userType = ref<UserType | null>(null)

  const isAuthenticated = computed(() => !!accessToken.value)

  function _setToken(token: string | null, type: string | null) {
    accessToken.value = token
    userType.value = (type?.toUpperCase() ?? null) as UserType | null
    setAccessToken(token)
  }

  async function login(email: string, password: string, type: UserType) {
    const res = await apiClient.post('/auth/login', { email, password, userType: type })
    _setToken(res.data.accessToken, res.data.userType)
  }

  async function logout() {
    await apiClient.post('/auth/logout').catch(() => {})
    _setToken(null, null)
  }

  async function silentRefresh(): Promise<boolean> {
    try {
      const res = await apiClient.post('/auth/refresh')
      _setToken(res.data.accessToken, res.data.userType)
      return true
    } catch {
      _setToken(null, null)
      return false
    }
  }

  return { accessToken, userType, isAuthenticated, login, logout, silentRefresh }
})
