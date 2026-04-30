<template>
  <div>
    <h1 class="font-display text-3xl text-text-primary tracking-wide mb-6">DASHBOARD</h1>

    <!-- Client legend -->
    <div v-if="clientColors.length" class="flex flex-wrap gap-3 mb-6">
      <RouterLink
        v-for="entry in clientColors"
        :key="entry.clientId"
        :to="`/clients/${entry.clientId}/calendar`"
        class="flex items-center gap-1.5 text-sm text-text-secondary hover:text-text-primary transition"
      >
        <span class="w-2.5 h-2.5 rounded-full flex-shrink-0" :style="{ backgroundColor: entry.color }"></span>
        {{ entry.name }}
      </RouterLink>
    </div>

    <!-- Month navigation -->
    <div class="flex items-center justify-between mb-4">
      <button @click="prevMonth" class="p-2 text-text-secondary hover:text-text-primary transition">
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="15 18 9 12 15 6"/>
        </svg>
      </button>
      <div class="flex items-center gap-3">
        <h2 class="font-semibold text-text-primary">{{ monthLabel }}</h2>
        <button
          v-if="!isCurrentMonth"
          @click="goToToday"
          class="text-xs text-primary hover:underline"
        >Today</button>
      </div>
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
          <div v-if="getSessionsForDay(day.iso).length" class="mt-1 flex justify-center gap-0.5">
            <span
              v-for="s in getSessionsForDay(day.iso).slice(0, 3)"
              :key="s.id"
              class="w-1.5 h-1.5 rounded-full"
              :style="{ backgroundColor: getClientColor(s.clientId) }"
            ></span>
          </div>
        </div>
      </div>

      <div class="mt-4 space-y-2">
        <DashboardSessionCard
          v-for="session in getSessionsForDay(selectedDay)"
          :key="session.id"
          :session="session"
          :client-color="getClientColor(session.clientId)"
          @click="navigateToClient(session.clientId)"
        />
        <p v-if="!getSessionsForDay(selectedDay).length" class="text-sm text-text-muted text-center py-4">
          No sessions scheduled
        </p>
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
          class="min-h-[90px] rounded-lg p-1.5 transition"
          :class="[
            cell.currentMonth ? 'bg-bg-surface' : 'bg-transparent opacity-30',
            cell.iso === todayIso ? 'ring-1 ring-primary' : ''
          ]"
        >
          <div class="text-xs text-right mb-1" :class="cell.iso === todayIso ? 'text-primary font-semibold' : 'text-text-muted'">
            {{ cell.day }}
          </div>
          <div class="space-y-1">
            <DashboardSessionCard
              v-for="session in getSessionsForDay(cell.iso)"
              :key="session.id"
              :session="session"
              :client-color="getClientColor(session.clientId)"
              @click="navigateToClient(session.clientId)"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Loading indicator -->
    <div v-if="loading" class="fixed inset-0 bg-bg-base/50 flex items-center justify-center pointer-events-none">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi, type DashboardSession } from '@/api/sessions'
import DashboardSessionCard from '@/components/sessions/DashboardSessionCard.vue'

const router = useRouter()

const sessions = ref<DashboardSession[]>([])
const loading = ref(false)

const today = new Date()
const todayIso = today.toISOString().slice(0, 10)
const currentYear = ref(today.getFullYear())
const currentMonth = ref(today.getMonth())
const selectedDay = ref(todayIso)

const PALETTE = [
  '#3B82F6',
  '#8B5CF6',
  '#F59E0B',
  '#EC4899',
  '#06B6D4',
  '#10B981',
  '#F97316',
  '#EF4444',
]

const clientColorMap = computed(() => {
  const map = new Map<string, string>()
  let i = 0
  for (const s of sessions.value) {
    if (!map.has(s.clientId)) {
      map.set(s.clientId, PALETTE[i % PALETTE.length])
      i++
    }
  }
  return map
})

const clientColors = computed(() => {
  const seen = new Map<string, { clientId: string; name: string; color: string }>()
  for (const s of sessions.value) {
    if (!seen.has(s.clientId)) {
      seen.set(s.clientId, {
        clientId: s.clientId,
        name: s.clientName,
        color: clientColorMap.value.get(s.clientId) ?? PALETTE[0]
      })
    }
  }
  return [...seen.values()]
})

function getClientColor(clientId: string): string {
  return clientColorMap.value.get(clientId) ?? '#6B7280'
}

const isCurrentMonth = computed(
  () => currentYear.value === today.getFullYear() && currentMonth.value === today.getMonth()
)

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

function getSessionsForDay(iso: string): DashboardSession[] {
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

function goToToday() {
  currentYear.value = today.getFullYear()
  currentMonth.value = today.getMonth()
}

function navigateToClient(clientId: string) {
  router.push(`/clients/${clientId}/calendar`)
}

async function refresh() {
  const year = currentYear.value
  const month = currentMonth.value
  const from = isoDate(year, month, 1)
  const to = `${isoDate(year, month, new Date(year, month + 1, 0).getDate())}T23:59:59`
  loading.value = true
  sessions.value = await dashboardApi.listSessions(from, to).catch(() => [])
  loading.value = false
}

watch([currentYear, currentMonth], refresh)
onMounted(refresh)
</script>
