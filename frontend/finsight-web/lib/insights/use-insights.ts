"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { useAuth } from "@/lib/auth/auth-context";
import { InsightService } from "@/lib/insights/insight-service";
import { queryKeys } from "@/lib/queries/query-keys";

/** Loads financial insights visible to the authenticated user. */
export function useInsights(includeDismissed = true, limit = 500) {
  const { api } = useAuth();
  const service = new InsightService(api);

  return useQuery({
    queryKey: queryKeys.insightsList(includeDismissed, limit),
    queryFn: () => service.getInsights(includeDismissed, limit),
    staleTime: 30_000,
  });
}

/** Loads one user-owned financial insight. */
export function useInsight(insightId: string) {
  const { api } = useAuth();
  const service = new InsightService(api);

  return useQuery({
    queryKey: queryKeys.insight(insightId),
    queryFn: () => service.getInsight(insightId),
    enabled: Boolean(insightId),
    staleTime: 30_000,
  });
}

/** Marks an insight as seen and refreshes affected intelligence views. */
export function useMarkInsightSeen() {
  const { api } = useAuth();
  const service = new InsightService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (insightId: string) => service.markSeen(insightId),
    onSuccess: async (_data, insightId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.insights() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.insight(insightId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.notifications() }),
      ]);
    },
  });
}

/** Dismisses an insight and refreshes affected intelligence views. */
export function useDismissInsight() {
  const { api } = useAuth();
  const service = new InsightService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (insightId: string) => service.dismissInsight(insightId),
    onSuccess: async (_data, insightId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.insights() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.insight(insightId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.notifications() }),
      ]);
    },
  });
}
