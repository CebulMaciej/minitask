<template>
  <div class="flex h-screen bg-bg-base overflow-hidden">
    <!-- Desktop sidebar -->
    <nav class="hidden md:flex flex-col w-56 bg-bg-surface border-r border-border-default flex-shrink-0">
      <div class="px-5 py-5 border-b border-border-default">
        <span class="font-display text-2xl text-primary tracking-widest">FITPLAN</span>
      </div>

      <div class="flex-1 py-4 px-3 space-y-1">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm text-text-secondary hover:text-text-primary hover:bg-bg-elevated transition"
          active-class="bg-bg-elevated text-text-primary"
        >
          <component :is="item.icon" class="w-4 h-4 flex-shrink-0" />
          {{ item.label }}
        </RouterLink>
      </div>

      <div class="px-3 py-4 border-t border-border-default">
        <button
          @click="handleLogout"
          class="flex items-center gap-3 w-full px-3 py-2.5 rounded-lg text-sm text-text-secondary hover:text-text-primary hover:bg-bg-elevated transition"
        >
          <LogoutIcon class="w-4 h-4 flex-shrink-0" />
          Sign Out
        </button>
      </div>
    </nav>

    <!-- Main content -->
    <div class="flex-1 flex flex-col min-w-0 overflow-hidden">
      <main class="flex-1 overflow-y-auto p-4 md:p-6 pb-20 md:pb-6">
        <RouterView />
      </main>

      <!-- Mobile bottom nav -->
      <nav class="md:hidden fixed bottom-0 left-0 right-0 bg-bg-surface border-t border-border-default flex">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex-1 flex flex-col items-center gap-1 py-2 text-xs text-text-muted hover:text-text-primary transition"
          active-class="text-primary"
        >
          <component :is="item.icon" class="w-5 h-5" />
          {{ item.label }}
        </RouterLink>
        <button
          @click="handleLogout"
          class="flex-1 flex flex-col items-center gap-1 py-2 text-xs text-text-muted hover:text-text-primary transition"
        >
          <LogoutIcon class="w-5 h-5" />
          Sign Out
        </button>
      </nav>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { defineComponent, h } from 'vue'

const router = useRouter()
const auth = useAuthStore()

const UsersIcon = defineComponent({
  render: () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2' }),
    h('circle', { cx: '9', cy: '7', r: '4' }),
    h('path', { d: 'M23 21v-2a4 4 0 0 0-3-3.87' }),
    h('path', { d: 'M16 3.13a4 4 0 0 1 0 7.75' })
  ])
})

const GridIcon = defineComponent({
  render: () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('rect', { x: '3', y: '3', width: '7', height: '7' }),
    h('rect', { x: '14', y: '3', width: '7', height: '7' }),
    h('rect', { x: '14', y: '14', width: '7', height: '7' }),
    h('rect', { x: '3', y: '14', width: '7', height: '7' })
  ])
})

const LogoutIcon = defineComponent({
  render: () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4' }),
    h('polyline', { points: '16 17 21 12 16 7' }),
    h('line', { x1: '21', y1: '12', x2: '9', y2: '12' })
  ])
})

const navItems = [
  { to: '/clients', label: 'Clients', icon: UsersIcon },
  { to: '/dashboard', label: 'Dashboard', icon: GridIcon }
]

async function handleLogout() {
  await auth.logout()
  router.push('/login')
}
</script>
