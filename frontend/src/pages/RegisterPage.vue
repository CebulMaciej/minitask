<template>
  <div class="min-h-screen bg-bg-base flex items-center justify-center px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-8">
        <h1 class="font-display text-4xl text-primary tracking-widest">FITPLAN</h1>
        <p class="text-text-secondary text-sm mt-1">Create Trainer Account</p>
      </div>

      <div v-if="success" class="bg-bg-surface rounded-xl p-6 shadow-elevated text-center">
        <div class="text-4xl mb-3">✉️</div>
        <h2 class="text-text-primary font-semibold mb-2">Check your inbox</h2>
        <p class="text-text-secondary text-sm">
          We sent a confirmation link to <strong class="text-text-primary">{{ form.email }}</strong>.
          Click the link to activate your account.
        </p>
        <RouterLink to="/login" class="mt-4 block text-primary text-sm hover:underline">
          Back to Sign In
        </RouterLink>
      </div>

      <div v-else class="bg-bg-surface rounded-xl p-6 shadow-elevated">
        <form @submit.prevent="handleRegister" novalidate>
          <div class="space-y-4">
            <div>
              <label class="block text-sm text-text-secondary mb-1">Full Name</label>
              <input
                v-model="form.name"
                type="text"
                autocomplete="name"
                class="w-full bg-bg-subtle border border-border-default rounded-lg px-3 py-2.5 text-text-primary placeholder-text-muted focus:outline-none focus:border-primary focus:shadow-glow-primary transition"
                placeholder="Alex Johnson"
                :class="{ 'border-red-500': errors.name }"
              />
              <p v-if="errors.name" class="mt-1 text-xs text-red-400">{{ errors.name }}</p>
            </div>

            <div>
              <label class="block text-sm text-text-secondary mb-1">Email</label>
              <input
                v-model="form.email"
                type="email"
                autocomplete="email"
                class="w-full bg-bg-subtle border border-border-default rounded-lg px-3 py-2.5 text-text-primary placeholder-text-muted focus:outline-none focus:border-primary focus:shadow-glow-primary transition"
                placeholder="trainer@example.com"
                :class="{ 'border-red-500': errors.email }"
              />
              <p v-if="errors.email" class="mt-1 text-xs text-red-400">{{ errors.email }}</p>
            </div>

            <div>
              <label class="block text-sm text-text-secondary mb-1">Password</label>
              <input
                v-model="form.password"
                type="password"
                autocomplete="new-password"
                class="w-full bg-bg-subtle border border-border-default rounded-lg px-3 py-2.5 text-text-primary placeholder-text-muted focus:outline-none focus:border-primary focus:shadow-glow-primary transition"
                placeholder="Min. 8 characters"
                :class="{ 'border-red-500': errors.password }"
              />
              <p v-if="errors.password" class="mt-1 text-xs text-red-400">{{ errors.password }}</p>
            </div>
          </div>

          <p v-if="registerError" class="mt-3 text-sm text-red-400 text-center">{{ registerError }}</p>

          <button
            type="submit"
            :disabled="loading"
            class="mt-6 w-full bg-primary text-text-inverse font-semibold py-2.5 rounded-lg hover:bg-primary-dark transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ loading ? 'Creating account…' : 'Create Account' }}
          </button>
        </form>
      </div>

      <p v-if="!success" class="text-center text-text-muted text-sm mt-4">
        Already have an account?
        <RouterLink to="/login" class="text-primary hover:underline">Sign in</RouterLink>
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { apiClient } from '@/api/client'

const loading = ref(false)
const success = ref(false)
const registerError = ref('')

const form = reactive({ name: '', email: '', password: '' })
const errors = reactive({ name: '', email: '', password: '' })

function validate(): boolean {
  errors.name = ''
  errors.email = ''
  errors.password = ''
  const emailRe = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!form.name.trim()) errors.name = 'Name is required'
  if (!form.email) {
    errors.email = 'Email is required'
  } else if (!emailRe.test(form.email)) {
    errors.email = 'Enter a valid email address'
  }
  if (!form.password) {
    errors.password = 'Password is required'
  } else if (form.password.length < 8) {
    errors.password = 'Password must be at least 8 characters'
  }
  return !errors.name && !errors.email && !errors.password
}

async function handleRegister() {
  if (!validate()) return
  loading.value = true
  registerError.value = ''
  try {
    await apiClient.post('/auth/register', {
      name: form.name.trim(),
      email: form.email,
      password: form.password
    })
    success.value = true
  } catch (err: unknown) {
    const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
    registerError.value = msg ?? 'Registration failed. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>
