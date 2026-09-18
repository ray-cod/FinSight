import Link from "next/link";
import { ArrowRight, CalendarClock, CircleDollarSign, ShieldCheck, TrendingDown, TrendingUp } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import type { Subscription } from "@/lib/api/types";
import { formatCurrency } from "@/lib/formatting/currency";
import { formatDate } from "@/lib/formatting/date";
import { SubscriptionStatusBadge } from "@/components/subscriptions/subscription-status-badge";
import { formatFrequency, getNextChargeState, getFrequencyDescription, monthlyEquivalent } from "@/components/subscriptions/subscription-utils";

/** Renders one recurring subscription with its most useful financial signals. */
export function SubscriptionCard({ subscription }: { readonly subscription: Subscription }) {
  const nextCharge = getNextChargeState(subscription.nextExpectedChargeAt);
  const monthly = monthlyEquivalent(subscription.currentAmount, subscription.frequency);
  const confidence = Math.round(subscription.detectionConfidence * 100);

  return (
    <Card className="flex h-full min-w-0 flex-col transition-shadow hover:shadow-md">
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 items-center gap-3">
            <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
              <CircleDollarSign className="size-5" aria-hidden="true" />
            </div>
            <div className="min-w-0">
              <CardTitle className="truncate text-base">{subscription.merchantName}</CardTitle>
              <CardDescription className="mt-1">{formatFrequency(subscription.frequency)}</CardDescription>
            </div>
          </div>
          <SubscriptionStatusBadge status={subscription.status} />
        </div>
      </CardHeader>

      <CardContent className="flex-1 space-y-5">
        <div className="flex items-end justify-between gap-4">
          <div>
            <p className="text-2xl font-semibold tracking-tight tabular-nums">
              {formatCurrency(subscription.currentAmount, subscription.currency)}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">{subscription.frequency === "BiWeekly" ? "every 2 weeks" : subscription.frequency === "SemiAnnual" ? "twice a year" : `${formatFrequency(subscription.frequency).toLocaleLowerCase()}`}</p>
          </div>
          {subscription.lastPriceChangedAt ? (
            <div className="text-right text-destructive">
              <div className="flex items-center justify-end gap-1 text-sm font-medium">
                <TrendingUp className="size-3.5" aria-hidden="true" />
                Price changed
              </div>
              <p className="text-[11px]">Detected {formatDate(subscription.lastPriceChangedAt)}</p>
            </div>
          ) : null}
        </div>

        {monthly !== null ? (
          <div className="rounded-lg border bg-muted/20 p-3">
            <p className="text-xs text-muted-foreground">Estimated monthly commitment</p>
            <p className="mt-1 font-semibold tabular-nums">{formatCurrency(monthly, subscription.currency)}</p>
          </div>
        ) : (
          <div className="rounded-lg border border-dashed p-3 text-sm text-muted-foreground">
            FinSight is still establishing a predictable cadence for this subscription.
          </div>
        )}

        <div className="space-y-2">
          <div className="flex items-center justify-between gap-3 text-xs">
            <span className="flex items-center gap-2 text-muted-foreground">
              <ShieldCheck className="size-3.5" aria-hidden="true" />
              Detection confidence
            </span>
            <span className="font-medium tabular-nums">{confidence}%</span>
          </div>
          <Progress value={confidence} aria-label={`Detection confidence ${confidence}%`}>
            <span className="sr-only">{confidence}% confidence</span>
          </Progress>
        </div>

        <div className="grid gap-3 sm:grid-cols-2">
          <div className="rounded-lg border p-3">
            <p className="text-xs text-muted-foreground">Last charge</p>
            <p className="mt-1 text-sm font-medium">{formatDate(subscription.lastChargeAt)}</p>
          </div>
          <div className={nextCharge.urgent ? "rounded-lg border border-destructive/20 bg-destructive/5 p-3" : "rounded-lg border p-3"}>
            <p className="flex items-center gap-1.5 text-xs text-muted-foreground"><CalendarClock className="size-3.5" aria-hidden="true" />Next charge</p>
            <p className={nextCharge.urgent ? "mt-1 text-sm font-semibold text-destructive" : "mt-1 text-sm font-medium"}>{nextCharge.label}</p>
            <p className="mt-0.5 text-[11px] text-muted-foreground">{nextCharge.detail}</p>
          </div>
        </div>

        <p className="text-xs leading-5 text-muted-foreground">{getFrequencyDescription(subscription.frequency)}</p>

        {subscription.lastPriceChangedAt ? (
          <div className="flex items-center justify-between gap-3 rounded-lg border border-destructive/15 bg-destructive/5 px-3 py-2 text-xs">
            <span className="text-muted-foreground">Last price-change signal</span>
            <span className="font-medium text-destructive">{formatDate(subscription.lastPriceChangedAt)}</span>
          </div>
        ) : null}

        <p className="text-[11px] text-muted-foreground">Detected {formatDate(subscription.firstDetectedAt)}</p>
      </CardContent>

      <CardFooter className="border-t bg-muted/10">
        <Button asChild variant="ghost" className="w-full justify-between">
          <Link href={`/subscriptions/${subscription.id}`}>
            View subscription details
            <ArrowRight aria-hidden="true" />
          </Link>
        </Button>
      </CardFooter>
    </Card>
  );
}
