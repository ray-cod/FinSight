import type { SecurityActivityItem } from "@/lib/api/types";
import type { ApiClient } from "@/lib/api/client";

/** Provides user-scoped security activity API operations. */
export class SecurityService {
  private readonly client: ApiClient;

  /** Creates a security service for the authenticated API client. */
  public constructor(client: ApiClient) {
    this.client = client;
  }

  /** Retrieves recent security activity. */
  public getActivity(limit = 25): Promise<SecurityActivityItem[]> {
    return this.client.get<SecurityActivityItem[]>("/security/activity", {
      query: { limit },
    });
  }
}
