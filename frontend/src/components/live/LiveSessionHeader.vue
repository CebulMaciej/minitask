<template>
  <div class="sticky top-0 z-10 bg-bg-base border-b border-border-default px-4 py-3 flex items-center justify-between">
    <div>
      <p class="text-text-muted text-xs uppercase tracking-widest">Live Session</p>
      <h2 class="font-semibold text-text-primary">{{ clientName }}</h2>
      <p class="text-text-secondary text-xs">{{ formattedDate }}</p>
    </div>

    <button
      @click="$emit('finish')"
      class="bg-primary text-text-inverse font-semibold px-4 py-2 rounded-lg hover:bg-primary-dark transition text-sm"
    >
      Finish Session
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ clientName: string; scheduledAt: string }>()
defineEmits<{ finish: [] }>()

const formattedDate = computed(() =>
  new Date(props.scheduledAt).toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit'
  })
)
</script>
