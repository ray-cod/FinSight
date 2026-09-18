import { describe, expect, it, vi } from "vitest";

import { SecurityService } from "@/lib/security/security-service";

function createClient() {
  return {
    get: vi.fn(),
  } as never;
}

describe("SecurityService", () => {
  it("requests user-scoped recent security activity with a bounded limit", async () => {
    const client = createClient() as { get: ReturnType<typeof vi.fn> };
    const service = new SecurityService(client as never);
    client.get.mockResolvedValue([]);

    await service.getActivity(25);

    expect(client.get).toHaveBeenCalledWith("/security/activity", {
      query: { limit: 25 },
    });
  });
});
