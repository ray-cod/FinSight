import { expect, test } from "@playwright/test";

const currentUser = {
  userId: "0e0b7e2f-9dd4-4a90-9c43-9ee9d6bc2f50",
  email: "alex@example.com",
  displayName: "Alex Morgan",
  emailConfirmed: false,
  createdAt: "2026-01-15T10:00:00Z",
};

test.beforeEach(async ({ page }) => {
  await page.route("**/api/v1/auth/login", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ accessToken: "access-token", refreshToken: "refresh-token", accessTokenExpiresAt: new Date(Date.now() + 15 * 60_000).toISOString(), userId: currentUser.userId, sessionId: "11111111-1111-1111-1111-111111111111" }) });
  });
  await page.route("**/api/v1/users/me", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(currentUser) });
  });
});

test("appearance preference persists and the settings workspace remains deep-linkable", async ({ page }) => {
  await page.route("**/api/v1/security/activity?limit=*", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([]) });
  });
  await page.route("**/api/v1/auth/sessions", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([]) });
  });
  await page.route("**/api/v1/notifications/preferences", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ inAppEnabled: true, emailEnabled: false, anomalyNotificationsEnabled: true, subscriptionNotificationsEnabled: true, insightNotificationsEnabled: true }) });
  });

  await page.goto("/login?returnUrl=%2Fsettings%3Fsection%3Dsecurity");
  await page.getByLabel("Email").fill(currentUser.email);
  await page.getByLabel("Password").fill("SecurePassword1!");
  await page.getByRole("button", { name: "Sign in" }).click();

  await expect(page).toHaveURL(/\/settings\?section=security$/);
  await expect(page.getByRole("tab", { name: "Security" })).toHaveAttribute("aria-selected", "true");

  await page.getByRole("button", { name: "Open account menu" }).click();
  await page.getByText("Appearance", { exact: true }).hover();
  await page.getByRole("menuitemradio", { name: "Dark" }).click();

  await expect.poll(async () => page.locator("html").evaluate((element) => element.classList.contains("dark"))).toBe(true);
  await page.reload();
  await expect.poll(async () => page.locator("html").evaluate((element) => element.classList.contains("dark"))).toBe(true);
});

test("offline state is visible without crashing the authenticated shell", async ({ page, context }) => {
  await page.route("**/api/v1/dashboard", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ primaryCurrency: "USD", balances: [], spendingTrend: [], categorySpending: [], recentTransactions: [], upcomingSubscriptions: [], priorityAlerts: [], metrics: { currentMonthSpending: 0, previousMonthSpending: 0, spendingChangePercentage: 0, currentMonthIncome: 0, activeSubscriptionCount: 0, openAnomalyCount: 0, activeInsightCount: 0, unreadNotificationCount: 0 } }) });
  });

  await page.goto("/login?returnUrl=%2Fdashboard");
  await page.getByLabel("Email").fill(currentUser.email);
  await page.getByLabel("Password").fill("SecurePassword1!");
  await page.getByRole("button", { name: "Sign in" }).click();
  await expect(page).toHaveURL(/\/dashboard$/);

  await context.setOffline(true);
  await page.evaluate(() => window.dispatchEvent(new Event("offline")));
  await expect(page.getByRole("status").filter({ hasText: "You are offline" })).toBeVisible();
  await context.setOffline(false);
});
