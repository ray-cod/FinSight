"use client";

import { useEffect } from "react";

import { ErrorState } from "@/components/feedback/error-state";
import { reportClientError } from "@/lib/telemetry/client";

interface GlobalErrorPageProps {
  readonly error: Error & { digest?: string };
  readonly reset: () => void;
}

export default function ErrorPage({ error, reset }: GlobalErrorPageProps) {
  useEffect(() => {
    reportClientError(error, { source: "app.error", digest: error.digest });
  }, [error]);

  return (
    <main className="flex min-h-screen items-center justify-center px-6 py-16">
      <div className="w-full max-w-lg">
        <ErrorState
          title="FinSight could not render this page"
          description="An unexpected application error occurred. Retry the page before continuing."
          onRetry={reset}
          correlationId={error.digest ?? null}
        />
      </div>
    </main>
  );
}
