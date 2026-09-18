import { describe, expect, it, vi } from "vitest";

import type { ApiClient } from "@/lib/api/client";
import { DashboardService } from "@/lib/dashboard/dashboard-service";

function createApiMock(): ApiClient {
  return {
    get: vi.fn(),
  } as unknown as ApiClient;
}

describe("DashboardService", () => {
  it("requests the aggregated dashboard endpoint", async () => {
    const api = createApiMock();
    vi.mocked(api.get).mockResolvedValue({ balances: [] });

    const service = new DashboardService(api);
    await service.getOverview();

    expect(api.get).toHaveBeenCalledWith("/dashboard");
  });
});
