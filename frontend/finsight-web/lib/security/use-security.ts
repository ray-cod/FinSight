"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { useAuth } from "@/lib/auth/auth-context";
import { AuthService } from "@/lib/auth/auth-service";
import { SecurityService } from "@/lib/security/security-service";
import { queryKeys } from "@/lib/queries/query-keys";

/** Loads the authenticated user's active sessions. */
export function useSecuritySessions() {
  const { api } = useAuth();
  const service = new AuthService(api);

  return useQuery({
    queryKey: queryKeys.securitySessions(),
    queryFn: () => service.getSessions(),
    staleTime: 15_000,
  });
}

/** Revokes all active sessions other than the current session. */
export function useRevokeOtherSessions() {
  const { api } = useAuth();
  const service = new AuthService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => service.revokeOtherSessions(),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.securitySessions() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.security() }),
      ]);
    },
  });
}

/** Loads the user's recent security activity without exposing audit metadata. */
export function useSecurityActivity(limit = 25) {
  const { api } = useAuth();
  const service = new SecurityService(api);

  return useQuery({
    queryKey: queryKeys.securityActivity(limit),
    queryFn: () => service.getActivity(limit),
    staleTime: 30_000,
  });
}
