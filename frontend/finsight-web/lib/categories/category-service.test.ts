import { describe, expect, it, vi } from "vitest";

import type { ApiClient } from "@/lib/api/client";
import { CategoryService } from "@/lib/categories/category-service";

function createApiMock() {
  return { get: vi.fn() } as unknown as ApiClient & { get: ReturnType<typeof vi.fn> };
}

describe("CategoryService", () => {
  it("loads the active classification taxonomy from the versioned categories resource", async () => {
    const api = createApiMock();
    const service = new CategoryService(api);
    api.get.mockResolvedValueOnce([]);

    await expect(service.getCategories()).resolves.toEqual([]);
    expect(api.get).toHaveBeenCalledWith("/categories");
  });
});
