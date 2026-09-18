"use client";

import { AlertTriangle, Copy, RefreshCw } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

interface ErrorStateProps {
  /** Heading presented to the user. */
  readonly title?: string;
  /** Human-readable recovery guidance. */
  readonly description?: string;
  /** Optional retry action. */
  readonly onRetry?: () => void;
  /** Optional technical correlation identifier from the API or client runtime. */
  readonly correlationId?: string | null;
}

/** Displays a consistent recoverable error state with optional support diagnostics. */
export function ErrorState({ title = "Something went wrong", description = "We could not load this area of FinSight. Please try again.", onRetry, correlationId }: ErrorStateProps) {
  const [copied, setCopied] = useState(false);
  const copyCorrelationId = async () => {
    if (!correlationId || !navigator.clipboard) return;
    try {
      await navigator.clipboard.writeText(correlationId);
      setCopied(true);
    } catch {
      setCopied(false);
    }
    window.setTimeout(() => setCopied(false), 1600);
  };

  return (
    <Card role="alert" className="border-destructive/25 bg-destructive/[0.03]">
      <CardHeader>
        <div className="flex size-10 items-center justify-center rounded-full bg-destructive/10 text-destructive">
          <AlertTriangle className="size-5" aria-hidden="true" />
        </div>
        <CardTitle>{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      {(onRetry || correlationId) ? (
        <CardContent className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div className="min-w-0">
            {correlationId ? <p className="text-xs text-muted-foreground" aria-live="polite">Reference ID: <code className="font-mono text-foreground/80">{correlationId}</code></p> : null}
            {copied ? <p className="mt-1 text-xs text-foreground">Reference ID copied.</p> : null}
          </div>
          <div className="flex shrink-0 flex-wrap gap-2">
            {onRetry ? <Button variant="outline" onClick={onRetry}><RefreshCw className="size-4" aria-hidden="true" />Try again</Button> : null}
            {correlationId ? <Button type="button" variant="ghost" size="sm" onClick={() => void copyCorrelationId()}><Copy className="size-4" aria-hidden="true" />Copy reference</Button> : null}
          </div>
        </CardContent>
      ) : null}
    </Card>
  );
}
