import { defineStore } from 'pinia'
import { ref } from 'vue'

interface LiveSessionState {
  sessionId: string
  clientId: string
}

const STORAGE_KEY = 'fitplan:live-session'

export const useLiveSessionStore = defineStore('liveSession', () => {
  const sessionId = ref<string | null>(null)
  const clientId = ref<string | null>(null)

  function restore(): LiveSessionState | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY)
      if (!raw) return null
      const data = JSON.parse(raw) as LiveSessionState
      sessionId.value = data.sessionId
      clientId.value = data.clientId
      return data
    } catch {
      return null
    }
  }

  function setActive(cid: string, sid: string) {
    clientId.value = cid
    sessionId.value = sid
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify({ sessionId: sid, clientId: cid }))
  }

  function clear() {
    sessionId.value = null
    clientId.value = null
    sessionStorage.removeItem(STORAGE_KEY)
  }

  return { sessionId, clientId, restore, setActive, clear }
})
