<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-50 flex items-center justify-center px-4">
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" />
      <div class="relative bg-bg-elevated rounded-2xl p-6 w-full max-w-xs shadow-elevated">
        <h3 class="text-text-primary font-semibold mb-2">{{ title }}</h3>
        <p class="text-text-secondary text-sm mb-5">{{ message }}</p>
        <div class="flex gap-3">
          <button
            @click="$emit('cancel')"
            class="flex-1 border border-border-default text-text-secondary py-2.5 rounded-lg hover:bg-bg-subtle transition text-sm"
          >
            {{ cancelLabel }}
          </button>
          <button
            @click="$emit('confirm')"
            class="flex-1 py-2.5 rounded-lg font-semibold transition text-sm"
            :class="destructive
              ? 'bg-red-500 text-white hover:bg-red-600'
              : 'bg-primary text-text-inverse hover:bg-primary-dark'"
          >
            {{ confirmLabel }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  destructive?: boolean
}>()

defineEmits<{ confirm: []; cancel: [] }>()
</script>
