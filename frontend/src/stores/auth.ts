import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiClient } from '@/api/client'

type UserType = 'TRAINER' | 'CLIENT'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const userType = ref<UserType | null>(null)

  const isAuthenticated = computed(() => !!accessToken.value)

  async function login(email: string, password: string, type: UserType) {
    const res = await apiClient.post('/auth/login', { email, password, userType: type })
    accessToken.value = res.data.accessToken
    userType.value = res.data.userType
  }

  async function logout() {
    await apiClient.post('/auth/logout').catch(() => {})
    accessToken.value = null
    userType.value = null
  }

  async function silentRefresh(): Promise<boolean> {
    try {
      const res = await apiClient.post('/auth/refresh')
      accessToken.value = res.data.accessToken
      userType.value = res.data.userType
      return true
    } catch {
      accessToken.value = null
      userType.value = null
      return false
    }
  }

  return { accessToken, userType, isAuthenticated, login, logout, silentRefresh }
})
