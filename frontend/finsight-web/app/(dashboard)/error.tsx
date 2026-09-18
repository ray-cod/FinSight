"use client";

import { useEffect } from "react";

import { ErrorState } from "@/components/feedback/error-state";
import { reportClientError } from "@/lib/telemetry/client";

interface DashboardRouteErrorProps {
  readonly error: Error & { digest?: string };
  readonly reset: () => void;
}

/** Provides a recoverable error boundary for authenticated workspace routes. */
export default function DashboardRouteError({ error, reset }: DashboardRouteErrorProps) {
  useEffect(() => {
    reportClientError(error, { source: "dashboard-route.error", digest: error.digest });
  }, [error]);

  return (
    <main className="mx-auto flex min-h-[60vh] w-full max-w-xl items-center justify-center py-12">
      <ErrorState
        title="This workspace could not be loaded"
        description="FinSight encountered an unexpected application error. You can retry without losing the rest of your session."
        onRetry={reset}
        correlationId={error.digest ?? null}
      />
    </main>
  );
}
