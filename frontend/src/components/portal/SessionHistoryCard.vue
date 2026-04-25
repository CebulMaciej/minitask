<template>
  <RouterLink
    :to="{ name: 'portal-session-detail', params: { sessionId: session.id } }"
    class="block p-4 bg-bg-surface hover:bg-bg-elevated rounded-xl transition"
    :class="session.hasUnexpectedProgress ? 'border border-accent/30' : 'border border-border-subtle'"
  >
    <div class="flex items-center justify-between">
      <div>
        <p class="text-text-primary font-medium">{{ formattedDate }}</p>
        <p class="text-text-muted text-sm mt-0.5">{{ session.exerciseCount }} exercises</p>
      </div>
      <div class="flex items-center gap-2">
        <UnexpectedProgressBadge v-if="session.hasUnexpectedProgress" />
        <svg class="w-4 h-4 text-text-muted" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="9 18 15 12 9 6"/>
        </svg>
      </div>
    </div>
  </RouterLink>
</template>

<script setup lang="ts">
import type { PortalSessionSummary } from '@/api/portal'
import UnexpectedProgressBadge from '@/components/live/UnexpectedProgressBadge.vue'
import { computed } from 'vue'

const props = defineProps<{ session: PortalSessionSummary }>()

const formattedDate = computed(() =>
  new Date(props.session.completedAt || props.session.scheduledAt).toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
)
</script>
