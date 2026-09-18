import { expect, test, type Page } from "@playwright/test";

const user = {
  userId: "0e0b7e2f-9dd4-4a90-9c43-9ee9d6bc2f50",
  email: "alex@example.com",
  displayName: "Alex Morgan",
  emailConfirmed: true,
  createdAt: "2026-01-15T10:00:00Z",
};

const activeSessions = [
  {
    sessionId: "11111111-1111-1111-1111-111111111111",
    createdAt: "2026-09-17T12:00:00Z",
    expiresAt: "2026-10-17T12:00:00Z",
    ipAddress: "192.168.1.12",
    isCurrent: true,
  },
  {
    sessionId: "22222222-2222-2222-2222-222222222222",
    createdAt: "2026-09-16T09:00:00Z",
    expiresAt: "2026-10-16T09:00:00Z",
    ipAddress: "192.168.1.20",
    isCurrent: false,
  },
];

async function authenticate(page: Page) {
  await page.addInitScript(() => {
    sessionStorage.setItem("finsight.access-token", "phase9-access-token");
    sessionStorage.setItem("finsight.refresh-token", "phase9-refresh-token");
  });

  await page.route("**/api/v1/auth/refresh", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        accessToken: "phase9-access-token",
        refreshToken: "phase9-refresh-token",
        accessTokenExpiresAt: "2099-01-01T00:00:00Z",
        userId: user.userId,
        sessionId: activeSessions[0].sessionId,
      }),
    });
  });

  await page.route("**/api/v1/users/me", async (route) => {
    await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(user) });
  });
}

test.describe("profile and security settings", () => {
  test("users can update their display name", async ({ page }) => {
    await authenticate(page);

    let displayName = user.displayName;
    await page.route("**/api/v1/users/me", async (route) => {
      if (route.request().method() === "PATCH") {
        const body = route.request().postDataJSON() as { displayName: string };
        displayName = body.displayName;
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify({ ...user, displayName }),
        });
        return;
      }
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ ...user, displayName }) });
    });

    await page.goto("/settings?section=profile");
    await expect(page.getByRole("heading", { name: "Profile", exact: true })).toBeVisible();

    const displayNameInput = page.getByLabel("Display name");
    await displayNameInput.fill("Alex FinSight");
    await page.getByRole("button", { name: "Save profile" }).click();

    await expect(page.getByText("Profile updated successfully.")).toBeVisible();
    await expect(displayNameInput).toHaveValue("Alex FinSight");
  });

  test("users can review sessions and revoke other sessions", async ({ page }) => {
    await authenticate(page);

    let sessions = [...activeSessions];
    await page.route("**/api/v1/auth/sessions", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify(sessions) });
    });
    await page.route("**/api/v1/auth/sessions/revoke-others", async (route) => {
      sessions = [sessions[0]];
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ revokedCount: 1 }) });
    });
    await page.route("**/api/v1/security/activity?**", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([
        { eventType: "LoginSucceeded", occurredAt: "2026-09-17T12:00:00Z" },
        { eventType: "ProfileUpdated", occurredAt: "2026-09-17T11:30:00Z" },
      ]) });
    });

    await page.goto("/settings?section=security");
    await expect(page.getByRole("heading", { name: "Active sessions" })).toBeVisible();
    await expect(page.getByText("Current session", { exact: true })).toBeVisible();
    await expect(page.getByText("Active session", { exact: true })).toBeVisible();

    await page.getByRole("button", { name: "Sign out other sessions" }).first().click();
    await expect(page.getByRole("heading", { name: "Sign out other sessions?" })).toBeVisible();
    await page.getByRole("button", { name: "Sign out other sessions" }).last().click();

    await expect(page.getByText("Active session", { exact: true })).toHaveCount(0);
    await expect(page.getByText("Other sessions revoked")).toBeVisible();
  });

  test("changing a password signs the browser session out", async ({ page }) => {
    await authenticate(page);

    await page.route("**/api/v1/auth/change-password", async (route) => {
      await route.fulfill({ status: 204 });
    });
    await page.route("**/api/v1/security/activity?**", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([]) });
    });
    await page.route("**/api/v1/auth/sessions", async (route) => {
      await route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify([]) });
    });

    await page.goto("/settings?section=security");
    await page.getByLabel("Current password").fill("CurrentPassword!234");
    await page.getByLabel("New password").fill("NewPassword!234");
    await page.getByLabel("Confirm new password").fill("NewPassword!234");
    await page.getByRole("button", { name: "Change password" }).click();

    await expect(page).toHaveURL(/\/login\?reason=password-changed$/);
    await expect(page.getByText(/previous sessions were signed out/i)).toBeVisible();
  });
});
