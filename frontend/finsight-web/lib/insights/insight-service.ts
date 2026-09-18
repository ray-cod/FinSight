import type { ApiClient } from "@/lib/api/client";
import type { Insight } from "@/lib/api/types";

/** Provides the user-owned financial insight API surface. */
export class InsightService {
  /** Creates an insight service using the supplied API client. */
  public constructor(private readonly api: ApiClient) {}

  /** Gets insights visible to the authenticated user. */
  public getInsights(includeDismissed = true, limit = 500): Promise<readonly Insight[]> {
    return this.api.get<readonly Insight[]>("/insights", {
      query: { includeDismissed, limit },
    });
  }

  /** Gets one user-owned insight. */
  public getInsight(insightId: string): Promise<Insight> {
    return this.api.get<Insight>(`/insights/${insightId}`);
  }

  /** Marks a user-owned insight as seen. */
  public async markSeen(insightId: string): Promise<void> {
    await this.api.post<void>(`/insights/${insightId}/seen`);
  }

  /** Dismisses a user-owned insight. */
  public async dismissInsight(insightId: string): Promise<void> {
    await this.api.post<void>(`/insights/${insightId}/dismiss`);
  }
}
