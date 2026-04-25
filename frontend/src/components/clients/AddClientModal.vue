<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-50 flex items-center justify-center px-4">
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="$emit('close')" />
      <div class="relative bg-bg-elevated rounded-2xl p-6 w-full max-w-sm shadow-elevated">
        <h2 class="font-display text-2xl text-text-primary tracking-wide mb-5">ADD CLIENT</h2>

        <form @submit.prevent="handleSubmit" novalidate>
          <div class="space-y-4">
            <div>
              <label class="block text-sm text-text-secondary mb-1">Full Name</label>
              <input
                v-model="form.name"
                type="text"
                class="w-full bg-bg-subtle border border-border-default rounded-lg px-3 py-2.5 text-text-primary placeholder-text-muted focus:outline-none focus:border-primary transition"
                placeholder="Client name"
                :class="{ 'border-red-500': errors.name }"
              />
              <p v-if="errors.name" class="mt-1 text-xs text-red-400">{{ errors.name }}</p>
            </div>

            <div>
              <label class="block text-sm text-text-secondary mb-1">Email</label>
              <input
                v-model="form.email"
                type="email"
                class="w-full bg-bg-subtle border border-border-default rounded-lg px-3 py-2.5 text-text-primary placeholder-text-muted focus:outline-none focus:border-primary transition"
                placeholder="client@example.com"
                :class="{ 'border-red-500': errors.email }"
              />
              <p v-if="errors.email" class="mt-1 text-xs text-red-400">{{ errors.email }}</p>
            </div>
          </div>

          <p v-if="submitError" class="mt-3 text-sm text-red-400">{{ submitError }}</p>

          <div class="flex gap-3 mt-6">
            <button
              type="button"
              @click="$emit('close')"
              class="flex-1 border border-border-default text-text-secondary py-2.5 rounded-lg hover:bg-bg-subtle transition text-sm"
            >
              Cancel
            </button>
            <button
              type="submit"
              :disabled="loading"
              class="flex-1 bg-primary text-text-inverse font-semibold py-2.5 rounded-lg hover:bg-primary-dark transition disabled:opacity-50 text-sm"
            >
              {{ loading ? 'Adding…' : 'Add Client' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { clientsApi } from '@/api/clients'

const emit = defineEmits<{ close: []; added: [] }>()

const loading = ref(false)
const submitError = ref('')
const form = reactive({ name: '', email: '' })
const errors = reactive({ name: '', email: '' })

function validate(): boolean {
  errors.name = ''
  errors.email = ''
  const emailRe = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!form.name.trim()) errors.name = 'Name is required'
  if (!form.email) {
    errors.email = 'Email is required'
  } else if (!emailRe.test(form.email)) {
    errors.email = 'Enter a valid email address'
  }
  return !errors.name && !errors.email
}

async function handleSubmit() {
  if (!validate()) return
  loading.value = true
  submitError.value = ''
  try {
    await clientsApi.add({ name: form.name.trim(), email: form.email })
    emit('added')
    emit('close')
  } catch (err: unknown) {
    const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
    submitError.value = msg ?? 'Failed to add client'
  } finally {
    loading.value = false
  }
}
</script>
