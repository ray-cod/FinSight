import { describe, expect, it, vi } from "vitest";

import { InsightService } from "@/lib/insights/insight-service";
import type { ApiClient } from "@/lib/api/client";

const insight = {
  id: "insight-1",
  anomalyId: "anomaly-1",
  transactionId: "tx-1",
  type: "UnusualTransaction",
  severity: "High",
  title: "Unusually large purchase",
  message: "This purchase is materially above your recent spending baseline.",
  occurredAt: "2026-09-10T10:00:00Z",
  expiresAt: "2026-10-10T10:00:00Z",
  status: "Active",
  createdAt: "2026-09-10T10:01:00Z",
} as const;

describe("InsightService", () => {
  it("requests user insights with the expected visibility controls", async () => {
    const get = vi.fn().mockResolvedValue([insight]);
    const service = new InsightService({ get } as unknown as ApiClient);

    await expect(service.getInsights(false, 75)).resolves.toEqual([insight]);
    expect(get).toHaveBeenCalledWith("/insights", { query: { includeDismissed: false, limit: 75 } });
  });

  it("executes insight lifecycle actions", async () => {
    const post = vi.fn().mockResolvedValue(undefined);
    const service = new InsightService({ post } as unknown as ApiClient);

    await service.markSeen("insight-1");
    await service.dismissInsight("insight-2");

    expect(post).toHaveBeenNthCalledWith(1, "/insights/insight-1/seen");
    expect(post).toHaveBeenNthCalledWith(2, "/insights/insight-2/dismiss");
  });
});
