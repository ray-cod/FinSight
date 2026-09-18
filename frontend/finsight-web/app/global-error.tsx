"use client";

import { useEffect } from "react";

import { ErrorState } from "@/components/feedback/error-state";
import { reportClientError } from "@/lib/telemetry/client";

import "./globals.css";

interface GlobalErrorProps {
  readonly error: Error & { digest?: string };
  readonly reset: () => void;
}

/** Renders the last-resort recovery boundary for root layout failures. */
export default function GlobalError({ error, reset }: GlobalErrorProps) {
  useEffect(() => {
    reportClientError(error, { source: "global-error", digest: error.digest });
  }, [error]);

  return (
    <html lang="en">
      <body className="min-h-screen bg-background text-foreground">
        <main className="flex min-h-screen items-center justify-center px-4 py-16 sm:px-6">
          <div className="w-full max-w-xl">
            <div className="mb-6 text-center">
              <p className="text-sm font-semibold tracking-tight">FinSight</p>
              <p className="mt-1 text-xs text-muted-foreground">Financial intelligence workspace</p>
            </div>
            <ErrorState
              title="FinSight needs to recover"
              description="A critical application error prevented the workspace from rendering. Retry the application before continuing."
              onRetry={reset}
              correlationId={error.digest ?? null}
            />
          </div>
        </main>
      </body>
    </html>
  );
}
