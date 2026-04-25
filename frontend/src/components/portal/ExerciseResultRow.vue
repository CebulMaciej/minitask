<template>
  <div
    class="p-4 rounded-xl"
    :class="exercise.unexpectedProgress
      ? 'border border-accent/40 bg-accent/5'
      : 'border border-border-subtle bg-bg-surface'"
  >
    <div class="flex items-start justify-between mb-3">
      <h4 class="font-medium text-text-primary">{{ exercise.name }}</h4>
      <UnexpectedProgressBadge v-if="exercise.unexpectedProgress" />
    </div>

    <div class="grid grid-cols-3 gap-3 text-center text-sm">
      <div v-for="field in fields" :key="field.key">
        <p class="text-text-muted text-xs mb-1">{{ field.label }}</p>
        <p class="text-text-secondary font-mono">{{ planned(field.key) }}</p>
        <p
          class="font-mono font-semibold mt-0.5"
          :class="exercise.unexpectedProgress ? 'text-accent' : 'text-text-primary'"
        >{{ actual(field.key) }}</p>
        <p class="text-text-muted text-[10px] mt-0.5">actual</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ExerciseActual } from '@/api/sessions'
import UnexpectedProgressBadge from '@/components/live/UnexpectedProgressBadge.vue'

const props = defineProps<{ exercise: ExerciseActual }>()

const fields = [
  { key: 'sets', label: 'Sets' },
  { key: 'reps', label: 'Reps' },
  { key: 'weight', label: 'Weight' }
]

function planned(key: string) {
  if (key === 'sets') return props.exercise.sets ?? '—'
  if (key === 'reps') return props.exercise.reps ?? '—'
  if (key === 'weight') return props.exercise.targetWeight ?? 'BW'
  return '—'
}

function actual(key: string) {
  if (key === 'sets') return props.exercise.actualSets ?? '—'
  if (key === 'reps') return props.exercise.actualReps ?? '—'
  if (key === 'weight') return props.exercise.actualWeight ?? 'BW'
  return '—'
}
</script>
