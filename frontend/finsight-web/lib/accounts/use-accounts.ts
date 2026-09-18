"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { useAuth } from "@/lib/auth/auth-context";
import { AccountService } from "@/lib/accounts/account-service";
import { queryKeys } from "@/lib/queries/query-keys";

/** Loads the current user's financial accounts. */
export function useAccounts() {
  const { api } = useAuth();
  const service = new AccountService(api);

  return useQuery({
    queryKey: queryKeys.accounts(),
    queryFn: () => service.getAccounts(),
    staleTime: 30_000,
  });
}

/** Loads the current user's active bank connections. */
export function useAccountConnections() {
  const { api } = useAuth();
  const service = new AccountService(api);

  return useQuery({
    queryKey: queryKeys.accountConnections(),
    queryFn: () => service.getConnections(),
    staleTime: 30_000,
  });
}

/** Loads supported active financial institutions. */
export function useInstitutions() {
  const { api } = useAuth();
  const service = new AccountService(api);

  return useQuery({
    queryKey: queryKeys.institutions(),
    queryFn: () => service.getInstitutions(),
    staleTime: 5 * 60_000,
  });
}

/** Loads one owned account. */
export function useAccount(accountId: string) {
  const { api } = useAuth();
  const service = new AccountService(api);

  return useQuery({
    queryKey: queryKeys.account(accountId),
    queryFn: () => service.getAccount(accountId),
    enabled: Boolean(accountId),
    staleTime: 30_000,
  });
}

/** Loads recent transactions attached to one account. */
export function useAccountTransactions(accountId: string, limit = 50) {
  const { api } = useAuth();
  const service = new AccountService(api);

  return useQuery({
    queryKey: queryKeys.accountTransactions(accountId, limit),
    queryFn: () => service.getAccountTransactions(accountId, limit),
    enabled: Boolean(accountId),
    staleTime: 15_000,
  });
}

/** Provides the account connection mutation workflow. */
export function useConnectAccount() {
  const { api } = useAuth();
  const queryClient = useQueryClient();
  const service = new AccountService(api);

  return useMutation({
    mutationFn: async (institutionCode: string) => {
      const connectionId = await service.connect(institutionCode);
      const sync = await service.sync(connectionId);
      return { connectionId, ...sync };
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.accounts() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.accountConnections() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
      ]);
    },
  });
}

/** Provides the immediate synchronization mutation workflow. */
export function useSyncAccount() {
  const { api } = useAuth();
  const queryClient = useQueryClient();
  const service = new AccountService(api);

  return useMutation({
    mutationFn: (connectionId: string) => service.sync(connectionId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.accounts() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.accountConnections() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
      ]);
    },
  });
}

/** Provides the bank connection disconnection mutation workflow. */
export function useDisconnectAccount() {
  const { api } = useAuth();
  const queryClient = useQueryClient();
  const service = new AccountService(api);

  return useMutation({
    mutationFn: (connectionId: string) => service.disconnect(connectionId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.accounts() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.accountConnections() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
      ]);
    },
  });
}
