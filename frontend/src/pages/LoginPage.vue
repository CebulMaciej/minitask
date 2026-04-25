<template>
  <div class="min-h-screen bg-bg-base flex items-center justify-center px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-8">
        <h1 class="font-display text-4xl text-primary tracking-widest">FITPLAN</h1>
        <p class="text-text-secondary text-sm mt-1">Trainer Portal</p>
      </div>

      <div class="bg-bg-surface rounded-xl p-6 shadow-elevated">
        <form @submit.prevent="handleLogin" novalidate>
          <div class="space-y-4">
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
                autocomplete="current-password"
                class="w-full bg-bg-subtle border border-border-default rounded-lg px-3 py-2.5 text-text-primary placeholder-text-muted focus:outline-none focus:border-primary focus:shadow-glow-primary transition"
                placeholder="••••••••"
                :class="{ 'border-red-500': errors.password }"
              />
              <p v-if="errors.password" class="mt-1 text-xs text-red-400">{{ errors.password }}</p>
            </div>
          </div>

          <p v-if="loginError" class="mt-3 text-sm text-red-400 text-center">{{ loginError }}</p>

          <button
            type="submit"
            :disabled="loading"
            class="mt-6 w-full bg-primary text-text-inverse font-semibold py-2.5 rounded-lg hover:bg-primary-dark transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ loading ? 'Signing in…' : 'Sign In' }}
          </button>
        </form>

        <div class="mt-4 flex items-center gap-3">
          <div class="flex-1 h-px bg-border-default"></div>
          <span class="text-text-muted text-xs">or</span>
          <div class="flex-1 h-px bg-border-default"></div>
        </div>

        <a
          href="/api/auth/google"
          class="mt-4 flex items-center justify-center gap-2 w-full border border-border-default rounded-lg py-2.5 text-text-primary text-sm hover:bg-bg-elevated transition"
        >
          <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none">
            <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
            <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
            <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z" fill="#FBBC05"/>
            <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
          </svg>
          Continue with Google
        </a>
      </div>

      <p class="text-center text-text-muted text-sm mt-4">
        No account?
        <RouterLink to="/register" class="text-primary hover:underline">Register</RouterLink>
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const loading = ref(false)
const loginError = ref('')

const form = reactive({ email: '', password: '' })
const errors = reactive({ email: '', password: '' })

function validate(): boolean {
  errors.email = ''
  errors.password = ''
  const emailRe = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
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
  return !errors.email && !errors.password
}

async function handleLogin() {
  if (!validate()) return
  loading.value = true
  loginError.value = ''
  try {
    await auth.login(form.email, form.password, 'TRAINER')
    router.push('/clients')
  } catch {
    loginError.value = 'Invalid email or password'
  } finally {
    loading.value = false
  }
}
</script>
