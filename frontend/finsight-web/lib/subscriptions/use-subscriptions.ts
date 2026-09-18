"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { useAuth } from "@/lib/auth/auth-context";
import { queryKeys } from "@/lib/queries/query-keys";
import { SubscriptionService } from "@/lib/subscriptions/subscription-service";

/** Loads all non-hidden subscription intelligence records for the authenticated user. */
export function useSubscriptions() {
  const { api } = useAuth();
  const service = new SubscriptionService(api);

  return useQuery({
    queryKey: [...queryKeys.subscriptions(), "all"] as const,
    queryFn: () => service.getSubscriptions(true),
    staleTime: 30_000,
  });
}

/** Loads one user-owned subscription. */
export function useSubscription(subscriptionId: string) {
  const { api } = useAuth();
  const service = new SubscriptionService(api);

  return useQuery({
    queryKey: queryKeys.subscription(subscriptionId),
    queryFn: () => service.getSubscription(subscriptionId),
    enabled: Boolean(subscriptionId),
    staleTime: 30_000,
  });
}

/** Loads recent observed subscription prices. */
export function useSubscriptionPriceHistory(subscriptionId: string, limit = 24) {
  const { api } = useAuth();
  const service = new SubscriptionService(api);

  return useQuery({
    queryKey: queryKeys.subscriptionPriceHistory(subscriptionId, limit),
    queryFn: () => service.getPriceHistory(subscriptionId, limit),
    enabled: Boolean(subscriptionId),
    staleTime: 60_000,
  });
}

/** Dismisses a subscription and refreshes affected financial views. */
export function useDismissSubscription() {
  const { api } = useAuth();
  const service = new SubscriptionService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (subscriptionId: string) => service.dismissSubscription(subscriptionId),
    onSuccess: async (_data, subscriptionId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.subscription(subscriptionId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
      ]);
    },
  });
}
