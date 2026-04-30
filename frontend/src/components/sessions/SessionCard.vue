<template>
  <button
    class="w-full text-left p-2 rounded-lg text-xs transition hover:bg-bg-elevated"
    :class="statusClasses"
    @click.stop="$emit('click')"
  >
    <div class="font-mono font-medium">{{ time }}</div>
    <div class="text-text-muted mt-0.5 truncate">{{ session.exerciseCount }} exercises</div>
    <span
      class="inline-block mt-1 px-1.5 py-0.5 rounded text-[10px] font-semibold uppercase tracking-wide"
      :class="badgeClasses"
    >{{ session.status }}</span>
  </button>
</template>

<script setup lang="ts">
import type { SessionSummary } from '@/api/sessions'
import { computed } from 'vue'

const props = defineProps<{ session: SessionSummary }>()
defineEmits<{ click: [] }>()

const time = computed(() =>
  new Date(props.session.scheduledAt).toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit'
  })
)

const statusClasses = computed(() => ({
  'border border-border-default bg-bg-surface': props.session.status === 'PLANNED',
  'border border-primary/40 bg-primary/5': props.session.status === 'IN_PROGRESS',
  'border border-border-subtle bg-bg-surface opacity-70': props.session.status === 'COMPLETED'
}))

const badgeClasses = computed(() => ({
  'bg-bg-subtle text-text-muted': props.session.status === 'PLANNED',
  'bg-primary/20 text-primary': props.session.status === 'IN_PROGRESS',
  'bg-green-500/10 text-green-400': props.session.status === 'COMPLETED'
}))
</script>
