import { describe, expect, it } from "vitest";

import { getAuthErrorMessage } from "@/lib/auth/auth-context";
import { ApiError } from "@/lib/errors/api-error";

describe("authentication error mapping", () => {
  it("maps duplicate-account failures to a useful message", () => {
    expect(getAuthErrorMessage(new ApiError("An account with this email already exists.", 500))).toBe(
      "An account with this email already exists.",
    );
  });

  it("maps API availability failures", () => {
    expect(getAuthErrorMessage(new ApiError("network", 0))).toBe(
      "FinSight could not be reached. Check that the API is running.",
    );
  });
});
