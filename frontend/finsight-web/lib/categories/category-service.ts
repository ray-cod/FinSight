import type { ApiClient } from "@/lib/api/client";
import type { Category } from "@/lib/api/types";

/** Provides the financial classification taxonomy API surface. */
export class CategoryService {
  /** Creates a category service using the supplied API client. */
  public constructor(private readonly api: ApiClient) {}

  /** Gets the active transaction categories and their subcategories. */
  public getCategories(): Promise<readonly Category[]> {
    return this.api.get<readonly Category[]>("/categories");
  }
}
