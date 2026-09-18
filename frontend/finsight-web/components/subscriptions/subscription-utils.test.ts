import { describe, expect, it } from "vitest";

import type { Subscription } from "@/lib/api/types";
import { formatFrequency, getLatestPriceChange, groupedMonthlyCommitments, isUpcomingWithinDays, monthlyEquivalent } from "@/components/subscriptions/subscription-utils";

function createSubscription(overrides: Partial<Subscription> = {}): Subscription {
  return {
    id: "subscription-1",
    merchantId: "merchant-1",
    merchantName: "Netflix",
    currency: "USD",
    frequency: "Monthly",
    currentAmount: 15,
    averageAmount: 15,
    detectionConfidence: 0.95,
    firstDetectedAt: "2026-01-01T00:00:00Z",
    lastChargeAt: "2026-09-01T00:00:00Z",
    nextExpectedChargeAt: "2099-01-01T00:00:00Z",
    lastPriceChangedAt: null,
    status: "Active",
    ...overrides,
  };
}

describe("subscription utilities", () => {
  it("formats supported billing frequencies for a financial UI", () => {
    expect(formatFrequency("BiWeekly")).toBe("Every 2 weeks");
    expect(formatFrequency("Monthly")).toBe("Monthly");
  });

  it("calculates price movement from the latest observed charges", () => {
    const result = getLatestPriceChange([
      { id: "2", transactionId: "t2", amount: 18, observedAt: "2026-09-01T00:00:00Z" },
      { id: "1", transactionId: "t1", amount: 15, observedAt: "2026-08-01T00:00:00Z" },
    ]);

    expect(result.previous).toBe(15);
    expect(result.current).toBe(18);
    expect(result.percentage).toBe(20);
    expect(result.direction).toBe("increase");
  });

  it("keeps recurring commitment totals separated by currency", () => {
    const totals = groupedMonthlyCommitments([
      createSubscription({ currency: "USD", currentAmount: 15 }),
      createSubscription({ id: "subscription-2", currency: "USD", currentAmount: 25 }),
      createSubscription({ id: "subscription-3", currency: "ZAR", currentAmount: 100 }),
      createSubscription({ id: "subscription-4", currency: "USD", status: "Dismissed", currentAmount: 999 }),
    ]);

    expect(totals.get("USD")).toBe(40);
    expect(totals.get("ZAR")).toBe(100);
  });

  it("normalizes recurring costs to a monthly equivalent", () => {
    expect(monthlyEquivalent(10, "Monthly")).toBe(10);
    expect(monthlyEquivalent(10, "Annual")).toBeCloseTo(10 / 12);
    expect(monthlyEquivalent(10, "Unknown")).toBeNull();
  });

  it("recognizes upcoming active charges within the requested window", () => {
    const upcoming = createSubscription({ nextExpectedChargeAt: new Date(Date.now() + 10 * 86_400_000).toISOString() });
    expect(isUpcomingWithinDays(upcoming, 30)).toBe(true);
    expect(isUpcomingWithinDays(upcoming, 5)).toBe(false);
    expect(isUpcomingWithinDays({ ...upcoming, status: "Inactive" }, 30)).toBe(false);
  });
});
