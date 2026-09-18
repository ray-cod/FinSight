import type { ApiClient } from "@/lib/api/client";
import type { Anomaly } from "@/lib/api/types";

/** Provides the user-owned financial anomaly API surface. */
export class AnomalyService {
  /** Creates an anomaly service using the supplied API client. */
  public constructor(private readonly api: ApiClient) {}

  /** Gets anomalies visible to the authenticated user. */
  public getAnomalies(includeResolved = true, limit = 500): Promise<readonly Anomaly[]> {
    return this.api.get<readonly Anomaly[]>("/anomalies", {
      query: { includeResolved, limit },
    });
  }

  /** Gets one user-owned anomaly. */
  public getAnomaly(anomalyId: string): Promise<Anomaly> {
    return this.api.get<Anomaly>(`/anomalies/${anomalyId}`);
  }

  /** Resolves a user-owned anomaly. */
  public async resolveAnomaly(anomalyId: string): Promise<void> {
    await this.api.post<void>(`/anomalies/${anomalyId}/resolve`);
  }

  /** Dismisses a user-owned anomaly. */
  public async dismissAnomaly(anomalyId: string): Promise<void> {
    await this.api.post<void>(`/anomalies/${anomalyId}/dismiss`);
  }
}
