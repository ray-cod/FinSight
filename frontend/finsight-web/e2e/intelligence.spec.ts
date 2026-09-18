import { expect, test, type Page } from "@playwright/test";

const accessToken = "phase7-access-token";
const refreshToken = "phase7-refresh-token";
const currentUser = {
  userId: "11111111-1111-1111-1111-111111111111",
  email: "demo@finsight.local",
  displayName: "Demo User",
  emailConfirmed: true,
  createdAt: "2026-01-15T10:00:00Z",
};

async function authenticate(page: Page) {
  await page.addInitScript(({ access, refresh }) => {
    sessionStorage.setItem("finsight.access-token", access);
    sessionStorage.setItem("finsight.refresh-token", refresh);
  }, { access: accessToken, refresh: refreshToken });

  await page.route("**/api/v1/auth/refresh", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ accessToken, refreshToken, accessTokenExpiresAt: "2099-01-01T00:00:00Z", userId: currentUser.userId, sessionId: "11111111-1111-1111-1111-111111111111" }) });
  });

  await page.route("**/api/v1/users/me", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(currentUser) });
  });
}

const anomaly = {
  id: "anomaly-1",
  transactionId: "transaction-1",
  accountId: "account-1",
  type: "LargeTransaction",
  severity: "High",
  score: 0.92,
  confidence: 0.96,
  title: "Unusually large purchase",
  description: "This purchase is materially above your normal spending baseline.",
  evidence: "Current charge: 920.00; typical charge: 180.00.",
  detectedAt: "2026-09-15T10:00:00Z",
  status: "Open",
  resolvedAt: null,
};

const insight = {
  id: "insight-1",
  anomalyId: "anomaly-1",
  transactionId: "transaction-1",
  type: "UnusualTransaction",
  severity: "High",
  title: "Unusually large purchase",
  message: "This purchase is materially above your recent spending baseline.",
  occurredAt: "2026-09-15T10:00:00Z",
  expiresAt: "2026-09-30T10:00:00Z",
  status: "Active",
  createdAt: "2026-09-15T10:01:00Z",
};

test.describe("anomalies and financial insights", () => {
  test("protected intelligence routes redirect unauthenticated users", async ({ page }) => {
    await page.goto("/anomalies");
    await expect(page).toHaveURL(/\/login\?returnUrl=%2Fanomalies/);
  });

  test("authenticated user can filter, inspect and resolve an anomaly", async ({ page }) => {
    await authenticate(page);

    await page.route("**/api/v1/anomalies?**", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([anomaly]) });
    });
    await page.route("**/api/v1/anomalies", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([anomaly]) });
    });
    await page.route("**/api/v1/accounts", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([{ id: "account-1", connectionId: "connection-1", institutionId: "institution-1", name: "Everyday Checking", type: "Checking", currency: "USD", currentBalance: 1200, availableBalance: 1100, status: "Active" }]) });
    });
    await page.route("**/api/v1/transactions/transaction-1", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ id: "transaction-1", accountId: "account-1", accountName: "Everyday Checking", institutionId: "institution-1", institutionName: "FinSight Demo Bank", providerTransactionId: "provider-1", rawDescription: "AMZN Mktp US", normalizedDescription: "Amazon", merchantId: "merchant-1", merchantName: "Amazon", categoryId: null, categoryName: "Shopping", subcategoryId: null, subcategoryName: null, amount: -920, currency: "USD", transactionDate: anomaly.detectedAt, importedAt: anomaly.detectedAt, type: "Purchase", status: "Imported", classificationStatus: "Classified", classificationSource: "Rule", classificationConfidence: 0.99, classifiedAt: anomaly.detectedAt, userCorrectedAt: null }) });
    });
    await page.route("**/api/v1/anomalies/anomaly-1", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(anomaly) });
    });
    await page.route("**/api/v1/anomalies/anomaly-1/resolve", async (route) => {
      await route.fulfill({ status: 204 });
    });

    await page.goto("/anomalies");
    await expect(page.getByRole("heading", { name: "Anomalies", exact: true })).toBeVisible();
    await expect(page.getByText("Unusually large purchase", { exact: true }).first()).toBeVisible();

    await page.getByLabel("Severity").click();
    await page.getByRole("option", { name: "High" }).click();
    await expect(page).toHaveURL(/severity=High/);

    await page.getByRole("link", { name: /view evidence/i }).click();
    await expect(page).toHaveURL(/\/anomalies\/anomaly-1$/);
    await expect(page.getByText("Supporting evidence", { exact: true })).toBeVisible();
    await expect(page.getByText("Current charge: 920.00; typical charge: 180.00.", { exact: true })).toBeVisible();

    await page.getByRole("button", { name: "Resolve" }).first().click();
    await expect(page.getByRole("heading", { name: "Resolve anomaly?" })).toBeVisible();
    await page.getByRole("button", { name: "Resolve anomaly" }).click();
  });

  test("authenticated user can inspect and mark an insight as seen", async ({ page }) => {
    await authenticate(page);

    await page.route("**/api/v1/insights?**", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([insight]) });
    });
    await page.route("**/api/v1/insights", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([insight]) });
    });
    await page.route("**/api/v1/insights/insight-1", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(insight) });
    });
    await page.route("**/api/v1/anomalies/anomaly-1", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(anomaly) });
    });
    await page.route("**/api/v1/transactions/transaction-1", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ id: "transaction-1", accountId: "account-1", accountName: "Everyday Checking", institutionId: "institution-1", institutionName: "FinSight Demo Bank", providerTransactionId: "provider-1", rawDescription: "AMZN Mktp US", normalizedDescription: "Amazon", merchantId: "merchant-1", merchantName: "Amazon", categoryId: null, categoryName: "Shopping", subcategoryId: null, subcategoryName: null, amount: -920, currency: "USD", transactionDate: insight.occurredAt, importedAt: insight.createdAt, type: "Purchase", status: "Imported", classificationStatus: "Classified", classificationSource: "Rule", classificationConfidence: 0.99, classifiedAt: insight.createdAt, userCorrectedAt: null }) });
    });

    let seen = false;
    await page.route("**/api/v1/insights/insight-1/seen", async (route) => {
      seen = true;
      await route.fulfill({ status: 204 });
    });

    await page.goto("/insights");
    await expect(page.getByRole("heading", { name: "Insights", exact: true })).toBeVisible();
    await expect(page.getByText("Unusually large purchase", { exact: true }).first()).toBeVisible();
    await page.getByRole("link", { name: /view insight/i }).click();
    await expect(page).toHaveURL(/\/insights\/insight-1$/);
    await expect(page.getByText(insight.message, { exact: true })).toBeVisible();
    await page.getByRole("button", { name: /mark as seen/i }).click();
    await expect.poll(() => seen).toBe(true);
  });
});
