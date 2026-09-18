import { describe, expect, it } from "vitest";

import { formatAnomalyType, formatInsightType, humanizeToken, isInsightExpiringSoon, severityRank } from "@/lib/intelligence/utils";

describe("intelligence utilities", () => {
  it("humanizes backend enum tokens", () => {
    expect(humanizeToken("MerchantSpendingSpike")).toBe("Merchant Spending Spike");
    expect(formatAnomalyType("NewMerchant")).toBe("New merchant");
    expect(formatInsightType("PossibleDuplicate")).toBe("Possible duplicate");
  });

  it("orders severity consistently", () => {
    expect(severityRank("Critical")).toBeGreaterThan(severityRank("High"));
    expect(severityRank("High")).toBeGreaterThan(severityRank("Medium"));
  });

  it("identifies insights that expire within one week", () => {
    const now = new Date("2026-09-17T00:00:00Z");
    expect(isInsightExpiringSoon("2026-09-21T00:00:00Z", now)).toBe(true);
    expect(isInsightExpiringSoon("2026-10-01T00:00:00Z", now)).toBe(false);
  });
});
