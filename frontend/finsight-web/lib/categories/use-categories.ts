"use client";

import { useQuery } from "@tanstack/react-query";

import { CategoryService } from "@/lib/categories/category-service";
import { useAuth } from "@/lib/auth/auth-context";
import { queryKeys } from "@/lib/queries/query-keys";

/** Loads the active transaction classification taxonomy. */
export function useCategories() {
  const { api } = useAuth();
  const service = new CategoryService(api);

  return useQuery({
    queryKey: queryKeys.categories(),
    queryFn: () => service.getCategories(),
    staleTime: 10 * 60_000,
  });
}
