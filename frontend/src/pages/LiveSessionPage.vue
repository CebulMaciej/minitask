<template>
  <div class="min-h-screen bg-bg-base flex flex-col">
    <LiveSessionHeader
      v-if="session && clientName"
      :client-name="clientName"
      :scheduled-at="session.scheduledAt"
      @finish="showFinishConfirm = true"
    />

    <div v-if="loading" class="flex-1 flex items-center justify-center">
      <div class="animate-spin w-8 h-8 border-2 border-primary border-t-transparent rounded-full"></div>
    </div>

    <div v-else-if="error" class="flex-1 flex items-center justify-center px-4 text-center">
      <p class="text-red-400">{{ error }}</p>
    </div>

    <div v-else-if="session" class="flex-1 overflow-y-auto px-4 py-4 space-y-3 pb-8">
      <div v-if="session.status === 'PLANNED'" class="text-center py-8">
        <p class="text-text-secondary mb-4">Ready to start?</p>
        <button
          @click="handleStart"
          :disabled="starting"
          class="bg-primary text-text-inverse font-semibold px-8 py-3 rounded-xl hover:bg-primary-dark transition disabled:opacity-50 text-lg"
        >
          {{ starting ? 'Starting…' : 'Start Session' }}
        </button>
      </div>

      <ExerciseInputRow
        v-for="exercise in session.exercises"
        :key="exercise.id"
        :exercise="exercise"
        @update="(field, val) => handleExerciseUpdate(exercise.id, field, val)"
      />
    </div>

    <ConfirmDialog
      v-if="showFinishConfirm"
      title="Finish session?"
      message="This will mark the session as completed and cannot be undone."
      confirm-label="Finish"
      cancel-label="Keep Going"
      @confirm="handleComplete"
      @cancel="showFinishConfirm = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { sessionsApi, type WorkoutSession } from '@/api/sessions'
import { clientsApi } from '@/api/clients'
import { useLiveSessionStore } from '@/stores/liveSession'
import LiveSessionHeader from '@/components/live/LiveSessionHeader.vue'
import ExerciseInputRow from '@/components/live/ExerciseInputRow.vue'
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue'

const route = useRoute()
const router = useRouter()
const liveSession = useLiveSessionStore()

const clientId = route.params.clientId as string
const sessionId = route.params.sessionId as string

const session = ref<WorkoutSession | null>(null)
const clientName = ref('')
const loading = ref(true)
const starting = ref(false)
const error = ref('')
const showFinishConfirm = ref(false)

const debounceTimers: Record<string, ReturnType<typeof setTimeout>> = {}

async function handleStart() {
  if (!session.value) return
  starting.value = true
  try {
    session.value = await sessionsApi.start(clientId, sessionId)
    liveSession.setActive(clientId, sessionId)
  } catch {
    error.value = 'Failed to start session'
  } finally {
    starting.value = false
  }
}

function handleExerciseUpdate(exerciseId: string, field: string, val: number | null) {
  if (!session.value) return
  const ex = session.value.exercises.find((e) => e.id === exerciseId)
  if (ex) (ex as Record<string, unknown>)[field] = val

  clearTimeout(debounceTimers[exerciseId])
  debounceTimers[exerciseId] = setTimeout(async () => {
    try {
      const updated = await sessionsApi.logExercise(clientId, sessionId, exerciseId, {
        actualSets: ex?.actualSets ?? undefined,
        actualReps: ex?.actualReps ?? undefined,
        actualWeight: ex?.actualWeight ?? undefined
      })
      session.value = updated
    } catch {
      // silent — retry on next input
    }
  }, 600)
}

async function handleComplete() {
  showFinishConfirm.value = false
  try {
    await sessionsApi.complete(clientId, sessionId)
    liveSession.clear()
    router.push({ name: 'client-calendar', params: { clientId } })
  } catch {
    error.value = 'Failed to complete session'
  }
}

onMounted(async () => {
  try {
    const [s, c] = await Promise.all([
      sessionsApi.get(clientId, sessionId),
      clientsApi.get(clientId)
    ])
    session.value = s
    clientName.value = c.name
    if (s.status === 'IN_PROGRESS') {
      liveSession.setActive(clientId, sessionId)
    }
  } catch {
    error.value = 'Session not found'
  } finally {
    loading.value = false
  }
})
</script>
