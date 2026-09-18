import { describe, expect, it, vi } from "vitest";

import type { ApiClient } from "@/lib/api/client";
import { TransactionService } from "@/lib/transactions/transaction-service";

function createApiMock() {
  return {
    get: vi.fn(),
    put: vi.fn(),
  } as unknown as ApiClient & {
    get: ReturnType<typeof vi.fn>;
    put: ReturnType<typeof vi.fn>;
  };
}

describe("TransactionService", () => {
  it("requests paginated transaction results with the provided server-side filters", async () => {
    const api = createApiMock();
    const service = new TransactionService(api);
    const query = { page: 2, pageSize: 50, search: "amazon", type: "Purchase" } as const;
    api.get.mockResolvedValueOnce({ items: [], page: 2 });

    await expect(service.getTransactions(query)).resolves.toEqual({ items: [], page: 2 });
    expect(api.get).toHaveBeenCalledWith("/transactions", { query });
  });

  it("loads a contextual transaction detail resource", async () => {
    const api = createApiMock();
    const service = new TransactionService(api);
    api.get.mockResolvedValueOnce({ id: "transaction-1" });

    await expect(service.getTransaction("transaction-1")).resolves.toEqual({ id: "transaction-1" });
    expect(api.get).toHaveBeenCalledWith("/transactions/transaction-1");
  });

  it("sends an explicit classification correction", async () => {
    const api = createApiMock();
    const service = new TransactionService(api);
    api.put.mockResolvedValueOnce(undefined);

    await expect(service.correctClassification("transaction-1", { categoryId: "category-1", subcategoryId: "subcategory-1" })).resolves.toBeUndefined();
    expect(api.put).toHaveBeenCalledWith("/transactions/transaction-1/classification", { categoryId: "category-1", subcategoryId: "subcategory-1" });
  });
});
