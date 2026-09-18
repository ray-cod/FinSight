import { afterEach, describe, expect, it } from "vitest";

import { tokenStore } from "@/lib/auth/token-store";

afterEach(() => {
  tokenStore.clear();
});

describe("tokenStore", () => {
  it("stores and reads an authentication token pair for the browser session", () => {
    tokenStore.setTokens("access-token", "refresh-token");

    expect(tokenStore.getAccessToken()).toBe("access-token");
    expect(tokenStore.getRefreshToken()).toBe("refresh-token");
  });

  it("can rotate only the access token", () => {
    tokenStore.setTokens("access-token", "refresh-token");
    tokenStore.setAccessToken("rotated-access-token");

    expect(tokenStore.getAccessToken()).toBe("rotated-access-token");
    expect(tokenStore.getRefreshToken()).toBe("refresh-token");
  });

  it("clears both tokens", () => {
    tokenStore.setTokens("access-token", "refresh-token");
    tokenStore.clear();

    expect(tokenStore.getAccessToken()).toBeNull();
    expect(tokenStore.getRefreshToken()).toBeNull();
  });
});
