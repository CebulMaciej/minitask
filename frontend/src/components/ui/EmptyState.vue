<template>
  <div class="text-center py-16">
    <p v-if="icon" class="text-4xl mb-3">{{ icon }}</p>
    <p class="text-text-secondary font-medium mb-1">{{ title }}</p>
    <p v-if="description" class="text-text-muted text-sm mb-4">{{ description }}</p>
    <button
      v-if="ctaLabel"
      @click="$emit('cta')"
      class="bg-primary text-text-inverse font-semibold px-5 py-2.5 rounded-lg hover:bg-primary-dark transition text-sm"
    >
      {{ ctaLabel }}
    </button>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  icon?: string
  title: string
  description?: string
  ctaLabel?: string
}>()

defineEmits<{ cta: [] }>()
</script>
