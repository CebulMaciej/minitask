<template>
  <button
    class="w-full flex items-center gap-4 p-4 bg-bg-surface hover:bg-bg-elevated rounded-xl transition text-left"
    @click="$emit('click')"
  >
    <div class="w-10 h-10 rounded-full bg-bg-subtle flex items-center justify-center flex-shrink-0">
      <span class="font-semibold text-text-primary text-sm">{{ initials }}</span>
    </div>
    <div class="flex-1 min-w-0">
      <p class="text-text-primary font-medium truncate">{{ client.name }}</p>
      <p class="text-text-muted text-xs truncate">{{ client.email }}</p>
    </div>
    <div class="text-right flex-shrink-0">
      <p v-if="client.lastSessionDate" class="text-text-muted text-xs">
        {{ formatDate(client.lastSessionDate) }}
      </p>
      <p v-else class="text-text-muted text-xs">No sessions</p>
    </div>
    <svg class="w-4 h-4 text-text-muted flex-shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <polyline points="9 18 15 12 9 6"/>
    </svg>
  </button>
</template>

<script setup lang="ts">
import type { Client } from '@/api/clients'
import { computed } from 'vue'

const props = defineProps<{ client: Client }>()
defineEmits<{ click: [] }>()

const initials = computed(() =>
  props.client.name
    .split(' ')
    .map((w) => w[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)
)

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}
</script>
