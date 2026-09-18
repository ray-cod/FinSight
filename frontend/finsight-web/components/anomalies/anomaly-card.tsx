"use client";

import * as React from "react";
import Link from "next/link";
import { AlertTriangle, ArrowRight, CheckCircle2, Clock3, EyeOff, Loader2 } from "lucide-react";

import { IntelligenceLifecycleBadge, IntelligenceSeverityBadge } from "@/components/intelligence/intelligence-badges";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import type { Anomaly } from "@/lib/api/types";
import { formatAnomalyType, severityClasses } from "@/lib/intelligence/utils";
import { formatRelativeDate } from "@/lib/formatting/date";
import { useDismissAnomaly, useResolveAnomaly } from "@/lib/anomalies/use-anomalies";

/** Renders an anomaly with contextual actions and a route to its full evidence. */
export function AnomalyCard({ anomaly }: { readonly anomaly: Anomaly }) {
  const resolve = useResolveAnomaly();
  const dismiss = useDismissAnomaly();
  const [action, setAction] = React.useState<"resolve" | "dismiss" | null>(null);
  const pending = resolve.isPending || dismiss.isPending;

  const confirm = async () => {
    if (action === "resolve") {
      await resolve.mutateAsync(anomaly.id);
    } else if (action === "dismiss") {
      await dismiss.mutateAsync(anomaly.id);
    }
    setAction(null);
  };

  return (
    <>
      <Card className="flex h-full min-w-0 flex-col transition-shadow hover:shadow-md">
        <CardHeader className="pb-3">
          <div className="flex items-start justify-between gap-3">
            <div className="flex min-w-0 items-center gap-3">
              <div className={`flex size-10 shrink-0 items-center justify-center rounded-xl border ${severityClasses(anomaly.severity)}`}>
                <AlertTriangle className="size-5" aria-hidden="true" />
              </div>
              <div className="min-w-0">
                <CardTitle className="line-clamp-2 text-base leading-5">{anomaly.title}</CardTitle>
                <p className="mt-1 truncate text-xs text-muted-foreground">{formatAnomalyType(anomaly.type)}</p>
              </div>
            </div>
            <div className="flex shrink-0 flex-wrap justify-end gap-1.5">
              <IntelligenceSeverityBadge severity={anomaly.severity} />
              <IntelligenceLifecycleBadge status={anomaly.status} />
            </div>
          </div>
        </CardHeader>
        <CardContent className="flex-1 space-y-4">
          <p className="line-clamp-3 text-sm leading-6 text-muted-foreground">{anomaly.description}</p>
          <div className="grid gap-3 sm:grid-cols-2">
            <Metric label="Detection score" value={`${Math.round(anomaly.score * 100)}%`} />
            <Metric label="Confidence" value={`${Math.round(anomaly.confidence * 100)}%`} />
          </div>
          <div className="flex items-center justify-between gap-3 rounded-lg border bg-muted/20 px-3 py-2 text-xs">
            <span className="flex items-center gap-1.5 text-muted-foreground"><Clock3 className="size-3.5" aria-hidden="true" />Detected</span>
            <span className="font-medium">{formatRelativeDate(anomaly.detectedAt)}</span>
          </div>
        </CardContent>
        <CardFooter className="flex flex-col gap-2 border-t bg-muted/10 sm:flex-row">
          <Button asChild variant="ghost" className="w-full justify-between sm:flex-1">
            <Link href={`/anomalies/${anomaly.id}`}>View evidence <ArrowRight aria-hidden="true" /></Link>
          </Button>
          {anomaly.status === "Open" ? (
            <div className="grid w-full grid-cols-2 gap-2 sm:w-auto sm:grid-cols-none sm:flex">
              <Button variant="outline" size="sm" onClick={() => setAction("resolve")} disabled={pending} aria-label={`Resolve ${anomaly.title}`}>
                <CheckCircle2 aria-hidden="true" /> Resolve
              </Button>
              <Button variant="ghost" size="sm" onClick={() => setAction("dismiss")} disabled={pending} aria-label={`Dismiss ${anomaly.title}`}>
                <EyeOff aria-hidden="true" /> Dismiss
              </Button>
            </div>
          ) : null}
        </CardFooter>
      </Card>

      <Dialog open={action !== null} onOpenChange={(open) => { if (!open && !pending) setAction(null); }}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{action === "resolve" ? "Resolve anomaly?" : "Dismiss anomaly?"}</DialogTitle>
            <DialogDescription>
              {action === "resolve"
                ? "Resolving records that you have reviewed so this signal no longer appears as open."
                : "Dismissed signals remain in your history but are removed from the default open-anomaly view."}
            </DialogDescription>
          </DialogHeader>
          {resolve.isError || dismiss.isError ? (
            <p role="alert" className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
              The anomaly could not be updated. Please try again.
            </p>
          ) : null}
          <DialogFooter>
            <Button variant="outline" onClick={() => setAction(null)} disabled={pending}>Cancel</Button>
            <Button variant={action === "dismiss" ? "destructive" : "default"} onClick={() => void confirm()} disabled={pending}>
              {pending ? <><Loader2 className="animate-spin" aria-hidden="true" /> Updating…</> : action === "resolve" ? "Resolve anomaly" : "Dismiss anomaly"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

function Metric({ label, value }: { readonly label: string; readonly value: string }) {
  return (
    <div className="rounded-lg border p-3">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="mt-1 text-lg font-semibold tabular-nums">{value}</p>
    </div>
  );
}

