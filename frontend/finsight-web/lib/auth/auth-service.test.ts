import { describe, expect, it, vi } from "vitest";

import type { ApiClient } from "@/lib/api/client";
import { AuthService } from "@/lib/auth/auth-service";

function createApiMock() {
  return {
    get: vi.fn(),
    post: vi.fn(),
  } as unknown as ApiClient & {
    get: ReturnType<typeof vi.fn>;
    post: ReturnType<typeof vi.fn>;
  };
}

describe("AuthService session operations", () => {
  it("loads active sessions", async () => {
    const api = createApiMock();
    const service = new AuthService(api);
    api.get.mockResolvedValueOnce([]);

    await expect(service.getSessions()).resolves.toEqual([]);
    expect(api.get).toHaveBeenCalledWith("/auth/sessions");
  });

  it("revokes other active sessions", async () => {
    const api = createApiMock();
    const service = new AuthService(api);
    api.post.mockResolvedValueOnce({ revokedCount: 2 });

    await expect(service.revokeOtherSessions()).resolves.toEqual({ revokedCount: 2 });
    expect(api.post).toHaveBeenCalledWith("/auth/sessions/revoke-others");
  });
});
