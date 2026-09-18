"use client";

import * as React from "react";

import { reportClientError } from "@/lib/telemetry/client";

/** Captures uncaught browser errors and rejected promises for centralized client diagnostics. */
export function ClientRuntimeTelemetry() {
  React.useEffect(() => {
    const handleError = (event: ErrorEvent) => {
      reportClientError(event.error ?? event.message, { source: "window.error" });
    };
    const handleRejection = (event: PromiseRejectionEvent) => {
      reportClientError(event.reason, { source: "unhandledrejection" });
    };

    window.addEventListener("error", handleError);
    window.addEventListener("unhandledrejection", handleRejection);
    return () => {
      window.removeEventListener("error", handleError);
      window.removeEventListener("unhandledrejection", handleRejection);
    };
  }, []);

  return null;
}
