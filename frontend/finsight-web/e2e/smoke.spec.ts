import { expect, test } from "@playwright/test";

const currentUser = {
  userId: "0e0b7e2f-9dd4-4a90-9c43-9ee9d6bc2f50",
  email: "alex@example.com",
  displayName: "Alex Morgan",
  emailConfirmed: false,
  createdAt: "2026-01-15T10:00:00Z",
};

test("unauthenticated users are redirected to sign in", async ({ page }) => {
  await page.goto("/dashboard");

  await expect(page).toHaveURL(/\/login\?returnUrl=%2Fdashboard$/);
  await expect(page.getByRole("heading", { name: "Sign in to FinSight" })).toBeVisible();
  await expect(page.getByLabel("Email")).toBeVisible();
  await expect(page.getByLabel("Password")).toBeVisible();
});

test("users can sign in and reach the protected workspace", async ({ page }) => {
  await page.route("**/api/v1/auth/login", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        accessToken: "access-token",
        refreshToken: "refresh-token",
        accessTokenExpiresAt: new Date(Date.now() + 15 * 60_000).toISOString(),
        userId: currentUser.userId,
        sessionId: "11111111-1111-1111-1111-111111111111",
      }),
    });
  });

  await page.route("**/api/v1/users/me", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(currentUser),
    });
  });

  await page.route("**/api/v1/dashboard", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        primaryCurrency: "USD",
        balances: [
          { currency: "USD", currentBalance: 16780.42, availableBalance: 16710.42, accountCount: 2 },
        ],
        spendingTrend: [
          { label: "Apr", startDate: "2026-04-01T00:00:00Z", endDate: "2026-05-01T00:00:00Z", income: 0, spending: 0, netCashFlow: 0 },
          { label: "May", startDate: "2026-05-01T00:00:00Z", endDate: "2026-06-01T00:00:00Z", income: 0, spending: 0, netCashFlow: 0 },
          { label: "Jun", startDate: "2026-06-01T00:00:00Z", endDate: "2026-07-01T00:00:00Z", income: 0, spending: 0, netCashFlow: 0 },
          { label: "Jul", startDate: "2026-07-01T00:00:00Z", endDate: "2026-08-01T00:00:00Z", income: 0, spending: 0, netCashFlow: 0 },
          { label: "Aug", startDate: "2026-08-01T00:00:00Z", endDate: "2026-09-01T00:00:00Z", income: 0, spending: 0, netCashFlow: 0 },
          { label: "Sep", startDate: "2026-09-01T00:00:00Z", endDate: "2026-10-01T00:00:00Z", income: 4200, spending: 786.17, netCashFlow: 3413.83 },
        ],
        categorySpending: [],
        recentTransactions: [],
        upcomingSubscriptions: [],
        priorityAlerts: [],
        metrics: {
          currentMonthSpending: 786.17,
          previousMonthSpending: 0,
          spendingChangePercentage: 100,
          currentMonthIncome: 4200,
          activeSubscriptionCount: 0,
          openAnomalyCount: 0,
          activeInsightCount: 0,
          unreadNotificationCount: 0,
        },
      }),
    });
  });

  await page.goto("/login?returnUrl=%2Fdashboard");
  await page.getByLabel("Email").fill(currentUser.email);
  await page.getByLabel("Password").fill("SecurePassword1!");
  await page.getByRole("button", { name: "Sign in" }).click();

  await expect(page).toHaveURL(/\/dashboard$/);
  await expect(page.getByRole("heading", { name: /good to see you, alex/i })).toBeVisible();
  await expect(page.getByText("This month's spending")).toBeVisible();
  await expect(page.getByRole("button", { name: "Open account menu" })).toBeVisible();
  await expect(page.getByText("Alex Morgan").first()).toBeVisible();
});

test("registration presents backend-compatible password requirements", async ({ page }) => {
  await page.goto("/register");

  await expect(page.getByRole("heading", { name: /start with a clearer financial picture/i })).toBeVisible();
  await expect(page.getByText("12 or more characters")).toBeVisible();
  await expect(page.getByText("Uppercase, lowercase and a number")).toBeVisible();
  await expect(page.getByText("At least one special character")).toBeVisible();

  await page.getByLabel("Name").fill("Alex Morgan");
  await page.getByLabel("Email").fill("not-an-email");
  await page.getByLabel("Password").fill("weak");
  await page.getByLabel("Confirm password").fill("weak");
  await page.getByRole("button", { name: "Create account" }).click();

  await expect(page.getByText("Enter a valid email address.")).toBeVisible();
  await expect(page.getByText(/Use 12\+ characters/)).toBeVisible();
});

test("forgot-password page keeps account existence private", async ({ page }) => {
  await page.route("**/api/v1/auth/forgot-password", async (route) => {
    await route.fulfill({ status: 202 });
  });

  await page.goto("/forgot-password");
  await page.getByLabel("Email").fill("alex@example.com");
  await page.getByRole("button", { name: "Send reset instructions" }).click();

  await expect(page.getByRole("heading", { name: "Check your email" })).toBeVisible();
  await expect(page.getByText(/does not reveal whether the account exists/i)).toBeVisible();
});

test("users can connect, synchronize and review a financial account", async ({ page }) => {
  await page.route("**/api/v1/auth/login", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        accessToken: "access-token",
        refreshToken: "refresh-token",
        accessTokenExpiresAt: new Date(Date.now() + 15 * 60_000).toISOString(),
        userId: currentUser.userId,
        sessionId: "11111111-1111-1111-1111-111111111111",
      }),
    });
  });

  await page.route("**/api/v1/users/me", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(currentUser) });
  });

  let connected = false;
  const connectionId = "11111111-1111-1111-1111-111111111111";
  const accountId = "22222222-2222-2222-2222-222222222222";

  await page.route("**/api/v1/institutions", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([{ id: "33333333-3333-3333-3333-333333333333", providerCode: "MOCK_BANK", name: "FinSight Demo Bank", isActive: true }]),
    });
  });

  await page.route("**/api/v1/accounts/connections", async (route) => {
    if (route.request().method() === "POST") {
      connected = true;
      await route.fulfill({ status: 201, contentType: "application/json", body: JSON.stringify(connectionId) });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(connected ? [{
        id: connectionId,
        institutionId: "33333333-3333-3333-3333-333333333333",
        providerCode: "MOCK_BANK",
        institutionName: "FinSight Demo Bank",
        status: "Connected",
        lastSuccessfulSyncAt: new Date().toISOString(),
        lastSyncAttemptAt: new Date().toISOString(),
        lastSyncError: null,
        accountCount: 1,
      }] : []),
    });
  });

  await page.route("**/api/v1/accounts", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(connected ? [{
        id: accountId,
        connectionId,
        institutionId: "33333333-3333-3333-3333-333333333333",
        name: "Everyday Checking",
        type: "Checking",
        currency: "USD",
        currentBalance: 12540.25,
        availableBalance: 12390.25,
        status: "Active",
      }] : []),
    });
  });

  await page.route(`**/api/v1/accounts/${accountId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        id: accountId,
        connectionId,
        institutionId: "33333333-3333-3333-3333-333333333333",
        name: "Everyday Checking",
        type: "Checking",
        currency: "USD",
        currentBalance: 12540.25,
        availableBalance: 12390.25,
        status: "Active",
      }),
    });
  });

  await page.route(`**/api/v1/accounts/${accountId}/transactions*`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([{
        id: "44444444-4444-4444-4444-444444444444",
        providerTransactionId: "provider-1",
        rawDescription: "AMZN Mktp US",
        normalizedDescription: "Amazon",
        merchantId: null,
        categoryId: null,
        subcategoryId: null,
        amount: -68.32,
        currency: "USD",
        transactionDate: new Date().toISOString(),
        type: "Purchase",
        status: "Imported",
        classificationStatus: "Classified",
        classificationSource: "Rule",
        classificationConfidence: 0.99,
        classifiedAt: new Date().toISOString(),
      }]),
    });
  });

  await page.route(`**/api/v1/accounts/connections/${connectionId}/sync`, async (route) => {
    connected = true;
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ importedTransactions: 12 }) });
  });

  await page.goto("/login?returnUrl=%2Faccounts");
  await page.getByLabel("Email").fill(currentUser.email);
  await page.getByLabel("Password").fill("SecurePassword1!");
  await page.getByRole("button", { name: "Sign in" }).click();

  await expect(page).toHaveURL(/\/accounts$/);
  await expect(page.getByRole("heading", { name: /your connected money/i })).toBeVisible();
  await expect(page.getByRole("heading", { name: "No accounts connected yet" })).toBeVisible();

  await page.getByRole("button", { name: /connect an account/i }).first().click();
  await page.getByLabel("Institution").selectOption("MOCK_BANK");
  await page.getByRole("button", { name: "Connect & sync" }).click();

  await expect(page.getByText("Account connected")).toBeVisible();
  await expect(page.getByText(/imported 12 new transactions/i)).toBeVisible();
  await page.getByRole("button", { name: "Done" }).click();

  await expect(page.getByRole("heading", { name: "Everyday Checking" })).toBeVisible();
  await expect(page.getByText("FinSight Demo Bank").first()).toBeVisible();

  await page.getByRole("link", { name: /view account/i }).click();
  await expect(page).toHaveURL(new RegExp(`/accounts/${accountId}$`));
  await expect(page.getByRole("heading", { name: "Everyday Checking" })).toBeVisible();
  await expect(page.getByText("Amazon")).toBeVisible();
});

test("users can search, filter and correct a transaction classification", async ({ page }) => {
  const transactionId = "55555555-5555-5555-5555-555555555555";
  const accountId = "66666666-6666-6666-6666-666666666666";
  const categoryId = "77777777-7777-7777-7777-777777777777";
  const subcategoryId = "88888888-8888-8888-8888-888888888888";
  const alternativeCategoryId = "99999999-9999-9999-9999-999999999999";
  const transactionDate = "2026-09-16T10:00:00Z";

  await page.route("**/api/v1/auth/login", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        accessToken: "access-token",
        refreshToken: "refresh-token",
        accessTokenExpiresAt: new Date(Date.now() + 15 * 60_000).toISOString(),
        userId: currentUser.userId,
        sessionId: "11111111-1111-1111-1111-111111111111",
      }),
    });
  });

  await page.route("**/api/v1/users/me", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(currentUser) });
  });

  await page.route("**/api/v1/accounts", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([{
        id: accountId,
        connectionId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        institutionId: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        name: "Everyday Checking",
        type: "Checking",
        currency: "USD",
        currentBalance: 12540.25,
        availableBalance: 12390.25,
        status: "Active",
      }]),
    });
  });

  await page.route("**/api/v1/categories", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([
        {
          id: categoryId,
          code: "SHOPPING",
          name: "Shopping",
          type: "Expense",
          subcategories: [{ id: subcategoryId, code: "ONLINE", name: "Online shopping" }],
        },
        {
          id: alternativeCategoryId,
          code: "DINING",
          name: "Dining",
          type: "Expense",
          subcategories: [],
        },
      ]),
    });
  });

  await page.route("**/api/v1/transactions?*", async (route) => {
    const url = new URL(route.request().url());
    const search = url.searchParams.get("search");
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        items: search && search.toLowerCase() === "amazon" ? [{
          id: transactionId,
          accountId,
          accountName: "Everyday Checking",
          institutionName: "FinSight Demo Bank",
          merchantName: "Amazon",
          rawDescription: "AMZN Mktp US",
          normalizedDescription: "Amazon",
          categoryId,
          categoryName: "Shopping",
          subcategoryId,
          subcategoryName: "Online shopping",
          amount: -68.32,
          currency: "USD",
          transactionDate,
          type: "Purchase",
          status: "Imported",
          classificationStatus: "Classified",
          classificationSource: "Rule",
          classificationConfidence: 0.99,
          classifiedAt: transactionDate,
        }] : [],
        page: 1,
        pageSize: 25,
        totalCount: search && search.toLowerCase() === "amazon" ? 1 : 0,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      }),
    });
  });

  await page.route(`**/api/v1/transactions/${transactionId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        id: transactionId,
        accountId,
        accountName: "Everyday Checking",
        institutionId: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        institutionName: "FinSight Demo Bank",
        providerTransactionId: "provider-amazon-1",
        rawDescription: "AMZN Mktp US",
        normalizedDescription: "Amazon",
        merchantId: "cccccccc-cccc-cccc-cccc-cccccccccccc",
        merchantName: "Amazon",
        categoryId,
        categoryName: "Shopping",
        subcategoryId,
        subcategoryName: "Online shopping",
        amount: -68.32,
        currency: "USD",
        transactionDate,
        importedAt: "2026-09-16T10:05:00Z",
        type: "Purchase",
        status: "Imported",
        classificationStatus: "Classified",
        classificationSource: "Rule",
        classificationConfidence: 0.99,
        classifiedAt: transactionDate,
        userCorrectedAt: null,
      }),
    });
  });

  let corrected = false;
  await page.route(`**/api/v1/transactions/${transactionId}/classification`, async (route) => {
    expect(route.request().method()).toBe("PUT");
    corrected = true;
    await route.fulfill({ status: 204 });
  });

  await page.goto("/login?returnUrl=%2Ftransactions");
  await page.getByLabel("Email").fill(currentUser.email);
  await page.getByLabel("Password").fill("SecurePassword1!");
  await page.getByRole("button", { name: "Sign in" }).click();

  await expect(page).toHaveURL(/\/transactions$/);
  await page.getByLabel("Search transactions").fill("Amazon");
  await expect(page.getByText("Amazon").first()).toBeVisible();
  await expect(page.getByText("68.32").first()).toBeVisible();

  await page.getByRole("link", { name: /Amazon/i }).first().click();
  await expect(page).toHaveURL(new RegExp(`/transactions/${transactionId}$`));
  await expect(page.getByRole("heading", { name: "Amazon" })).toBeVisible();
  await expect(page.getByText("Rule-based classification")).toBeVisible();

  await page.getByRole("button", { name: /edit classification/i }).click();
  await page.getByText("Correct transaction classification").first().waitFor();
  await page.getByLabel("Category").last().selectOption(alternativeCategoryId);
  await page.getByRole("button", { name: /save correction/i }).click();

  await expect.poll(() => corrected).toBe(true);
});
