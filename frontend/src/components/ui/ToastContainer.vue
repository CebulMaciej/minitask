<template>
  <Teleport to="body">
    <div class="fixed top-4 right-4 z-[100] flex flex-col gap-2 max-w-sm">
      <TransitionGroup name="toast">
        <div
          v-for="toast in toastStore.toasts"
          :key="toast.id"
          class="flex items-start gap-3 px-4 py-3 rounded-xl shadow-elevated cursor-pointer"
          :class="toastClasses(toast.type)"
          @click="toastStore.dismiss(toast.id)"
        >
          <span class="text-lg flex-shrink-0">{{ icons[toast.type] }}</span>
          <p class="text-sm">{{ toast.message }}</p>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { useToastStore, type ToastType } from '@/stores/toast'

const toastStore = useToastStore()

const icons: Record<ToastType, string> = {
  success: '✓',
  error: '✗',
  warning: '⚠',
  info: 'ℹ'
}

function toastClasses(type: ToastType) {
  return {
    success: 'bg-green-500/20 border border-green-500/30 text-green-300',
    error: 'bg-red-500/20 border border-red-500/30 text-red-300',
    warning: 'bg-yellow-500/20 border border-yellow-500/30 text-yellow-300',
    info: 'bg-blue-500/20 border border-blue-500/30 text-blue-300'
  }[type]
}
</script>

<style>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.25s ease;
}
.toast-enter-from {
  opacity: 0;
  transform: translateX(1rem);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(1rem);
}
</style>
