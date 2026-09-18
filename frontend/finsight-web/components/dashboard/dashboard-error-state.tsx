"use client";

import { ErrorState } from "@/components/feedback/error-state";

interface DashboardErrorStateProps {
  /** Retry callback for the failed dashboard/resource request. */
  readonly onRetry: () => void;
  /** Optional normalized API correlation identifier. */
  readonly correlationId?: string | null;
  /** Optional workspace-specific heading. */
  readonly title?: string;
  /** Optional workspace-specific recovery guidance. */
  readonly description?: string;
}

/** Renders the shared application error treatment for financial data workspaces. */
export function DashboardErrorState({ onRetry, correlationId, title, description }: DashboardErrorStateProps) {
  return (
    <ErrorState
      title={title ?? "We could not load your financial overview"}
      description={description ?? "FinSight could not retrieve the latest financial data. Your account and existing cached data remain protected."}
      onRetry={onRetry}
      correlationId={correlationId}
    />
  );
}
