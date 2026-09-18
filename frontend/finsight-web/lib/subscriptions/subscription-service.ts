import type { ApiClient } from "@/lib/api/client";
import type { Subscription, SubscriptionPriceObservation } from "@/lib/api/types";

/** Provides the user-owned subscription intelligence API surface. */
export class SubscriptionService {
  /** Creates a subscription service using the supplied API client. */
  public constructor(private readonly api: ApiClient) {}

  /** Gets subscriptions visible to the authenticated user. */
  public getSubscriptions(includeDismissed = true): Promise<readonly Subscription[]> {
    return this.api.get<readonly Subscription[]>("/subscriptions", {
      query: { includeDismissed },
    });
  }

  /** Gets one user-owned subscription. */
  public getSubscription(subscriptionId: string): Promise<Subscription> {
    return this.api.get<Subscription>(`/subscriptions/${subscriptionId}`);
  }

  /** Gets recent observed prices for a user-owned subscription. */
  public getPriceHistory(subscriptionId: string, limit = 24): Promise<readonly SubscriptionPriceObservation[]> {
    return this.api.get<readonly SubscriptionPriceObservation[]>(
      `/subscriptions/${subscriptionId}/price-history`,
      { query: { limit } },
    );
  }

  /** Dismisses a detected subscription for the authenticated user. */
  public async dismissSubscription(subscriptionId: string): Promise<void> {
    await this.api.post<void>(`/subscriptions/${subscriptionId}/dismiss`);
  }
}
