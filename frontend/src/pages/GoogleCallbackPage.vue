<template>
  <div class="min-h-screen bg-bg-base flex items-center justify-center">
    <div class="text-center">
      <p v-if="error" class="text-red-400">{{ error }}</p>
      <p v-else class="text-text-secondary">Completing sign-in…</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()
const error = ref('')

onMounted(async () => {
  const ok = await auth.silentRefresh()
  if (ok) {
    router.replace(auth.userType === 'CLIENT' ? '/portal' : '/clients')
  } else {
    error.value = 'Google sign-in failed. Please try again.'
    setTimeout(() => router.replace('/login'), 2000)
  }
})
</script>
