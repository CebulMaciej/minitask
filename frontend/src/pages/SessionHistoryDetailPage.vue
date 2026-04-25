<template>
  <div>
    <button @click="router.back()" class="flex items-center gap-2 text-text-secondary hover:text-text-primary transition mb-5">
      <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="15 18 9 12 15 6"/>
      </svg>
      Back
    </button>

    <div v-if="loading" class="space-y-3">
      <div class="h-8 w-48 bg-bg-surface rounded-lg animate-pulse mb-6" />
      <div v-for="i in 3" :key="i" class="h-32 bg-bg-surface rounded-xl animate-pulse" />
    </div>

    <div v-else-if="session">
      <div class="mb-6">
        <h1 class="font-display text-2xl text-text-primary tracking-wide">WORKOUT DETAIL</h1>
        <p class="text-text-secondary text-sm mt-1">{{ formattedDate }}</p>
      </div>

      <div class="space-y-3">
        <ExerciseResultRow
          v-for="exercise in session.exercises"
          :key="exercise.id"
          :exercise="exercise"
        />
      </div>
    </div>

    <div v-else class="text-center py-12">
      <p class="text-text-secondary">Session not found</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { portalApi } from '@/api/portal'
import type { WorkoutSession } from '@/api/sessions'
import ExerciseResultRow from '@/components/portal/ExerciseResultRow.vue'

const route = useRoute()
const router = useRouter()
const sessionId = route.params.sessionId as string

const session = ref<WorkoutSession | null>(null)
const loading = ref(true)

const formattedDate = computed(() =>
  session.value
    ? new Date(session.value.completedAt || session.value.scheduledAt).toLocaleDateString('en-US', {
        weekday: 'long',
        month: 'long',
        day: 'numeric',
        year: 'numeric'
      })
    : ''
)

onMounted(async () => {
  try {
    session.value = await portalApi.getSession(sessionId)
  } catch {
    // handled by v-else-if
  } finally {
    loading.value = false
  }
})
</script>
