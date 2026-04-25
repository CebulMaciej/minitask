<template>
  <div class="space-y-2">
    <SkeletonCard v-for="i in count" :key="i" :height="height" />
  </div>
</template>

<script setup lang="ts">
import SkeletonCard from './SkeletonCard.vue'

withDefaults(defineProps<{ count?: number; height?: number }>(), { count: 4, height: 64 })
</script>
