<template>
  <div>
    <!-- Header -->
    <div class="flex items-center gap-3 mb-6">
      <button @click="router.back()" class="text-text-muted hover:text-text-primary transition">
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="15 18 9 12 15 6"/>
        </svg>
      </button>
      <h1 class="font-display text-2xl text-text-primary tracking-wide">{{ clientName }}</h1>
    </div>

    <!-- Month navigation -->
    <div class="flex items-center justify-between mb-4">
      <button @click="prevMonth" class="p-2 text-text-secondary hover:text-text-primary transition">
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="15 18 9 12 15 6"/>
        </svg>
      </button>
      <h2 class="font-semibold text-text-primary">{{ monthLabel }}</h2>
      <button @click="nextMonth" class="p-2 text-text-secondary hover:text-text-primary transition">
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="9 18 15 12 9 6"/>
        </svg>
      </button>
    </div>

    <!-- Mobile: week strip -->
    <div class="md:hidden">
      <div class="flex gap-1 overflow-x-auto pb-2 scrollbar-hide">
        <div
          v-for="day in weekDays"
          :key="day.iso"
          class="flex-shrink-0 w-16 rounded-xl p-2 cursor-pointer transition"
          :class="day.iso === selectedDay ? 'bg-primary/10 border border-primary/30' : 'bg-bg-surface hover:bg-bg-elevated'"
          @click="selectedDay = day.iso"
        >
          <div class="text-center text-text-muted text-xs mb-1">{{ day.dayName }}</div>
          <div class="text-center font-semibold text-sm" :class="day.iso === todayIso ? 'text-primary' : 'text-text-primary'">{{ day.date }}</div>
          <div v-if="getSessionsForDay(day.iso).length" class="mt-1 flex justify-center">
            <span class="w-1.5 h-1.5 rounded-full bg-primary"></span>
          </div>
        </div>
      </div>

      <div class="mt-4 space-y-2">
        <SessionCard
          v-for="session in getSessionsForDay(selectedDay)"
          :key="session.id"
          :session="session"
          @click="editSession(session)"
        />
        <button
          @click="newSession(selectedDay)"
          class="w-full py-3 border border-dashed border-border-default rounded-xl text-sm text-text-muted hover:text-text-primary hover:border-primary transition"
        >
          + Schedule workout
        </button>
      </div>
    </div>

    <!-- Desktop: month grid -->
    <div class="hidden md:block">
      <div class="grid grid-cols-7 gap-1 mb-1">
        <div v-for="d in ['Sun','Mon','Tue','Wed','Thu','Fri','Sat']" :key="d" class="text-center text-xs text-text-muted py-1">{{ d }}</div>
      </div>
      <div class="grid grid-cols-7 gap-1">
        <div
          v-for="cell in calendarCells"
          :key="cell.key"
          class="min-h-[80px] rounded-lg p-1.5 cursor-pointer transition"
          :class="[
            cell.currentMonth ? 'bg-bg-surface hover:bg-bg-elevated' : 'bg-transparent opacity-30',
            cell.iso === todayIso ? 'ring-1 ring-primary' : ''
          ]"
          @click="cell.currentMonth && newSession(cell.iso)"
        >
          <div class="text-xs text-right mb-1" :class="cell.iso === todayIso ? 'text-primary font-semibold' : 'text-text-muted'">
            {{ cell.day }}
          </div>
          <div class="space-y-1">
            <SessionCard
              v-for="session in getSessionsForDay(cell.iso)"
              :key="session.id"
              :session="session"
              @click="editSession(session)"
            />
          </div>
        </div>
      </div>
    </div>

    <SessionFormModal
      v-if="showModal"
      :client-id="clientId"
      :initial-date="modalDate"
      :session="editingSession ?? undefined"
      @close="closeModal"
      @saved="refresh"
      @deleted="refresh"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { clientsApi } from '@/api/clients'
import { sessionsApi, type SessionSummary, type WorkoutSession } from '@/api/sessions'
import SessionCard from '@/components/sessions/SessionCard.vue'
import SessionFormModal from '@/components/sessions/SessionFormModal.vue'

const route = useRoute()
const router = useRouter()
const clientId = route.params.clientId as string

const clientName = ref('')
const sessions = ref<SessionSummary[]>([])
const showModal = ref(false)
const modalDate = ref('')
const editingSession = ref<WorkoutSession | null>(null)

const today = new Date()
const todayIso = today.toISOString().slice(0, 10)
const currentYear = ref(today.getFullYear())
const currentMonth = ref(today.getMonth())
const selectedDay = ref(todayIso)

const monthLabel = computed(() =>
  new Date(currentYear.value, currentMonth.value).toLocaleDateString('en-US', {
    month: 'long',
    year: 'numeric'
  })
)

function isoDate(year: number, month: number, day: number) {
  return `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`
}

const calendarCells = computed(() => {
  const year = currentYear.value
  const month = currentMonth.value
  const firstDay = new Date(year, month, 1).getDay()
  const daysInMonth = new Date(year, month + 1, 0).getDate()
  const prevDays = new Date(year, month, 0).getDate()

  const cells = []
  for (let i = firstDay - 1; i >= 0; i--) {
    const d = prevDays - i
    const prevMonth = month === 0 ? 11 : month - 1
    const prevYear = month === 0 ? year - 1 : year
    cells.push({ key: `prev-${d}`, day: d, iso: isoDate(prevYear, prevMonth, d), currentMonth: false })
  }
  for (let d = 1; d <= daysInMonth; d++) {
    cells.push({ key: `cur-${d}`, day: d, iso: isoDate(year, month, d), currentMonth: true })
  }
  const remaining = 42 - cells.length
  for (let d = 1; d <= remaining; d++) {
    const nextMonth = month === 11 ? 0 : month + 1
    const nextYear = month === 11 ? year + 1 : year
    cells.push({ key: `next-${d}`, day: d, iso: isoDate(nextYear, nextMonth, d), currentMonth: false })
  }
  return cells
})

const weekDays = computed(() => {
  const days = []
  const start = new Date()
  start.setDate(start.getDate() - start.getDay())
  for (let i = 0; i < 14; i++) {
    const d = new Date(start)
    d.setDate(start.getDate() + i)
    days.push({
      iso: d.toISOString().slice(0, 10),
      dayName: d.toLocaleDateString('en-US', { weekday: 'short' }),
      date: d.getDate()
    })
  }
  return days
})

function getSessionsForDay(iso: string) {
  return sessions.value.filter((s) => s.scheduledAt.slice(0, 10) === iso)
}

function prevMonth() {
  if (currentMonth.value === 0) { currentYear.value--; currentMonth.value = 11 }
  else currentMonth.value--
}

function nextMonth() {
  if (currentMonth.value === 11) { currentYear.value++; currentMonth.value = 0 }
  else currentMonth.value++
}

function newSession(date: string) {
  editingSession.value = null
  modalDate.value = date + 'T09:00'
  showModal.value = true
}

async function editSession(session: SessionSummary) {
  const full = await sessionsApi.get(clientId, session.id).catch(() => null)
  if (!full) return
  editingSession.value = full
  modalDate.value = ''
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  editingSession.value = null
}

async function refresh() {
  const year = currentYear.value
  const month = currentMonth.value
  const from = isoDate(year, month, 1)
  const to = `${isoDate(year, month, new Date(year, month + 1, 0).getDate())}T23:59:59`
  sessions.value = await sessionsApi.list(clientId, from, to).catch(() => [])
}

watch([currentYear, currentMonth], refresh)

onMounted(async () => {
  const client = await clientsApi.get(clientId).catch(() => null)
  if (client) clientName.value = client.name
  await refresh()
})
</script>
