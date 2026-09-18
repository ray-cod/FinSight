"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { useAuth } from "@/lib/auth/auth-context";
import { queryKeys } from "@/lib/queries/query-keys";
import { TransactionService } from "@/lib/transactions/transaction-service";
import type { TransactionListQuery } from "@/lib/api/types";

/** Loads a filtered transaction page for the authenticated user. */
export function useTransactions(query: TransactionListQuery) {
  const { api } = useAuth();
  const service = new TransactionService(api);

  return useQuery({
    queryKey: [...queryKeys.transactions(), query] as const,
    queryFn: () => service.getTransactions(query),
    staleTime: 15_000,
    placeholderData: (previous) => previous,
  });
}

/** Loads one user-owned transaction. */
export function useTransaction(transactionId: string) {
  const { api } = useAuth();
  const service = new TransactionService(api);

  return useQuery({
    queryKey: queryKeys.transaction(transactionId),
    queryFn: () => service.getTransaction(transactionId),
    enabled: Boolean(transactionId),
    staleTime: 30_000,
  });
}

/** Updates a transaction classification and invalidates affected financial views. */
export function useCorrectTransactionClassification() {
  const { api } = useAuth();
  const queryClient = useQueryClient();
  const service = new TransactionService(api);

  return useMutation({
    mutationFn: ({ transactionId, categoryId, subcategoryId }: {
      readonly transactionId: string;
      readonly categoryId: string;
      readonly subcategoryId?: string | null;
    }) => service.correctClassification(transactionId, { categoryId, subcategoryId }),
    onSuccess: async (_data, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.transaction(variables.transactionId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.transactions() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
      ]);
    },
  });
}
