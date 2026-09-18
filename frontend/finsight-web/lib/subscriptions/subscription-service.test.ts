import { describe, expect, it, vi } from "vitest";

import type { ApiClient } from "@/lib/api/client";
import { SubscriptionService } from "@/lib/subscriptions/subscription-service";

function createApiMock() {
  return {
    get: vi.fn(),
    post: vi.fn(),
  } as unknown as ApiClient & {
    get: ReturnType<typeof vi.fn>;
    post: ReturnType<typeof vi.fn>;
  };
}

describe("SubscriptionService", () => {
  it("loads the authenticated user's subscription records including dismissed findings", async () => {
    const api = createApiMock();
    const service = new SubscriptionService(api);
    api.get.mockResolvedValueOnce([]);

    await expect(service.getSubscriptions()).resolves.toEqual([]);
    expect(api.get).toHaveBeenCalledWith("/subscriptions", { query: { includeDismissed: true } });
  });

  it("loads subscription detail and bounded price history", async () => {
    const api = createApiMock();
    const service = new SubscriptionService(api);
    api.get.mockResolvedValueOnce({ id: "subscription-1" });
    api.get.mockResolvedValueOnce([{ id: "observation-1", amount: 15.99 }]);

    await expect(service.getSubscription("subscription-1")).resolves.toEqual({ id: "subscription-1" });
    await expect(service.getPriceHistory("subscription-1", 24)).resolves.toEqual([{ id: "observation-1", amount: 15.99 }]);

    expect(api.get).toHaveBeenNthCalledWith(1, "/subscriptions/subscription-1");
    expect(api.get).toHaveBeenNthCalledWith(2, "/subscriptions/subscription-1/price-history", { query: { limit: 24 } });
  });

  it("dismisses a subscription through the explicit lifecycle command", async () => {
    const api = createApiMock();
    const service = new SubscriptionService(api);
    api.post.mockResolvedValueOnce(undefined);

    await expect(service.dismissSubscription("subscription-1")).resolves.toBeUndefined();
    expect(api.post).toHaveBeenCalledWith("/subscriptions/subscription-1/dismiss");
  });
});
