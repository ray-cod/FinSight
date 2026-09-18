import { expect, test, type Page } from "@playwright/test";

const accessToken = "phase6-access-token";
const refreshToken = "phase6-refresh-token";

const subscription = {
  id: "subscription-1",
  merchantId: "merchant-1",
  merchantName: "Netflix",
  currency: "USD",
  frequency: "Monthly",
  currentAmount: 17.99,
  averageAmount: 16.99,
  detectionConfidence: 0.96,
  firstDetectedAt: "2026-01-01T00:00:00Z",
  lastChargeAt: "2026-09-01T00:00:00Z",
  nextExpectedChargeAt: "2099-10-01T00:00:00Z",
  lastPriceChangedAt: "2026-08-01T00:00:00Z",
  status: "Active",
};

const priceHistory = [
  { id: "price-2", transactionId: "transaction-2", amount: 17.99, observedAt: "2026-09-01T00:00:00Z" },
  { id: "price-1", transactionId: "transaction-1", amount: 15.99, observedAt: "2026-08-01T00:00:00Z" },
];

async function authenticate(page: Page) {
  await page.addInitScript(({ access, refresh }) => {
    sessionStorage.setItem("finsight.access-token", access);
    sessionStorage.setItem("finsight.refresh-token", refresh);
  }, { access: accessToken, refresh: refreshToken });

  await page.route("**/api/v1/auth/refresh", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        accessToken,
        refreshToken,
        accessTokenExpiresAt: "2099-01-01T00:00:00Z",
        userId: "user-1",
        sessionId: "11111111-1111-1111-1111-111111111111",
      }),
    });
  });

  await page.route("**/api/v1/users/me", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        userId: "user-1",
        email: "demo@finsight.local",
        displayName: "Demo User",
        emailConfirmed: true,
      }),
    });
  });
}

test.describe("subscription intelligence", () => {
  test("protected subscription routes redirect unauthenticated users", async ({ page }) => {
    await page.goto("/subscriptions");
    await expect(page).toHaveURL(/\/login\?returnUrl=%2Fsubscriptions/);
  });

  test("authenticated user can review and filter subscription intelligence", async ({ page }) => {
    await authenticate(page);

    await page.route("**/api/v1/subscriptions?**", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([subscription]) });
    });
    await page.route("**/api/v1/subscriptions", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([subscription]) });
    });

    await page.goto("/subscriptions");
    await expect(page.getByRole("heading", { name: /know what is recurring/i })).toBeVisible();
    await expect(page.getByText("Netflix", { exact: true }).first()).toBeVisible();
    await expect(page.getByText("Price changed", { exact: true })).toBeVisible();

    await page.getByLabel("Search subscriptions").fill("Netflix");
    await expect(page).toHaveURL(/q=Netflix/);
    await expect(page.getByText("Netflix", { exact: true }).first()).toBeVisible();
  });

  test("authenticated user can inspect price history and dismiss a subscription", async ({ page }) => {
    await authenticate(page);
    await page.route("**/api/v1/subscriptions/subscription-1/price-history**", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(priceHistory) });
    });
    await page.route("**/api/v1/subscriptions/subscription-1", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(subscription) });
    });
    await page.route("**/api/v1/subscriptions/subscription-1/dismiss", async (route) => {
      await route.fulfill({ status: 204 });
    });

    await page.goto("/subscriptions/subscription-1");
    await expect(page.getByRole("heading", { name: "Netflix", exact: true })).toBeVisible();
    await expect(page.getByText("+12.5%", { exact: true })).toBeVisible();
    await expect(page.getByText("Observed charges", { exact: true })).toBeVisible();

    await page.getByRole("button", { name: /dismiss detection/i }).click();
    await expect(page.getByRole("heading", { name: /dismiss this subscription/i })).toBeVisible();
    await page.getByRole("button", { name: /dismiss subscription/i }).click();
    await expect(page.getByRole("heading", { name: "Netflix", exact: true })).toBeVisible();
  });
});
