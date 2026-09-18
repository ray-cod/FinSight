import { describe, expect, it, vi } from "vitest";

import { NotificationService } from "@/lib/notifications/notification-service";

import type { ApiClient } from "@/lib/api/client";

function createApiMock() {
  return {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
  } as unknown as ApiClient & {
    get: ReturnType<typeof vi.fn>;
    post: ReturnType<typeof vi.fn>;
    put: ReturnType<typeof vi.fn>;
  };
}

describe("NotificationService", () => {
  it("requests a bounded notification list with read state", async () => {
    const api = createApiMock();
    const service = new NotificationService(api);
    vi.mocked(api.get).mockResolvedValue([]);

    await service.getNotifications(true, 100);

    expect(api.get).toHaveBeenCalledWith("/notifications", {
      query: { includeRead: true, limit: 100 },
    });
  });

  it("dismisses an in-app notification through its lifecycle endpoint", async () => {
    const api = createApiMock();
    const service = new NotificationService(api);
    api.post.mockResolvedValueOnce(undefined);

    await service.dismiss("notification-1");

    expect(api.post).toHaveBeenCalledWith("/notifications/notification-1/dismiss");
  });

  it("reads the dedicated unread count endpoint", async () => {
    const api = createApiMock();
    const service = new NotificationService(api);
    api.get.mockResolvedValueOnce({ count: 4 });

    await expect(service.getUnreadCount()).resolves.toEqual({ count: 4 });
    expect(api.get).toHaveBeenCalledWith("/notifications/unread-count");
  });

  it("updates the complete preference document", async () => {
    const api = createApiMock();
    const service = new NotificationService(api);
    vi.mocked(api.put).mockResolvedValue(undefined);

    const preferences = {
      inAppEnabled: true,
      emailEnabled: true,
      anomalyNotificationsEnabled: false,
      subscriptionNotificationsEnabled: true,
      insightNotificationsEnabled: false,
    };

    await service.updatePreferences(preferences);

    expect(api.put).toHaveBeenCalledWith("/notifications/preferences", preferences);
  });
});
