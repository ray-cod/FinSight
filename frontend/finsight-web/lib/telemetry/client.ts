import { clientEnv } from "@/lib/config/env";

interface ClientErrorContext {
  readonly source?: string;
  readonly digest?: string;
  readonly path?: string;
}

interface ClientErrorPayload {
  readonly name: string;
  readonly message: string;
  readonly source: string;
  readonly digest?: string;
  readonly path: string;
  readonly correlationId: string;
  readonly timestamp: string;
}

function createCorrelationId(): string {
  if (typeof crypto !== "undefined" && "randomUUID" in crypto) return crypto.randomUUID();
  return `client-${Date.now()}-${Math.random().toString(36).slice(2)}`;
}

function sanitizeMessage(message: string): string {
  return message.replace(/\s+/g, " ").trim().slice(0, 500);
}

function normalizeError(error: unknown): { name: string; message: string } {
  if (error instanceof Error) return { name: error.name, message: sanitizeMessage(error.message) };
  if (typeof error === "string") return { name: "Error", message: sanitizeMessage(error) };
  try {
    return { name: "UnknownError", message: sanitizeMessage(JSON.stringify(error)) };
  } catch {
    return { name: "UnknownError", message: "An unknown client error occurred." };
  }
}

/** Reports a browser runtime error to the configured client telemetry sink without blocking recovery. */
export function reportClientError(error: unknown, context: ClientErrorContext = {}): ClientErrorPayload {
  const normalized = normalizeError(error);
  const payload: ClientErrorPayload = {
    ...normalized,
    source: context.source ?? "unknown",
    ...(context.digest ? { digest: context.digest } : {}),
    path: context.path ?? (typeof window !== "undefined" ? window.location.pathname : "unknown"),
    correlationId: createCorrelationId(),
    timestamp: new Date().toISOString(),
  };

  if (clientEnv.NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT && typeof window !== "undefined") {
    const body = JSON.stringify(payload);
    try {
      if (typeof navigator.sendBeacon === "function") {
        navigator.sendBeacon(
          clientEnv.NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT,
          new Blob([body], { type: "application/json" }),
        );
      } else {
        void fetch(clientEnv.NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body,
          keepalive: true,
        });
      }
    } catch {
      // Diagnostics must never interfere with the product error path.
    }
  }

  if (process.env.NODE_ENV !== "production") {
    console.error("[FinSight client error]", payload);
  }

  return payload;
}
