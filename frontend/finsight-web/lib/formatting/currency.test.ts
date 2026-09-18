import { describe, expect, it } from "vitest";

import { formatCurrency } from "@/lib/formatting/currency";

describe("formatCurrency", () => {
  it("formats an ISO currency amount", () => {
    expect(formatCurrency(1250.5, "USD", "en-US")).toBe("$1,250.50");
  });
});
