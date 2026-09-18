"use client";

import { useQuery } from "@tanstack/react-query";

import { queryKeys } from "@/lib/queries/query-keys";
import { DashboardService } from "@/lib/dashboard/dashboard-service";
import { useAuth } from "@/lib/auth/auth-context";

/** Loads the authenticated user's dashboard read model with a short freshness window. */
export function useDashboard() {
  const { api, status } = useAuth();
  const service = new DashboardService(api);

  return useQuery({
    queryKey: queryKeys.dashboard(),
    queryFn: () => service.getOverview(),
    enabled: status === "authenticated",
    staleTime: 30_000,
    refetchOnWindowFocus: false,
  });
}
