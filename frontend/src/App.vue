<template>
  <div>
    <RouterView />
    <ToastContainer />

    <!-- Resume session banner -->
    <Transition name="slide-up">
      <div
        v-if="liveSession.sessionId && !isOnLivePage"
        class="fixed bottom-0 left-0 right-0 z-40 bg-primary text-text-inverse px-4 py-3 flex items-center justify-between md:max-w-md md:mx-auto md:bottom-4 md:rounded-xl md:shadow-elevated"
      >
        <div>
          <p class="font-semibold text-sm">Session in progress</p>
          <p class="text-xs opacity-70">Tap to resume</p>
        </div>
        <button
          @click="resumeSession"
          class="bg-white/20 hover:bg-white/30 px-3 py-1.5 rounded-lg text-sm font-semibold transition"
        >
          Resume
        </button>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { RouterView, useRoute, useRouter } from 'vue-router'
import { onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useLiveSessionStore } from '@/stores/liveSession'
import { useToastStore } from '@/stores/toast'
import { setErrorHandler } from '@/api/client'
import ToastContainer from '@/components/ui/ToastContainer.vue'

const auth = useAuthStore()
const liveSession = useLiveSessionStore()
const toast = useToastStore()
const route = useRoute()
const router = useRouter()

const isOnLivePage = computed(() => route.name === 'live-session')

setErrorHandler((msg) => toast.error(msg))

onMounted(async () => {
  await auth.silentRefresh()
  liveSession.restore()
})

function resumeSession() {
  if (liveSession.clientId && liveSession.sessionId) {
    router.push({
      name: 'live-session',
      params: { clientId: liveSession.clientId, sessionId: liveSession.sessionId }
    })
  }
}
</script>

<style>
.slide-up-enter-active,
.slide-up-leave-active {
  transition: transform 0.3s ease;
}
.slide-up-enter-from,
.slide-up-leave-to {
  transform: translateY(100%);
}
</style>
