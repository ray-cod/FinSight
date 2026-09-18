import { describe, expect, it, vi } from "vitest";

import { AnomalyService } from "@/lib/anomalies/anomaly-service";
import type { ApiClient } from "@/lib/api/client";

const anomaly = {
  id: "anomaly-1",
  transactionId: "tx-1",
  accountId: "account-1",
  type: "LargeTransaction",
  severity: "High",
  score: 0.92,
  confidence: 0.96,
  title: "Unusually large purchase",
  description: "The purchase is materially above your normal spend.",
  evidence: "Current charge: 920.00; typical charge: 180.00.",
  detectedAt: "2026-09-10T10:00:00Z",
  status: "Open",
  resolvedAt: null,
} as const;

describe("AnomalyService", () => {
  it("requests user anomalies with the expected visibility controls", async () => {
    const get = vi.fn().mockResolvedValue([anomaly]);
    const service = new AnomalyService({ get } as unknown as ApiClient);

    await expect(service.getAnomalies(false, 100)).resolves.toEqual([anomaly]);
    expect(get).toHaveBeenCalledWith("/anomalies", { query: { includeResolved: false, limit: 100 } });
  });

  it("executes anomaly lifecycle actions", async () => {
    const post = vi.fn().mockResolvedValue(undefined);
    const service = new AnomalyService({ post } as unknown as ApiClient);

    await service.resolveAnomaly("anomaly-1");
    await service.dismissAnomaly("anomaly-2");

    expect(post).toHaveBeenNthCalledWith(1, "/anomalies/anomaly-1/resolve");
    expect(post).toHaveBeenNthCalledWith(2, "/anomalies/anomaly-2/dismiss");
  });
});
