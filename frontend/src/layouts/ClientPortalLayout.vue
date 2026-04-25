<template>
  <div class="min-h-screen bg-bg-base">
    <header class="bg-bg-surface border-b border-border-default px-4 py-3 flex items-center justify-between">
      <span class="font-display text-xl text-primary tracking-widest">FITPLAN</span>
      <button
        @click="handleLogout"
        class="text-sm text-text-secondary hover:text-text-primary transition"
      >
        Sign Out
      </button>
    </header>

    <main class="max-w-2xl mx-auto px-4 py-6">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

async function handleLogout() {
  await auth.logout()
  router.push('/login')
}
</script>
