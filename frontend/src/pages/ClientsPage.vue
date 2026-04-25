<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="font-display text-3xl text-text-primary tracking-wide">CLIENTS</h1>
      <button
        @click="showAddModal = true"
        class="bg-primary text-text-inverse font-semibold px-4 py-2 rounded-lg hover:bg-primary-dark transition text-sm"
      >
        + Add Client
      </button>
    </div>

    <div v-if="loading" class="space-y-3">
      <div v-for="i in 4" :key="i" class="h-16 bg-bg-surface rounded-xl animate-pulse" />
    </div>

    <div v-else-if="error" class="text-red-400 text-sm">{{ error }}</div>

    <div v-else-if="clients.length === 0" class="text-center py-16">
      <p class="text-text-muted text-4xl mb-3">👥</p>
      <p class="text-text-secondary font-medium mb-1">No clients yet</p>
      <p class="text-text-muted text-sm mb-4">Add your first client to get started</p>
      <button
        @click="showAddModal = true"
        class="bg-primary text-text-inverse font-semibold px-5 py-2.5 rounded-lg hover:bg-primary-dark transition text-sm"
      >
        Add Client
      </button>
    </div>

    <div v-else class="space-y-2">
      <ClientListItem
        v-for="client in clients"
        :key="client.id"
        :client="client"
        @click="router.push({ name: 'client-calendar', params: { clientId: client.id } })"
      />
    </div>

    <AddClientModal v-if="showAddModal" @close="showAddModal = false" @added="refresh" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { clientsApi, type Client } from '@/api/clients'
import ClientListItem from '@/components/clients/ClientListItem.vue'
import AddClientModal from '@/components/clients/AddClientModal.vue'

const router = useRouter()
const clients = ref<Client[]>([])
const loading = ref(true)
const error = ref('')
const showAddModal = ref(false)

async function refresh() {
  loading.value = true
  error.value = ''
  try {
    clients.value = await clientsApi.list()
  } catch {
    error.value = 'Failed to load clients'
  } finally {
    loading.value = false
  }
}

onMounted(refresh)
</script>
