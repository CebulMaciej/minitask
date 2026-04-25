<template>
  <div>
    <h1 class="font-display text-3xl text-text-primary tracking-wide mb-6">MY WORKOUTS</h1>

    <div v-if="loading" class="space-y-3">
      <div v-for="i in 5" :key="i" class="h-16 bg-bg-surface rounded-xl animate-pulse" />
    </div>

    <div v-else-if="sessions.length === 0" class="text-center py-16">
      <p class="text-4xl mb-3">💪</p>
      <p class="text-text-secondary font-medium mb-1">No completed workouts yet</p>
      <p class="text-text-muted text-sm">Your workout history will appear here after sessions are completed.</p>
    </div>

    <div v-else class="space-y-3">
      <SessionHistoryCard
        v-for="session in sessions"
        :key="session.id"
        :session="session"
      />

      <button
        v-if="hasMore"
        @click="loadMore"
        :disabled="loadingMore"
        class="w-full py-3 text-sm text-text-secondary hover:text-text-primary border border-border-default rounded-xl transition"
      >
        {{ loadingMore ? 'Loading…' : 'Load more' }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { portalApi, type PortalSessionSummary } from '@/api/portal'
import SessionHistoryCard from '@/components/portal/SessionHistoryCard.vue'

const sessions = ref<PortalSessionSummary[]>([])
const loading = ref(true)
const loadingMore = ref(false)
const page = ref(1)
const hasMore = ref(false)
const PAGE_SIZE = 20

async function load(reset = false) {
  if (reset) {
    page.value = 1
    sessions.value = []
    loading.value = true
  } else {
    loadingMore.value = true
  }
  try {
    const data = await portalApi.listSessions(page.value, PAGE_SIZE)
    sessions.value = reset ? data : [...sessions.value, ...data]
    hasMore.value = data.length === PAGE_SIZE
    page.value++
  } catch {
    // silent
  } finally {
    loading.value = false
    loadingMore.value = false
  }
}

function loadMore() {
  load(false)
}

onMounted(() => load(true))
</script>
