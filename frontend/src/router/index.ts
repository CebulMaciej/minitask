import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/pages/LoginPage.vue'),
      meta: { requiresGuest: true }
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/pages/RegisterPage.vue'),
      meta: { requiresGuest: true }
    },
    {
      path: '/confirm-email',
      name: 'confirm-email',
      component: () => import('@/pages/ConfirmEmailPage.vue')
    },
    {
      path: '/auth/google/callback',
      name: 'google-callback',
      component: () => import('@/pages/GoogleCallbackPage.vue')
    },
    {
      path: '/clients/:clientId/sessions/:sessionId/live',
      name: 'live-session',
      component: () => import('@/pages/LiveSessionPage.vue'),
      meta: { requiresAuth: true, role: 'TRAINER' }
    },
    {
      path: '/',
      component: () => import('@/layouts/AppShell.vue'),
      meta: { requiresAuth: true, role: 'TRAINER' },
      children: [
        {
          path: 'dashboard',
          name: 'dashboard',
          component: () => import('@/pages/TrainerDashboard.vue')
        },
        {
          path: 'clients',
          name: 'clients',
          component: () => import('@/pages/ClientsPage.vue')
        },
        {
          path: 'clients/:clientId/calendar',
          name: 'client-calendar',
          component: () => import('@/pages/ClientCalendarPage.vue')
        }
      ]
    },
    {
      path: '/portal',
      component: () => import('@/layouts/ClientPortalLayout.vue'),
      meta: { requiresAuth: true, role: 'CLIENT' },
      children: [
        {
          path: '',
          name: 'portal',
          component: () => import('@/pages/ClientPortalPage.vue')
        },
        {
          path: 'sessions/:sessionId',
          name: 'portal-session-detail',
          component: () => import('@/pages/SessionHistoryDetailPage.vue')
        }
      ]
    },
    {
      path: '/',
      redirect: '/login'
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/pages/NotFoundPage.vue')
    }
  ]
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.requiresGuest && auth.isAuthenticated) {
    return auth.userType === 'TRAINER' ? { name: 'clients' } : { name: 'portal' }
  }

  if (to.meta.role && auth.userType !== to.meta.role) {
    if (!auth.isAuthenticated) return { name: 'login' }
    return auth.userType === 'TRAINER' ? { name: 'clients' } : { name: 'portal' }
  }
})

export default router
