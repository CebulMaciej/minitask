import { test, expect, type Page } from '@playwright/test'

const TEST_TRAINER = {
  name: 'E2E Trainer',
  email: `e2e-trainer-${Date.now()}@test.fitplan`,
  password: 'TestPass123!'
}

const TEST_CLIENT = {
  name: 'E2E Client',
  email: `e2e-client-${Date.now()}@test.fitplan`
}

test.describe('Trainer critical flow', () => {
  let page: Page

  test.beforeEach(async ({ page: p }) => {
    page = p
  })

  test('full trainer journey: register → add client → schedule → live session → complete', async ({ page }) => {
    // Step 1: Register
    await page.goto('/register')
    await page.getByLabel('Full Name').fill(TEST_TRAINER.name)
    await page.getByLabel('Email').fill(TEST_TRAINER.email)
    await page.getByLabel('Password').fill(TEST_TRAINER.password)
    await page.getByRole('button', { name: 'Create Account' }).click()
    await expect(page.getByText('Check your inbox')).toBeVisible()

    // Step 2: Confirm email via API (bypass email in test)
    const confirmToken = await page.request.post('/api/test/confirm-email', {
      data: { email: TEST_TRAINER.email }
    }).then((r) => r.json()).then((d) => d.token)
    await page.goto(`/confirm-email?token=${confirmToken}`)
    await expect(page.getByText('Email confirmed')).toBeVisible()

    // Step 3: Login
    await page.goto('/login')
    await page.getByLabel('Email').fill(TEST_TRAINER.email)
    await page.getByLabel('Password').fill(TEST_TRAINER.password)
    await page.getByRole('button', { name: 'Sign In' }).click()
    await expect(page).toHaveURL(/\/clients/)

    // Step 4: Add client
    await page.getByRole('button', { name: /Add Client/i }).click()
    await page.getByLabel('Full Name').fill(TEST_CLIENT.name)
    await page.getByLabel('Email').fill(TEST_CLIENT.email)
    await page.getByRole('button', { name: 'Add Client' }).click()
    await expect(page.getByText(TEST_CLIENT.name)).toBeVisible()

    // Step 5: Navigate to client calendar
    await page.getByText(TEST_CLIENT.name).click()
    await expect(page).toHaveURL(/\/clients\/.*\/calendar/)

    // Step 6: Schedule a session (click today's cell on desktop grid)
    const today = new Date()
    const dayNum = today.getDate()
    await page.locator('.grid-cols-7 .cursor-pointer').filter({ hasText: String(dayNum) }).first().click()

    // Fill session form
    const modal = page.locator('[data-testid="session-form"]').or(page.locator('.rounded-t-2xl'))
    await expect(modal).toBeVisible()
    await page.getByRole('button', { name: /Add Exercise/i }).click()
    const nameInput = page.locator('input[placeholder="Exercise name"]').first()
    await nameInput.fill('Bench Press')
    await page.locator('input[placeholder="3"]').first().fill('3')
    await page.locator('input[placeholder="10"]').first().fill('8')
    await page.locator('input[placeholder="BW"]').first().fill('80')
    await page.getByRole('button', { name: 'Save' }).click()

    // Session card should appear
    await expect(page.getByText('1 exercises')).toBeVisible()

    // Step 7: Navigate to live session
    const sessionCard = page.getByText('1 exercises').first()
    await sessionCard.click()
    // Edit modal opens — close and navigate to live session via URL
    // (In real app, trainer would navigate from session detail or a "Start" button on the card)
    await page.keyboard.press('Escape')
    // Navigate to live session directly
    const sessionId = await page.evaluate(() => {
      const url = window.location.href
      return url
    })
    // Find session id from API
    const sessionsData = await page.request.get(
      `/api/clients/${page.url().match(/\/clients\/([^/]+)/)?.[1]}/sessions`
    ).then((r) => r.json())
    const sid = sessionsData[0]?.id
    const cid = page.url().match(/\/clients\/([^/]+)/)?.[1]
    await page.goto(`/clients/${cid}/sessions/${sid}/live`)

    // Step 8: Start session
    await page.getByRole('button', { name: 'Start Session' }).click()
    await expect(page.getByText('Live Session')).toBeVisible()

    // Step 9: Log exercise actuals (beat the planned weight)
    const weightInput = page.locator('input[type="number"]').last()
    await weightInput.fill('100') // beats 80kg planned → unexpected progress
    await page.waitForTimeout(800) // debounce

    // PR badge should appear
    await expect(page.locator('text=PR').or(page.locator('[class*="accent"]'))).toBeVisible({ timeout: 3000 })

    // Step 10: Complete session
    await page.getByRole('button', { name: 'Finish Session' }).click()
    await page.getByRole('button', { name: 'Finish' }).click()
    await expect(page).toHaveURL(/\/calendar/)
  })

  test('form validation rejects invalid inputs', async ({ page }) => {
    await page.goto('/login')
    await page.getByRole('button', { name: 'Sign In' }).click()
    await expect(page.getByText('Email is required')).toBeVisible()
    await expect(page.getByText('Password is required')).toBeVisible()

    await page.getByLabel('Email').fill('notanemail')
    await page.getByLabel('Password').fill('short')
    await page.getByRole('button', { name: 'Sign In' }).click()
    await expect(page.getByText('Enter a valid email address')).toBeVisible()
    await expect(page.getByText('Password must be at least 8 characters')).toBeVisible()
  })
})
