"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { useAuth } from "@/lib/auth/auth-context";
import { queryKeys } from "@/lib/queries/query-keys";
import { AnomalyService } from "@/lib/anomalies/anomaly-service";

/** Loads anomaly intelligence visible to the authenticated user. */
export function useAnomalies(includeResolved = true, limit = 500) {
  const { api } = useAuth();
  const service = new AnomalyService(api);

  return useQuery({
    queryKey: queryKeys.anomaliesList(includeResolved, limit),
    queryFn: () => service.getAnomalies(includeResolved, limit),
    staleTime: 30_000,
  });
}

/** Loads one user-owned anomaly. */
export function useAnomaly(anomalyId: string) {
  const { api } = useAuth();
  const service = new AnomalyService(api);

  return useQuery({
    queryKey: queryKeys.anomaly(anomalyId),
    queryFn: () => service.getAnomaly(anomalyId),
    enabled: Boolean(anomalyId),
    staleTime: 30_000,
  });
}

/** Resolves an anomaly and refreshes related financial intelligence views. */
export function useResolveAnomaly() {
  const { api } = useAuth();
  const service = new AnomalyService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (anomalyId: string) => service.resolveAnomaly(anomalyId),
    onSuccess: async (_data, anomalyId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.anomalies() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.anomaly(anomalyId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.insights() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.notifications() }),
      ]);
    },
  });
}

/** Dismisses an anomaly and refreshes related financial intelligence views. */
export function useDismissAnomaly() {
  const { api } = useAuth();
  const service = new AnomalyService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (anomalyId: string) => service.dismissAnomaly(anomalyId),
    onSuccess: async (_data, anomalyId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.anomalies() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.anomaly(anomalyId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.insights() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.notifications() }),
      ]);
    },
  });
}
