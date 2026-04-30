<template>
  <button
    class="w-full text-left p-2 rounded-lg text-xs transition hover:brightness-110 border border-border-default bg-bg-surface"
    :style="{ borderLeftColor: clientColor, borderLeftWidth: '3px' }"
    @click.stop="$emit('click')"
  >
    <div class="font-mono font-medium text-text-primary">{{ time }}</div>
    <div class="font-medium truncate mt-0.5" :style="{ color: clientColor }">{{ session.clientName }}</div>
    <div class="text-text-muted mt-0.5">{{ session.exerciseCount }} ex.</div>
    <span
      class="inline-block mt-1 px-1.5 py-0.5 rounded text-[10px] font-semibold uppercase tracking-wide"
      :class="badgeClasses"
    >{{ session.status }}</span>
  </button>
</template>

<script setup lang="ts">
import type { DashboardSession } from '@/api/sessions'
import { computed } from 'vue'

const props = defineProps<{ session: DashboardSession; clientColor: string }>()
defineEmits<{ click: [] }>()

const time = computed(() =>
  new Date(props.session.scheduledAt).toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit'
  })
)

const badgeClasses = computed(() => ({
  'bg-bg-subtle text-text-muted': props.session.status === 'PLANNED',
  'bg-primary/20 text-primary': props.session.status === 'IN_PROGRESS',
  'bg-green-500/10 text-green-400': props.session.status === 'COMPLETED',
  'bg-red-500/10 text-red-400': props.session.status === 'CANCELLED'
}))
</script>
