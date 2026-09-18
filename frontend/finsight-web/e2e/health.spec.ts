import { expect, test } from "@playwright/test";

test("production web health endpoint is available", async ({ request }) => {
  const response = await request.get("/api/health");

  expect(response.ok()).toBe(true);
  await expect(response.json()).resolves.toMatchObject({
    status: "ok",
    service: "finsight-web",
  });
});
