"use client";

import Link from "next/link";
import { ArrowRight, Check, Eye, Lightbulb, Loader2, Sparkles, X } from "lucide-react";

import { IntelligenceLifecycleBadge, IntelligenceSeverityBadge } from "@/components/intelligence/intelligence-badges";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import type { Insight } from "@/lib/api/types";
import { formatInsightType, isInsightExpiringSoon } from "@/lib/intelligence/utils";
import { formatDate, formatRelativeDate } from "@/lib/formatting/date";
import { useDismissInsight, useMarkInsightSeen } from "@/lib/insights/use-insights";

/** Renders a financial insight with concise explanation and lifecycle controls. */
export function InsightCard({ insight }: { readonly insight: Insight }) {
  const markSeen = useMarkInsightSeen();
  const dismiss = useDismissInsight();
  const pending = markSeen.isPending || dismiss.isPending;
  const expiringSoon = isInsightExpiringSoon(insight.expiresAt);

  return (
    <Card className={`flex h-full min-w-0 flex-col transition-shadow hover:shadow-md ${insight.status === "Active" ? "border-primary/15" : ""}`}>
      <CardHeader className="pb-3">
        <div className="flex items-start gap-3">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
            {insight.type === "UnusualTransaction" ? <Sparkles className="size-5" aria-hidden="true" /> : <Lightbulb className="size-5" aria-hidden="true" />}
          </div>
          <div className="min-w-0 flex-1">
            <div className="flex flex-wrap items-center gap-1.5">
              <IntelligenceSeverityBadge severity={insight.severity} />
              <IntelligenceLifecycleBadge status={insight.status} />
              {expiringSoon ? <span className="rounded-full border border-amber-500/30 bg-amber-500/10 px-2 py-0.5 text-[11px] font-medium text-amber-700 dark:text-amber-300">Expires soon</span> : null}
            </div>
            <CardTitle className="mt-3 line-clamp-2 text-base leading-5">{insight.title}</CardTitle>
            <p className="mt-1 text-xs text-muted-foreground">{formatInsightType(insight.type)}</p>
          </div>
        </div>
      </CardHeader>
      <CardContent className="flex-1 space-y-4">
        <p className="line-clamp-4 text-sm leading-6 text-muted-foreground">{insight.message}</p>
        <div className="rounded-xl border bg-muted/20 p-4">
          <div className="flex items-start gap-3">
            <Eye className="mt-0.5 size-4 shrink-0 text-muted-foreground" aria-hidden="true" />
            <div className="min-w-0 text-xs leading-5">
              <p className="font-medium">Observed {formatRelativeDate(insight.occurredAt)}</p>
              <p className="mt-1 text-muted-foreground">Created {formatDate(insight.createdAt)}</p>
              {insight.expiresAt ? <p className="mt-1 text-muted-foreground">Expires {formatDate(insight.expiresAt)}</p> : null}
            </div>
          </div>
        </div>
      </CardContent>
      <CardFooter className="flex flex-col gap-2 border-t bg-muted/10">
        <Button asChild variant="ghost" className="w-full justify-between">
          <Link href={`/insights/${insight.id}`}>View insight <ArrowRight aria-hidden="true" /></Link>
        </Button>
        <div className="grid w-full grid-cols-2 gap-2">
          {insight.status === "Active" ? (
            <Button variant="outline" size="sm" onClick={() => markSeen.mutate(insight.id)} disabled={pending}>
              {markSeen.isPending ? <Loader2 className="animate-spin" aria-hidden="true" /> : <Check aria-hidden="true" />} Mark as seen
            </Button>
          ) : (
            <span className="inline-flex items-center justify-center rounded-lg border px-3 py-2 text-xs text-muted-foreground">{insight.status}</span>
          )}
          {insight.status !== "Dismissed" && insight.status !== "Expired" ? (
            <Button variant="ghost" size="sm" onClick={() => dismiss.mutate(insight.id)} disabled={pending}>
              {dismiss.isPending ? <Loader2 className="animate-spin" aria-hidden="true" /> : <X aria-hidden="true" />} Dismiss
            </Button>
          ) : null}
        </div>
        {markSeen.isError || dismiss.isError ? <p role="alert" className="w-full text-xs text-destructive">The insight could not be updated. Please try again.</p> : null}
      </CardFooter>
    </Card>
  );
}
