"use client";

import { useToastManager } from "@/components/ui/toast";

interface ToastOptions {
  readonly title: string;
  readonly description?: string;
}

/** Provides consistent success/error/info feedback for product mutations. */
export function useAppToast() {
  const manager = useToastManager();

  return {
    success({ title, description }: ToastOptions) {
      return manager.add({ title, description, type: "success" });
    },
    error({ title, description }: ToastOptions) {
      return manager.add({ title, description, type: "error", priority: "high" });
    },
    info({ title, description }: ToastOptions) {
      return manager.add({ title, description, type: "info" });
    },
  };
}
