"use client";

import * as React from "react";
import Link from "next/link";
import { ArrowLeft, CalendarClock, CircleDollarSign, ExternalLink as ExternalArrow, Loader2, ShieldCheck, TrendingDown, TrendingUp, XCircle, type LucideIcon } from "lucide-react";
import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { DashboardErrorState } from "@/components/dashboard/dashboard-error-state";
import { SubscriptionStatusBadge } from "@/components/subscriptions/subscription-status-badge";
import { formatCurrency } from "@/lib/formatting/currency";
import { formatDate, formatRelativeDate } from "@/lib/formatting/date";
import type { SubscriptionPriceObservation } from "@/lib/api/types";
import { useDismissSubscription, useSubscription, useSubscriptionPriceHistory } from "@/lib/subscriptions/use-subscriptions";
import { formatFrequency, getChangeLabel, getFrequencyDescription, getLatestPriceChange, getNextChargeState, annualEquivalent, monthlyEquivalent } from "@/components/subscriptions/subscription-utils";
import { useAppToast } from "@/components/feedback/use-app-toast";

function PriceHistoryChart({ history, currency }: { readonly history: readonly SubscriptionPriceObservation[]; readonly currency: string }) {
  const data = [...history]
    .sort((a, b) => new Date(a.observedAt).getTime() - new Date(b.observedAt).getTime())
    .map((observation) => ({
      date: formatDate(observation.observedAt),
      amount: observation.amount,
    }));

  if (data.length < 2) {
    return (
      <div className="flex min-h-64 items-center justify-center rounded-xl border border-dashed bg-muted/10 px-6 text-center">
        <div>
          <TrendingUp className="mx-auto size-5 text-muted-foreground" aria-hidden="true" />
          <p className="mt-3 text-sm font-medium">Not enough history to chart a trend</p>
          <p className="mt-1 max-w-md text-xs leading-5 text-muted-foreground">FinSight needs at least two observed charges before it can show a meaningful price movement.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-[320px] w-full" aria-label="Subscription price history chart">
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart data={data} margin={{ top: 12, right: 8, left: -20, bottom: 0 }}>
          <defs>
            <linearGradient id="subscription-price-fill" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopOpacity={0.2} />
              <stop offset="100%" stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid vertical={false} strokeDasharray="3 3" className="stroke-border" />
          <XAxis dataKey="date" tickLine={false} axisLine={false} tick={{ fontSize: 11 }} minTickGap={24} />
          <YAxis tickLine={false} axisLine={false} tick={{ fontSize: 11 }} tickFormatter={(value) => `${currency} ${value}`} />
          <Tooltip
            content={({ active, payload, label }) => {
              if (!active || !payload?.length) return null;
              return (
                <div className="rounded-lg border bg-popover px-3 py-2 text-xs shadow-lg">
                  <p className="font-medium">{label}</p>
                  <p className="mt-1 text-muted-foreground">Observed charge</p>
                  <p className="mt-0.5 font-semibold tabular-nums">{formatCurrency(Number(payload[0]?.value ?? 0), currency)}</p>
                </div>
              );
            }}
          />
          <Area type="monotone" dataKey="amount" stroke="currentColor" fill="url(#subscription-price-fill)" className="text-foreground" strokeWidth={2} />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}

function LoadingDetail() {
  return (
    <div className="space-y-8" aria-busy="true">
      <Skeleton className="h-5 w-28" />
      <div className="space-y-2"><Skeleton className="h-9 w-80" /><Skeleton className="h-4 w-[36rem] max-w-full" /></div>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{Array.from({ length: 4 }, (_, index) => <Skeleton key={index} className="h-28 rounded-xl" />)}</div>
      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.6fr)_minmax(300px,0.8fr)]"><Skeleton className="h-[430px] rounded-xl" /><Skeleton className="h-[430px] rounded-xl" /></div>
    </div>
  );
}

/** Renders detailed subscription intelligence and observed price history. */
export function SubscriptionDetail({ subscriptionId }: { readonly subscriptionId: string }) {
  const subscription = useSubscription(subscriptionId);
  const history = useSubscriptionPriceHistory(subscriptionId, 24);
  const dismiss = useDismissSubscription();
  const [dismissOpen, setDismissOpen] = React.useState(false);
  const toast = useAppToast();

  if (subscription.isLoading) return <LoadingDetail />;
  if (subscription.isError || !subscription.data) return <DashboardErrorState title="Subscription unavailable" description="FinSight could not load this recurring commitment. Please try again or return to your subscriptions." onRetry={() => void subscription.refetch()} />;

  const data = subscription.data;
  const observations = history.data ?? [];
  const priceChange = getLatestPriceChange(observations);
  const nextCharge = getNextChargeState(data.nextExpectedChargeAt);
  const monthly = monthlyEquivalent(data.currentAmount, data.frequency);
  const annual = annualEquivalent(data.currentAmount, data.frequency);
  const confidence = Math.round(data.detectionConfidence * 100);
  const hasComparableChange = priceChange.percentage !== null && priceChange.previous !== null;
  const priceChangeIsIncrease = hasComparableChange && (priceChange.percentage ?? 0) > 0;
  const PriceChangeIcon = !hasComparableChange ? CircleDollarSign : priceChangeIsIncrease ? TrendingUp : TrendingDown;

  const handleDismiss = async () => {
    try {
      await dismiss.mutateAsync(data.id);
      toast.success({ title: "Subscription dismissed", description: "The recurring detection will no longer appear in the active subscription view." });
      setDismissOpen(false);
    } catch (error) {
      toast.error({ title: "Subscription could not be dismissed", description: error instanceof Error ? error.message : "Please try again." });
    }
  };

  return (
    <div className="space-y-8">
      <section className="space-y-5">
        <Button asChild variant="ghost" size="sm" className="-ml-2">
          <Link href="/subscriptions"><ArrowLeft aria-hidden="true" /> Back to subscriptions</Link>
        </Button>
        <div className="flex flex-col justify-between gap-5 lg:flex-row lg:items-end">
          <div className="min-w-0 space-y-3">
            <div className="flex flex-wrap items-center gap-2"><SubscriptionStatusBadge status={data.status} /><Badge variant="outline">{data.currency}</Badge></div>
            <div>
              <h2 className="text-2xl font-semibold tracking-tight sm:text-3xl">{data.merchantName}</h2>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground">FinSight detected this recurring commitment from transaction history. Review the cadence, price movement and next expected charge below.</p>
            </div>
          </div>
          {data.status !== "Dismissed" ? (
            <Button variant="outline" onClick={() => setDismissOpen(true)} disabled={dismiss.isPending}>
              <XCircle aria-hidden="true" /> Dismiss detection
            </Button>
          ) : null}
        </div>
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-label="Subscription summary">
        <DetailCard icon={CircleDollarSign} label="Current charge" value={formatCurrency(data.currentAmount, data.currency)} detail={formatFrequency(data.frequency)} />
        <DetailCard icon={CalendarClock} label="Next expected charge" value={nextCharge.label} detail={data.nextExpectedChargeAt ? formatDate(data.nextExpectedChargeAt) : nextCharge.detail} />
        <DetailCard icon={ShieldCheck} label="Detection confidence" value={`${confidence}%`} detail="Strength of the recurring pattern" />
        <DetailCard icon={PriceChangeIcon} label="Latest price movement" value={priceChange.percentage === null ? "No comparison" : `${priceChange.percentage > 0 ? "+" : ""}${priceChange.percentage.toFixed(1)}%`} detail={hasComparableChange ? getChangeLabel(priceChange, data.currency) : "Need another observation"} />
      </section>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.6fr)_minmax(300px,0.8fr)]">
        <Card className="min-w-0">
          <CardHeader>
            <CardTitle>Price history</CardTitle>
            <CardDescription>Observed charges that FinSight has associated with this recurring pattern.</CardDescription>
          </CardHeader>
          <CardContent>
            {history.isLoading ? <Skeleton className="h-[320px] rounded-xl" /> : history.isError ? <p className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">Price history could not be loaded. The subscription itself is still available.</p> : <PriceHistoryChart history={observations} currency={data.currency} />}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Commitment profile</CardTitle>
            <CardDescription>{getFrequencyDescription(data.frequency)}</CardDescription>
          </CardHeader>
          <CardContent className="space-y-5">
            <ProfileRow label="Billing frequency" value={formatFrequency(data.frequency)} />
            <ProfileRow label="Average observed charge" value={formatCurrency(data.averageAmount, data.currency)} />
            <ProfileRow label="Estimated monthly commitment" value={monthly === null ? "Not available" : formatCurrency(monthly, data.currency)} />
            <ProfileRow label="Estimated annual commitment" value={annual === null ? "Not available" : formatCurrency(annual, data.currency)} />
            <Separator />
            <ProfileRow label="First detected" value={formatDate(data.firstDetectedAt)} />
            <ProfileRow label="Last charge" value={formatDate(data.lastChargeAt)} />
            <ProfileRow label="Latest data" value={history.data?.[0] ? formatRelativeDate(history.data[0].observedAt) : "Not available"} />
            {data.lastPriceChangedAt ? <ProfileRow label="Last price change detected" value={formatDate(data.lastPriceChangedAt)} /> : null}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>What this means</CardTitle>
          <CardDescription>How to interpret the detected recurring pattern.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-3">
          <Explanation icon={ShieldCheck} title="Pattern confidence" body={`FinSight assigned ${confidence}% confidence based on recurring transaction cadence and amount consistency.`} />
          <Explanation icon={CalendarClock} title="Next charge" body={data.nextExpectedChargeAt ? `The next charge is estimated for ${formatDate(data.nextExpectedChargeAt)}. ${nextCharge.detail}.` : "A next charge is not currently projected because the observed cadence is not predictable enough."} />
          <Explanation icon={!hasComparableChange ? CircleDollarSign : priceChangeIsIncrease ? TrendingUp : TrendingDown} title="Price movement" body={hasComparableChange ? `The latest comparable observation moved from ${formatCurrency(priceChange.previous!, data.currency)} to ${formatCurrency(priceChange.current, data.currency)} (${priceChange.percentage! > 0 ? "+" : ""}${priceChange.percentage!.toFixed(1)}%).` : "FinSight does not have enough comparable observations to quantify a price change yet."} />
        </CardContent>
        <CardFooter className="border-t bg-muted/10 text-xs text-muted-foreground">
          {observations.length > 0 ? `Using the latest ${observations.length} observed price${observations.length === 1 ? "" : "s"}.` : "No price observations are available yet."}
        </CardFooter>
      </Card>

      {observations.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Observed charges</CardTitle>
            <CardDescription>Each observation maps back to the transaction that supplied the price signal.</CardDescription>
          </CardHeader>
          <CardContent className="p-0">
            <div className="divide-y">
              {[...observations].sort((a, b) => new Date(b.observedAt).getTime() - new Date(a.observedAt).getTime()).slice(0, 8).map((observation) => (
                <Link key={observation.id} href={`/transactions/${observation.transactionId}`} className="flex items-center gap-4 px-5 py-4 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none sm:px-6">
                  <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted"><CircleDollarSign className="size-4 text-muted-foreground" aria-hidden="true" /></div>
                  <div className="min-w-0 flex-1"><p className="text-sm font-medium">Observed charge</p><p className="mt-0.5 text-xs text-muted-foreground">{formatDate(observation.observedAt)}</p></div>
                  <div className="flex items-center gap-3"><p className="text-sm font-semibold tabular-nums">{formatCurrency(observation.amount, data.currency)}</p><ExternalArrow /></div>
                </Link>
              ))}
            </div>
          </CardContent>
        </Card>
      ) : null}

      <Dialog open={dismissOpen} onOpenChange={setDismissOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Dismiss this subscription?</DialogTitle>
            <DialogDescription>
              Dismissing {data.merchantName} removes this detected recurring pattern from your active subscription view. Future transaction processing can detect a new pattern again if the evidence changes.
            </DialogDescription>
          </DialogHeader>
          {dismiss.isError ? <p role="alert" className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">{dismiss.error instanceof Error ? dismiss.error.message : "The subscription could not be dismissed. Please try again."}</p> : null}
          <DialogFooter>
            <Button variant="outline" onClick={() => setDismissOpen(false)} disabled={dismiss.isPending}>Cancel</Button>
            <Button variant="destructive" onClick={() => void handleDismiss()} disabled={dismiss.isPending}>
              {dismiss.isPending ? <><Loader2 className="animate-spin" aria-hidden="true" /> Dismissing…</> : "Dismiss subscription"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function DetailCard({ icon: Icon, label, value, detail }: { readonly icon: LucideIcon; readonly label: string; readonly value: string; readonly detail: string }) {
  return <Card><CardHeader className="pb-2"><div className="flex items-center justify-between gap-3"><CardDescription>{label}</CardDescription><Icon className="size-4 text-muted-foreground" aria-hidden="true" /></div></CardHeader><CardContent><p className="text-xl font-semibold tracking-tight tabular-nums">{value}</p><p className="mt-1 text-xs text-muted-foreground">{detail}</p></CardContent></Card>;
}

function ProfileRow({ label, value }: { readonly label: string; readonly value: string }) {
  return <div className="flex items-start justify-between gap-5 text-sm"><span className="text-muted-foreground">{label}</span><span className="text-right font-medium tabular-nums">{value}</span></div>;
}

function Explanation({ icon: Icon, title, body }: { readonly icon: LucideIcon; readonly title: string; readonly body: string }) {
  return <div className="rounded-xl border bg-muted/20 p-4"><Icon className="size-4 text-muted-foreground" aria-hidden="true" /><p className="mt-3 text-sm font-medium">{title}</p><p className="mt-1 text-sm leading-6 text-muted-foreground">{body}</p></div>;
}
