<template>
  <div class="flex items-start gap-2 py-3 border-b border-border-subtle last:border-0">
    <div class="flex-1 min-w-0 space-y-2">
      <input
        :value="exercise.name"
        @input="update('name', ($event.target as HTMLInputElement).value)"
        type="text"
        placeholder="Exercise name"
        class="w-full bg-bg-subtle border border-border-default rounded-lg px-3 py-2 text-sm text-text-primary placeholder-text-muted focus:outline-none focus:border-primary transition"
        :class="{ 'border-red-500': showNameError && !exercise.name }"
      />

      <div class="grid grid-cols-3 gap-2">
        <div>
          <label class="text-xs text-text-muted block mb-1">Sets</label>
          <input
            :value="exercise.sets"
            @input="update('sets', toPositiveInt(($event.target as HTMLInputElement).value))"
            type="number"
            min="1"
            placeholder="3"
            class="w-full bg-bg-subtle border border-border-default rounded-lg px-2 py-2 text-sm text-text-primary focus:outline-none focus:border-primary transition text-center"
          />
        </div>
        <div>
          <label class="text-xs text-text-muted block mb-1">Reps</label>
          <input
            :value="exercise.reps"
            @input="update('reps', toPositiveInt(($event.target as HTMLInputElement).value))"
            type="number"
            min="1"
            placeholder="10"
            class="w-full bg-bg-subtle border border-border-default rounded-lg px-2 py-2 text-sm text-text-primary focus:outline-none focus:border-primary transition text-center"
          />
        </div>
        <div>
          <label class="text-xs text-text-muted block mb-1">Weight</label>
          <input
            :value="exercise.targetWeight ?? ''"
            @input="update('targetWeight', toOptionalNumber(($event.target as HTMLInputElement).value))"
            type="number"
            min="0"
            placeholder="BW"
            class="w-full bg-bg-subtle border border-border-default rounded-lg px-2 py-2 text-sm text-text-primary focus:outline-none focus:border-primary transition text-center"
          />
        </div>
      </div>
    </div>

    <button
      @click="$emit('remove')"
      class="mt-1 p-1.5 text-text-muted hover:text-red-400 transition flex-shrink-0"
      title="Remove exercise"
    >
      <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4h6v2"/>
      </svg>
    </button>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  exercise: { name: string; sets: number | null; reps: number | null; targetWeight?: number | null }
  showNameError?: boolean
}>()

const emit = defineEmits<{
  update: [field: string, value: unknown]
  remove: []
}>()

function update(field: string, value: unknown) {
  emit('update', field, value)
}

function toPositiveInt(v: string): number | null {
  const n = parseInt(v)
  return n > 0 ? n : null
}

function toOptionalNumber(v: string): number | null {
  if (!v) return null
  const n = parseFloat(v)
  return isNaN(n) ? null : n
}
</script>
