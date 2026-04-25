<template>
  <div class="min-h-screen bg-bg-base flex items-center justify-center px-4">
    <div class="w-full max-w-sm text-center">
      <h1 class="font-display text-4xl text-primary tracking-widest mb-8">FITPLAN</h1>

      <div class="bg-bg-surface rounded-xl p-6 shadow-elevated">
        <div v-if="status === 'confirming'" class="text-text-secondary">
          <div class="animate-spin w-8 h-8 border-2 border-primary border-t-transparent rounded-full mx-auto mb-3"></div>
          Confirming your email…
        </div>

        <div v-else-if="status === 'success'">
          <div class="text-4xl mb-3">✓</div>
          <h2 class="text-text-primary font-semibold mb-2">Email confirmed!</h2>
          <p class="text-text-secondary text-sm mb-4">Your account is ready. You can now sign in.</p>
          <RouterLink
            to="/login"
            class="block bg-primary text-text-inverse font-semibold py-2.5 rounded-lg hover:bg-primary-dark transition text-sm"
          >
            Sign In
          </RouterLink>
        </div>

        <div v-else>
          <div class="text-4xl mb-3">✗</div>
          <h2 class="text-text-primary font-semibold mb-2">Link invalid or expired</h2>
          <p class="text-text-secondary text-sm mb-4">{{ errorMsg }}</p>
          <RouterLink to="/login" class="text-primary text-sm hover:underline">Back to Sign In</RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { apiClient } from '@/api/client'

const route = useRoute()
const status = ref<'confirming' | 'success' | 'error'>('confirming')
const errorMsg = ref('The confirmation link has expired or already been used.')

onMounted(async () => {
  const token = route.query.token as string
  if (!token) {
    status.value = 'error'
    errorMsg.value = 'No confirmation token found in the URL.'
    return
  }
  try {
    await apiClient.post('/auth/confirm-email', { token })
    status.value = 'success'
  } catch {
    status.value = 'error'
  }
})
</script>
