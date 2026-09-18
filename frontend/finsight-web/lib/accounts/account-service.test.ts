import { describe, expect, it, vi } from "vitest";

import { AccountService } from "@/lib/accounts/account-service";
import type { ApiClient } from "@/lib/api/client";

function createApiMock() {
  return {
    get: vi.fn(),
    post: vi.fn(),
    delete: vi.fn(),
  } as unknown as ApiClient & {
    get: ReturnType<typeof vi.fn>;
    post: ReturnType<typeof vi.fn>;
    delete: ReturnType<typeof vi.fn>;
  };
}

describe("AccountService", () => {
  it("uses the versioned account resources through the shared API client", async () => {
    const api = createApiMock();
    const service = new AccountService(api);
    const accounts = [{ id: "account-1" }];
    api.get.mockResolvedValueOnce(accounts);

    await expect(service.getAccounts()).resolves.toEqual(accounts);
    expect(api.get).toHaveBeenCalledWith("/accounts");
  });

  it("connects a bank institution using the provider code", async () => {
    const api = createApiMock();
    const service = new AccountService(api);
    api.post.mockResolvedValueOnce("connection-1");

    await expect(service.connect("MOCK_BANK")).resolves.toBe("connection-1");
    expect(api.post).toHaveBeenCalledWith("/accounts/connections", { institutionCode: "MOCK_BANK" });
  });

  it("synchronizes and disconnects an owned connection through the corresponding commands", async () => {
    const api = createApiMock();
    const service = new AccountService(api);
    api.post.mockResolvedValueOnce({ importedTransactions: 4 });
    api.delete.mockResolvedValueOnce(undefined);

    await expect(service.sync("connection-1")).resolves.toEqual({ importedTransactions: 4 });
    await expect(service.disconnect("connection-1")).resolves.toBeUndefined();

    expect(api.post).toHaveBeenCalledWith("/accounts/connections/connection-1/sync");
    expect(api.delete).toHaveBeenCalledWith("/accounts/connections/connection-1");
  });
});
