<template>
  <div class="bg-bg-surface rounded-xl p-4 animate-pulse" :style="{ height: height + 'px' }">
    <div class="h-4 bg-bg-subtle rounded w-3/4 mb-2" />
    <div class="h-3 bg-bg-subtle rounded w-1/2" />
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ height?: number }>(), { height: 64 })
</script>
