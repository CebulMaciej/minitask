<template>
  <div
    class="p-4 rounded-xl transition"
    :class="exercise.unexpectedProgress
      ? 'border border-accent/50 bg-accent/5 shadow-[0_0_12px_rgba(255,107,53,0.15)]'
      : 'border border-border-default bg-bg-surface'"
  >
    <div class="flex items-start justify-between mb-3">
      <h3 class="font-semibold text-text-primary">{{ exercise.name }}</h3>
      <UnexpectedProgressBadge v-if="exercise.unexpectedProgress" />
    </div>

    <div class="grid grid-cols-3 gap-3">
      <div v-for="field in fields" :key="field.key">
        <div class="text-xs text-text-muted mb-1 text-center">{{ field.label }}</div>
        <div class="text-center text-text-secondary font-mono text-sm mb-2">
          {{ plannedValue(field.key) }}
        </div>
        <input
          :value="actualValue(field.key)"
          @input="handleInput(field.key, ($event.target as HTMLInputElement).value)"
          type="number"
          min="0"
          :placeholder="String(plannedValue(field.key) ?? '')"
          class="w-full bg-bg-subtle border border-border-default rounded-lg px-2 py-3 text-text-primary font-mono text-2xl text-center focus:outline-none focus:border-primary focus:shadow-glow-primary transition"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ExerciseActual } from '@/api/sessions'
import UnexpectedProgressBadge from './UnexpectedProgressBadge.vue'

const props = defineProps<{ exercise: ExerciseActual }>()
const emit = defineEmits<{ update: [field: string, value: number | null] }>()

const fields = [
  { key: 'sets', label: 'Sets' },
  { key: 'reps', label: 'Reps' },
  { key: 'weight', label: 'Weight (kg)' }
]

function plannedValue(key: string) {
  if (key === 'sets') return props.exercise.sets
  if (key === 'reps') return props.exercise.reps
  if (key === 'weight') return props.exercise.targetWeight ?? null
  return null
}

function actualValue(key: string) {
  if (key === 'sets') return props.exercise.actualSets ?? ''
  if (key === 'reps') return props.exercise.actualReps ?? ''
  if (key === 'weight') return props.exercise.actualWeight ?? ''
  return ''
}

function handleInput(key: string, raw: string) {
  const n = raw === '' ? null : Number(raw)
  const apiKey = key === 'weight' ? 'actualWeight' : key === 'sets' ? 'actualSets' : 'actualReps'
  emit('update', apiKey, isNaN(n as number) ? null : n)
}
</script>
