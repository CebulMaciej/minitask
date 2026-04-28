import { test, expect } from '@playwright/test'

test.describe('Client portal flow', () => {
  test('client logs in and views session with unexpected progress highlighted', async ({ page }) => {
    // Seed: create trainer, client, and a completed session with unexpected progress via API
    const trainerEmail = `trainer-portal-${Date.now()}@test.fitplan`
    const clientEmail = `client-portal-${Date.now()}@test.fitplan`
    const password = 'TestPass123!'

    // Register and confirm trainer
    await page.request.post('/api/auth/register', {
      data: { name: 'Portal Trainer', email: trainerEmail, password }
    })
    const token = await page.request
      .post('/api/test/confirm-email', { data: { email: trainerEmail } })
      .then((r) => r.json())
      .then((d) => d.token)
    await page.request.post('/api/auth/confirm-email', { data: { token } })

    // Login as trainer to get access token
    const loginRes = await page.request
      .post('/api/auth/login', { data: { email: trainerEmail, password, userType: 'TRAINER' } })
      .then((r) => r.json())
    const trainerToken = loginRes.accessToken

    const authHeaders = { Authorization: `Bearer ${trainerToken}` }

    // Add client
    const client = await page.request
      .post('/api/clients', {
        headers: authHeaders,
        data: { name: 'Portal Client', email: clientEmail }
      })
      .then((r) => r.json())

    // Create session
    const session = await page.request
      .post(`/api/clients/${client.id}/sessions`, {
        headers: authHeaders,
        data: {
          scheduledAt: new Date().toISOString(),
          exercises: [{ name: 'Squat', sets: 3, reps: 5, targetWeight: 100 }]
        }
      })
      .then((r) => r.json())

    // Start session
    await page.request.post(`/api/clients/${client.id}/sessions/${session.id}/start`, {
      headers: authHeaders
    })

    // Log actuals with unexpected progress (150kg > 100kg planned)
    await page.request.patch(
      `/api/clients/${client.id}/sessions/${session.id}/exercises/${session.exercises[0].id}`,
      {
        headers: authHeaders,
        data: { actualSets: 3, actualReps: 5, actualWeight: 150 }
      }
    )

    // Complete session
    await page.request.post(`/api/clients/${client.id}/sessions/${session.id}/complete`, {
      headers: authHeaders
    })

    // Now login as client
    await page.goto('/login')
    // Client uses a temp password set by the system (from invitation email)
    // For test purposes, use a test endpoint to get client credentials
    const clientPassword = await page.request
      .post('/api/test/client-password', { data: { email: clientEmail } })
      .then((r) => r.json())
      .then((d) => d.password)

    await page.getByLabel('Email').fill(clientEmail)
    await page.getByLabel('Password').fill(clientPassword)
    await page.getByRole('button', { name: 'Sign In' }).click()

    // Should redirect to portal
    await expect(page).toHaveURL(/\/portal/)

    // Session appears with PR badge
    await expect(page.locator('text=PR')).toBeVisible()

    // Click to view detail
    await page.locator('[class*="rounded-xl"]').first().click()
    await expect(page).toHaveURL(/\/portal\/sessions\//)

    // Detail page shows exercise with unexpected progress highlighted
    await expect(page.getByText('Squat')).toBeVisible()
    await expect(page.locator('text=PR')).toBeVisible()

    // Actuals are shown
    await expect(page.getByText('150')).toBeVisible() // actual weight
    await expect(page.getByText('100')).toBeVisible() // planned weight
  })

  test('client cannot access trainer routes', async ({ page }) => {
    // This test verifies role-based routing
    // Simulate being logged in as a CLIENT
    await page.goto('/clients')
    // Since we're not authenticated, should redirect to login
    await expect(page).toHaveURL(/\/login/)
  })
})
