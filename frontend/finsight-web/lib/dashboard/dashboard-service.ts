import type { ApiClient } from "@/lib/api/client";
import type { DashboardResponse } from "@/lib/api/types";

/** Provides the dashboard read model through the authenticated FinSight API client. */
export class DashboardService {
  /** Creates a dashboard service using the supplied API client. */
  public constructor(private readonly api: ApiClient) {}

  /** Gets the current dashboard overview. */
  public getOverview(): Promise<DashboardResponse> {
    return this.api.get<DashboardResponse>("/dashboard");
  }
}
