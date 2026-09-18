"use client";

import { useEffect, useMemo, useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import { CalendarClock, CircleDollarSign, Filter, TrendingUp, WalletCards, type LucideIcon } from "lucide-react";

import { SubscriptionCard } from "@/components/subscriptions/subscription-card";
import { SubscriptionFilters, type SubscriptionFilterState } from "@/components/subscriptions/subscription-filters";
import { isRecentPriceChange, isUpcomingWithinDays, groupedMonthlyCommitments, subscriptionFrequencies, subscriptionStatuses } from "@/components/subscriptions/subscription-utils";
import { SubscriptionStatusBadge } from "@/components/subscriptions/subscription-status-badge";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { DashboardErrorState } from "@/components/dashboard/dashboard-error-state";
import { EmptyState } from "@/components/feedback/empty-state";
import type { Subscription } from "@/lib/api/types";
import { useSubscriptions } from "@/lib/subscriptions/use-subscriptions";
import { formatCurrency } from "@/lib/formatting/currency";

const defaultFilters: SubscriptionFilterState = {
  search: "",
  status: "all",
  frequency: "all",
  currency: "all",
};

function readFilters(params: URLSearchParams): SubscriptionFilterState {
  const status = params.get("status") ?? "all";
  const frequency = params.get("frequency") ?? "all";

  return {
    search: params.get("q") ?? "",
    status: subscriptionStatuses.includes(status as (typeof subscriptionStatuses)[number]) ? status : "all",
    frequency: subscriptionFrequencies.includes(frequency as (typeof subscriptionFrequencies)[number]) ? frequency : "all",
    currency: params.get("currency") ?? "all",
  };
}

function toSearchParams(filters: SubscriptionFilterState): string {
  const params = new URLSearchParams();
  if (filters.search.trim()) params.set("q", filters.search.trim());
  if (filters.status !== "all") params.set("status", filters.status);
  if (filters.frequency !== "all") params.set("frequency", filters.frequency);
  if (filters.currency !== "all") params.set("currency", filters.currency);
  return params.toString();
}

function filterSubscriptions(subscriptions: readonly Subscription[], filters: SubscriptionFilterState): readonly Subscription[] {
  const search = filters.search.trim().toLocaleLowerCase();

  return subscriptions.filter((subscription) => {
    const matchesSearch = !search || subscription.merchantName.toLocaleLowerCase().includes(search);
    const matchesStatus = filters.status === "all" || subscription.status === filters.status;
    const matchesFrequency = filters.frequency === "all" || subscription.frequency === filters.frequency;
    const matchesCurrency = filters.currency === "all" || subscription.currency === filters.currency;
    return matchesSearch && matchesStatus && matchesFrequency && matchesCurrency;
  });
}

function LoadingSubscriptions() {
  return (
    <div className="space-y-8" aria-busy="true">
      <div className="space-y-2"><Skeleton className="h-8 w-64" /><Skeleton className="h-4 w-[34rem] max-w-full" /></div>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{Array.from({ length: 4 }, (_, index) => <Skeleton key={index} className="h-28 rounded-xl" />)}</div>
      <Skeleton className="h-20 rounded-xl" />
      <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">{Array.from({ length: 6 }, (_, index) => <Skeleton key={index} className="h-[520px] rounded-xl" />)}</div>
    </div>
  );
}

function SummaryCard({ icon: Icon, label, value, detail }: { readonly icon: LucideIcon; readonly label: string; readonly value: string; readonly detail: string }) {
  return (
    <Card>
      <CardHeader className="pb-2"><div className="flex items-center justify-between gap-3"><span className="text-xs font-medium text-muted-foreground">{label}</span><Icon className="size-4 text-muted-foreground" aria-hidden="true" /></div></CardHeader>
      <CardContent><p className="text-2xl font-semibold tracking-tight tabular-nums">{value}</p><p className="mt-1 text-xs text-muted-foreground">{detail}</p></CardContent>
    </Card>
  );
}

/** Renders the complete subscription intelligence workspace. */
export function SubscriptionOverview() {
  const subscriptions = useSubscriptions();
  const router = useRouter();
  const pathname = usePathname();
  const [filters, setFilters] = useState<SubscriptionFilterState>(defaultFilters);

  useEffect(() => {
    const syncFromUrl = () => setFilters(readFilters(new URLSearchParams(window.location.search)));
    syncFromUrl();
    window.addEventListener("popstate", syncFromUrl);
    return () => window.removeEventListener("popstate", syncFromUrl);
  }, []);

  const allSubscriptions = subscriptions.data ?? [];
  const visibleSubscriptions = useMemo(() => filterSubscriptions(allSubscriptions, filters), [allSubscriptions, filters]);
  const activeSubscriptions = useMemo(() => allSubscriptions.filter((subscription) => subscription.status === "Active"), [allSubscriptions]);
  const upcomingCount = useMemo(() => activeSubscriptions.filter((subscription) => isUpcomingWithinDays(subscription, 30)).length, [activeSubscriptions]);
  const recentPriceChanges = useMemo(() => activeSubscriptions.filter((subscription) => isRecentPriceChange(subscription)).length, [activeSubscriptions]);
  const commitmentGroups = useMemo(() => [...groupedMonthlyCommitments(activeSubscriptions).entries()].sort(([left], [right]) => left.localeCompare(right)), [activeSubscriptions]);

  const updateFilters = (next: SubscriptionFilterState) => {
    setFilters(next);
    const query = toSearchParams(next);
    router.replace(query ? `${pathname}?${query}` : pathname, { scroll: false });
  };

  if (subscriptions.isLoading) return <LoadingSubscriptions />;
  if (subscriptions.isError) return <DashboardErrorState title="Subscriptions unavailable" description="FinSight could not load your recurring commitments. Please try again." onRetry={() => void subscriptions.refetch()} />;

  return (
    <div className="space-y-8">
      <section className="flex flex-col justify-between gap-5 lg:flex-row lg:items-end">
        <div className="max-w-3xl space-y-2">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">Subscription intelligence</p>
          <h2 className="text-2xl font-semibold tracking-tight sm:text-3xl">Know what is recurring before it renews.</h2>
          <p className="text-sm leading-6 text-muted-foreground sm:text-base">
            FinSight detects recurring commitments from your transaction history, estimates when they will charge again, and highlights meaningful price changes.
          </p>
        </div>
        <div className="flex items-center gap-2 text-xs text-muted-foreground"><Filter className="size-3.5" aria-hidden="true" />Showing {visibleSubscriptions.length} of {allSubscriptions.length}</div>
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-label="Subscription summary">
        <SummaryCard icon={WalletCards} label="Active subscriptions" value={String(activeSubscriptions.length)} detail={`${allSubscriptions.length} detected in total`} />
        <SummaryCard icon={CalendarClock} label="Upcoming in 30 days" value={String(upcomingCount)} detail="Expected recurring charges" />
        <SummaryCard icon={TrendingUp} label="Recent price changes" value={String(recentPriceChanges)} detail="Detected within the last 90 days" />
        <SummaryCard icon={CircleDollarSign} label="Monthly commitments" value={commitmentGroups.length === 0 ? "—" : commitmentGroups.length === 1 ? formatCurrency(commitmentGroups[0][1], commitmentGroups[0][0]) : `${commitmentGroups.length} currencies`} detail={commitmentGroups.length === 0 ? "No predictable active commitments yet" : commitmentGroups.map(([currency, amount]) => `${formatCurrency(amount, currency)} / month`).join(" · ")} />
      </section>

      {commitmentGroups.length > 1 ? (
        <section className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3" aria-label="Monthly commitments by currency">
          {commitmentGroups.map(([currency, amount]) => (
            <Card key={currency} size="sm"><CardContent className="flex items-center justify-between gap-4 py-3"><div><p className="text-xs font-medium text-muted-foreground">{currency} monthly commitments</p><p className="mt-1 text-lg font-semibold tabular-nums">{formatCurrency(amount, currency)}</p></div><Badge variant="outline">Active</Badge></CardContent></Card>
          ))}
        </section>
      ) : null}

      <SubscriptionFilters filters={filters} subscriptions={allSubscriptions} onChange={updateFilters} />

      {visibleSubscriptions.length === 0 ? (
        <EmptyState
          icon={CircleDollarSign}
          title="No subscriptions match these filters"
          description="Try clearing a filter or connect and synchronize an account so FinSight can analyze recurring transaction patterns."
          action={{ label: "Clear filters", onClick: () => updateFilters(defaultFilters) }}
        />
      ) : (
        <section className="grid gap-5 md:grid-cols-2 xl:grid-cols-3" aria-label="Detected subscriptions">
          {visibleSubscriptions.map((subscription) => <SubscriptionCard key={subscription.id} subscription={subscription} />)}
        </section>
      )}

      {allSubscriptions.length > 0 ? (
        <section className="flex flex-wrap items-center gap-2 rounded-xl border bg-muted/20 p-4 text-xs text-muted-foreground">
          {subscriptionStatuses.map((status) => <span key={status} className="inline-flex items-center gap-1.5"><SubscriptionStatusBadge status={status} /><span>{allSubscriptions.filter((subscription) => subscription.status === status).length}</span></span>)}
          <span className="ml-auto">Detection confidence reflects the strength of the recurring-payment pattern.</span>
        </section>
      ) : null}
    </div>
  );
}
