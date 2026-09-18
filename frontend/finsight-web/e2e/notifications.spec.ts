import { expect, test, type Page } from "@playwright/test";

const accessToken = "phase8-access-token";
const refreshToken = "phase8-refresh-token";
const user = {
  userId: "user-8",
  email: "demo@finsight.local",
  displayName: "Demo User",
  emailConfirmed: true,
  createdAt: "2026-01-15T10:00:00Z",
};

const notifications = [
  {
    id: "notification-1",
    type: "FinancialInsight",
    channel: "InApp",
    title: "Insight available",
    message: "Your dining spending is trending higher than usual.",
    status: "Delivered",
    createdAt: "2026-09-17T08:00:00Z",
    readAt: null,
  },
  {
    id: "notification-2",
    type: "SubscriptionPriceChanged",
    channel: "InApp",
    title: "Subscription price changed",
    message: "Your streaming subscription increased this month.",
    status: "Read",
    createdAt: "2026-09-16T08:00:00Z",
    readAt: "2026-09-16T09:00:00Z",
  },
];

const preferences = {
  inAppEnabled: true,
  emailEnabled: false,
  anomalyNotificationsEnabled: true,
  subscriptionNotificationsEnabled: true,
  insightNotificationsEnabled: true,
};

async function authenticate(page: Page) {
  await page.addInitScript(({ access, refresh }) => {
    sessionStorage.setItem("finsight.access-token", access);
    sessionStorage.setItem("finsight.refresh-token", refresh);
  }, { access: accessToken, refresh: refreshToken });

  await page.route("**/api/v1/auth/refresh", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ accessToken, refreshToken, accessTokenExpiresAt: "2099-01-01T00:00:00Z", userId: user.userId, sessionId: "11111111-1111-1111-1111-111111111111" }) });
  });
  await page.route("**/api/v1/users/me", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(user) });
  });
}

test.describe("notifications and preferences", () => {
  test("protected notification route redirects unauthenticated users", async ({ page }) => {
    await page.goto("/notifications");
    await expect(page).toHaveURL(/\/login\?returnUrl=%2Fnotifications/);
  });

  test("authenticated users can review unread notifications and mark all read", async ({ page }) => {
    await authenticate(page);

    let unreadCount = 1;
    let dismissed = false;
    await page.route("**/api/v1/notifications/unread-count", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ count: unreadCount }) });
    });
    await page.route("**/api/v1/notifications?**", async (route) => {
      const current = notifications.map((item) => item.id === "notification-1" && dismissed ? { ...item, status: "Dismissed", readAt: "2026-09-17T08:30:00Z" } : item);
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(current) });
    });
    await page.route("**/api/v1/notifications/notification-1/dismiss", async (route) => {
      dismissed = true;
      unreadCount = 0;
      await route.fulfill({ status: 204 });
    });
    await page.route("**/api/v1/notifications/read-all", async (route) => {
      unreadCount = 0;
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ count: 1 }) });
    });

    await page.goto("/notifications");
    await expect(page.getByRole("heading", { name: "Notifications", exact: true })).toBeVisible();
    await expect(page.getByText("Insight available", { exact: true })).toBeVisible();
    await expect(page.getByText("1", { exact: true }).first()).toBeVisible();

    await page.getByRole("button", { name: "Dismiss" }).click();
    await expect(page.getByText("Dismissed", { exact: true }).last()).toBeVisible();

    await page.getByRole("button", { name: "Mark all read" }).click();
    await expect(page.getByRole("button", { name: "Mark all read" })).toBeDisabled();
  });

  test("authenticated users can save notification preferences", async ({ page }) => {
    await authenticate(page);

    await page.route("**/api/v1/notifications/unread-count", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ count: 0 }) });
    });
    await page.route("**/api/v1/notifications/preferences", async (route) => {
      if (route.request().method() === "GET") {
        await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(preferences) });
        return;
      }
      await route.fulfill({ status: 204 });
    });

    await page.goto("/settings?section=notifications");
    await expect(page.getByRole("heading", { name: "Settings", exact: true })).toBeVisible();
    await expect(page.getByRole("heading", { name: "Notification preferences" })).toBeVisible();

    await page.getByRole("switch", { name: /email notifications/i }).click();
    await expect(page.getByText("Unsaved changes", { exact: true })).toBeVisible();
    await page.getByRole("button", { name: "Save preferences" }).click();
    await expect(page.getByText("Preferences saved.", { exact: true })).toBeVisible();
  });
});
