"use client";

import Link from "next/link";
import {
  AlertTriangle,
  ArrowDownRight,
  ArrowRight,
  ArrowUpRight,
  Bell,
  CreditCard,
  Lightbulb,
  Repeat2,
  TrendingUp,
  WalletCards,
  type LucideIcon,
} from "lucide-react";
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
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { DashboardErrorState } from "@/components/dashboard/dashboard-error-state";
import { useDashboard } from "@/lib/dashboard/use-dashboard";
import { useAuth } from "@/lib/auth/auth-context";
import { formatCurrency } from "@/lib/formatting/currency";
import { formatRelativeDate } from "@/lib/formatting/date";
import type { DashboardBalance, DashboardTransaction } from "@/lib/api/types";

function isExpense(transaction: DashboardTransaction): boolean {
  return transaction.type === "Purchase" || transaction.type === "Fee";
}

function formatAmount(transaction: DashboardTransaction): string {
  const amount = formatCurrency(Math.abs(transaction.amount), transaction.currency);
  return transaction.amount < 0 || isExpense(transaction) ? `-${amount}` : `+${amount}`;
}

function accountBalanceLabel(balance: DashboardBalance): string {
  return balance.accountCount === 1 ? "1 account" : `${balance.accountCount} accounts`;
}

function firstName(displayName: string): string {
  return displayName.trim().split(/\s+/)[0] || "there";
}

function MetricCard({
  label,
  value,
  detail,
  icon: Icon,
}: {
  readonly label: string;
  readonly value: string;
  readonly detail: string;
  readonly icon: LucideIcon;
}) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between gap-3">
          <CardDescription>{label}</CardDescription>
          <div className="flex size-9 items-center justify-center rounded-lg bg-muted text-muted-foreground">
            <Icon className="size-4" aria-hidden="true" />
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <p className="text-2xl font-semibold tracking-tight tabular-nums">{value}</p>
        <p className="mt-1 text-xs text-muted-foreground">{detail}</p>
      </CardContent>
    </Card>
  );
}

function ChartTooltip({ active, payload, label }: { active?: boolean; payload?: Array<{ value?: number; dataKey?: string }>; label?: string }) {
  if (!active || !payload?.length) return null;

  return (
    <div className="rounded-lg border bg-popover px-3 py-2 text-xs shadow-lg">
      <p className="mb-1 font-medium">{label}</p>
      {payload.map((item) => (
        <div key={item.dataKey} className="flex items-center justify-between gap-6 text-muted-foreground">
          <span className="capitalize">{item.dataKey}</span>
          <span className="font-medium text-foreground">{formatCurrency(item.value ?? 0)}</span>
        </div>
      ))}
    </div>
  );
}

function LoadingDashboard() {
  return (
    <div className="space-y-6" aria-busy="true">
      <div className="space-y-2">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-96 max-w-full" />
      </div>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: 4 }, (_, index) => <Skeleton key={index} className="h-32 rounded-xl" />)}
      </div>
      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.7fr)_minmax(320px,0.8fr)]">
        <Skeleton className="h-[360px] rounded-xl" />
        <Skeleton className="h-[360px] rounded-xl" />
      </div>
    </div>
  );
}

/** Renders the complete financial overview for the authenticated FinSight user. */
export function DashboardOverview() {
  const { user } = useAuth();
  const dashboard = useDashboard();

  if (dashboard.isLoading) return <LoadingDashboard />;
  if (dashboard.isError || !dashboard.data) return <DashboardErrorState onRetry={() => void dashboard.refetch()} />;

  const data = dashboard.data;
  const primaryCurrency = data.primaryCurrency;
  const metrics = data.metrics;
  const spendingChange = Math.abs(metrics.spendingChangePercentage);
  const spendingUp = metrics.spendingChangePercentage > 0;
  const greeting = user ? `Good to see you, ${firstName(user.displayName)}.` : "Your financial overview.";

  return (
    <div className="space-y-6">
      <section className="space-y-1">
        <h2 className="text-2xl font-semibold tracking-tight sm:text-3xl">{greeting}</h2>
        <p className="text-sm text-muted-foreground sm:text-base">
          A live view of your cash position, spending patterns and the financial signals FinSight has detected.
        </p>
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-label="Financial summary">
        <MetricCard
          label="Current balance"
          value={data.balances.length === 0 ? "No accounts" : formatCurrency(data.balances.find((balance) => balance.currency === primaryCurrency)?.currentBalance ?? 0, primaryCurrency)}
          detail={data.balances.length === 0 ? "Connect an account to get started" : data.balances.length === 1 ? accountBalanceLabel(data.balances[0]) : `${primaryCurrency} primary balance · ${data.balances.length} currencies total`}
          icon={WalletCards}
        />
        <MetricCard
          label="This month's spending"
          value={formatCurrency(metrics.currentMonthSpending, primaryCurrency)}
          detail={
            metrics.previousMonthSpending === 0
              ? "Compared with no spend last month"
              : `${spendingUp ? "Up" : "Down"} ${spendingChange}% vs last month`
          }
          icon={spendingUp ? ArrowUpRight : ArrowDownRight}
        />
        <MetricCard
          label="This month's income"
          value={formatCurrency(metrics.currentMonthIncome, primaryCurrency)}
          detail="Recorded deposits this month"
          icon={TrendingUp}
        />
        <MetricCard
          label="Open financial signals"
          value={String(metrics.openAnomalyCount + metrics.activeInsightCount)}
          detail={`${metrics.openAnomalyCount} anomalies · ${metrics.activeInsightCount} insights`}
          icon={Lightbulb}
        />
      </section>

      {data.balances.length > 1 ? (
        <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3" aria-label="Balances by currency">
          {data.balances.map((balance) => (
            <Card key={balance.currency} size="sm">
              <CardContent className="flex items-center justify-between gap-4 py-3">
                <div>
                  <p className="text-xs font-medium text-muted-foreground">{balance.currency}</p>
                  <p className="mt-1 text-lg font-semibold tabular-nums">{formatCurrency(balance.currentBalance, balance.currency)}</p>
                </div>
                <Badge variant="outline">{accountBalanceLabel(balance)}</Badge>
              </CardContent>
            </Card>
          ))}
        </section>
      ) : null}

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.7fr)_minmax(320px,0.8fr)]">
        <Card className="min-w-0">
          <CardHeader>
            <CardTitle>Cash flow</CardTitle>
            <CardDescription>Income and spending across the last six calendar months.</CardDescription>
            <CardAction>
              <Button asChild variant="ghost" size="sm">
                <Link href="/transactions">View transactions <ArrowRight aria-hidden="true" /></Link>
              </Button>
            </CardAction>
          </CardHeader>
          <CardContent>
            <div className="h-[300px] w-full">
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={data.spendingTrend} margin={{ top: 10, right: 8, left: -20, bottom: 0 }}>
                  <defs>
                    <linearGradient id="income-fill" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopOpacity={0.24} />
                      <stop offset="100%" stopOpacity={0} />
                    </linearGradient>
                    <linearGradient id="spending-fill" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopOpacity={0.16} />
                      <stop offset="100%" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid vertical={false} strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="label" tickLine={false} axisLine={false} tick={{ fontSize: 12 }} />
                  <YAxis tickLine={false} axisLine={false} tick={{ fontSize: 11 }} tickFormatter={(value) => `${Math.round(value / 1000)}k`} />
                  <Tooltip content={<ChartTooltip />} />
                  <Area type="monotone" dataKey="income" stroke="currentColor" fill="url(#income-fill)" className="text-foreground" strokeWidth={2} />
                  <Area type="monotone" dataKey="spending" stroke="currentColor" fill="url(#spending-fill)" className="text-muted-foreground" strokeWidth={2} />
                </AreaChart>
              </ResponsiveContainer>
            </div>
            <div className="mt-4 grid gap-3 sm:grid-cols-3">
              <div className="rounded-lg border bg-muted/20 p-3">
                <p className="text-xs text-muted-foreground">This month</p>
                <p className="mt-1 font-semibold tabular-nums">{formatCurrency(metrics.currentMonthIncome - metrics.currentMonthSpending, primaryCurrency)}</p>
                <p className="text-xs text-muted-foreground">Net cash flow</p>
              </div>
              <div className="rounded-lg border bg-muted/20 p-3">
                <p className="text-xs text-muted-foreground">Previous month</p>
                <p className="mt-1 font-semibold tabular-nums">{formatCurrency(data.spendingTrend.at(-2)?.netCashFlow ?? 0, primaryCurrency)}</p>
                <p className="text-xs text-muted-foreground">Net cash flow</p>
              </div>
              <div className="rounded-lg border bg-muted/20 p-3">
                <p className="text-xs text-muted-foreground">Unread alerts</p>
                <p className="mt-1 font-semibold tabular-nums">{metrics.unreadNotificationCount}</p>
                <p className="text-xs text-muted-foreground">Notifications to review</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Where your money goes</CardTitle>
            <CardDescription>Top spending categories this month.</CardDescription>
            <CardAction>
              <Button asChild variant="ghost" size="sm">
                <Link href="/transactions">Explore <ArrowRight aria-hidden="true" /></Link>
              </Button>
            </CardAction>
          </CardHeader>
          <CardContent>
            {data.categorySpending.length === 0 ? (
              <div className="flex min-h-56 items-center justify-center rounded-lg border border-dashed px-6 text-center text-sm text-muted-foreground">
                FinSight has not recorded enough current-month spending to show a category breakdown yet.
              </div>
            ) : (
              <div className="space-y-5">
                {data.categorySpending.map((category) => (
                  <div key={`${category.categoryId ?? "uncategorized"}-${category.categoryName}`} className="space-y-2">
                    <div className="flex items-center justify-between gap-4 text-sm">
                      <span className="truncate font-medium">{category.categoryName}</span>
                      <span className="shrink-0 tabular-nums text-muted-foreground">{formatCurrency(category.amount, primaryCurrency)}</span>
                    </div>
                    <div className="flex items-center gap-3">
                      <Progress value={category.percentage} aria-label={`${category.categoryName} ${category.percentage}% of spending`} className="min-w-0 flex-1" />
                      <span className="w-10 text-right text-xs tabular-nums text-muted-foreground">{category.percentage}%</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Recent activity</CardTitle>
            <CardDescription>Your latest imported transactions.</CardDescription>
            <CardAction>
              <Button asChild variant="ghost" size="sm">
                <Link href="/transactions">All transactions <ArrowRight aria-hidden="true" /></Link>
              </Button>
            </CardAction>
          </CardHeader>
          <CardContent className="p-0">
            {data.recentTransactions.length === 0 ? (
              <div className="mx-6 mb-6 rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground">
                No transactions have been imported yet. Connect an account to start building your financial picture.
              </div>
            ) : (
              <div className="divide-y">
                {data.recentTransactions.slice(0, 6).map((transaction) => (
                  <Link key={transaction.id} href={`/transactions/${transaction.id}`} className="flex items-center gap-3 px-6 py-3.5 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none">
                    <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted">
                      <CreditCard className="size-4 text-muted-foreground" aria-hidden="true" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium">{transaction.merchantName}</p>
                      <p className="truncate text-xs text-muted-foreground">{transaction.categoryName} · {formatRelativeDate(transaction.transactionDate)}</p>
                    </div>
                    <div className="shrink-0 text-right">
                      <p className={transaction.amount < 0 ? "text-sm font-semibold tabular-nums" : "text-sm font-semibold tabular-nums text-emerald-700 dark:text-emerald-400"}>{formatAmount(transaction)}</p>
                      <p className="text-xs text-muted-foreground">{transaction.currency}</p>
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Priority signals</CardTitle>
            <CardDescription>Items FinSight thinks deserve your attention.</CardDescription>
            <CardAction>
              <Button asChild variant="ghost" size="sm">
                <Link href="/insights">View intelligence <ArrowRight aria-hidden="true" /></Link>
              </Button>
            </CardAction>
          </CardHeader>
          <CardContent>
            {data.priorityAlerts.length === 0 ? (
              <div className="flex min-h-56 flex-col items-center justify-center rounded-lg border border-dashed px-6 text-center">
                <div className="flex size-10 items-center justify-center rounded-full bg-muted">
                  <Lightbulb className="size-5 text-muted-foreground" aria-hidden="true" />
                </div>
                <p className="mt-3 text-sm font-medium">No priority signals</p>
                <p className="mt-1 max-w-xs text-xs text-muted-foreground">Nothing unusual has been surfaced from your recent financial activity.</p>
              </div>
            ) : (
              <div className="space-y-3">
                {data.priorityAlerts.map((alert) => (
                  <Link key={`${alert.kind}-${alert.id}`} href={alert.href} className="block rounded-lg border p-4 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none">
                    <div className="flex gap-3">
                      <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted">
                        {alert.kind === "anomaly" ? <AlertTriangle className="size-4" aria-hidden="true" /> : <Lightbulb className="size-4" aria-hidden="true" />}
                      </div>
                      <div className="min-w-0 flex-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <p className="text-sm font-medium">{alert.title}</p>
                          <Badge variant={alert.severity === "Critical" || alert.severity === "High" ? "destructive" : "secondary"}>{alert.severity}</Badge>
                        </div>
                        <p className="mt-1 line-clamp-2 text-xs leading-5 text-muted-foreground">{alert.description}</p>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <section className="grid gap-6 xl:grid-cols-[minmax(0,1.2fr)_minmax(0,0.8fr)]">
        <Card>
          <CardHeader>
            <CardTitle>Upcoming recurring payments</CardTitle>
            <CardDescription>Subscriptions FinSight expects to charge next.</CardDescription>
            <CardAction>
              <Button asChild variant="ghost" size="sm">
                <Link href="/subscriptions">All subscriptions <ArrowRight aria-hidden="true" /></Link>
              </Button>
            </CardAction>
          </CardHeader>
          <CardContent>
            {data.upcomingSubscriptions.length === 0 ? (
              <div className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground">No active subscriptions have been detected yet.</div>
            ) : (
              <div className="grid gap-3 sm:grid-cols-2">
                {data.upcomingSubscriptions.map((subscription) => (
                  <Link key={subscription.id} href={`/subscriptions/${subscription.id}`} className="rounded-lg border p-4 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none">
                    <div className="flex items-start justify-between gap-3">
                      <div className="flex min-w-0 items-center gap-3">
                        <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted">
                          <Repeat2 className="size-4 text-muted-foreground" aria-hidden="true" />
                        </div>
                        <div className="min-w-0">
                          <p className="truncate text-sm font-medium">{subscription.merchantName}</p>
                          <p className="text-xs text-muted-foreground">{subscription.frequency}</p>
                        </div>
                      </div>
                      <p className="shrink-0 text-sm font-semibold tabular-nums">{formatCurrency(subscription.currentAmount, subscription.currency)}</p>
                    </div>
                    <p className="mt-3 text-xs text-muted-foreground">
                      {subscription.nextExpectedChargeAt ? `Expected ${formatRelativeDate(subscription.nextExpectedChargeAt)}` : "Next charge not predicted"}
                    </p>
                  </Link>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Workspace pulse</CardTitle>
            <CardDescription>The financial areas worth checking next.</CardDescription>
          </CardHeader>
          <CardContent className="grid gap-3 sm:grid-cols-2 xl:grid-cols-1">
            <Link href="/accounts" className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none">
              <WalletCards className="size-4 text-muted-foreground" aria-hidden="true" />
              <span className="flex-1 text-sm font-medium">Accounts</span>
              <span className="text-xs text-muted-foreground">{data.balances.reduce((total, item) => total + item.accountCount, 0)} connected</span>
              <ArrowRight className="size-4 text-muted-foreground" aria-hidden="true" />
            </Link>
            <Link href="/subscriptions" className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none">
              <Repeat2 className="size-4 text-muted-foreground" aria-hidden="true" />
              <span className="flex-1 text-sm font-medium">Subscriptions</span>
              <span className="text-xs text-muted-foreground">{metrics.activeSubscriptionCount} active</span>
              <ArrowRight className="size-4 text-muted-foreground" aria-hidden="true" />
            </Link>
            <Link href="/anomalies" className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none">
              <AlertTriangle className="size-4 text-muted-foreground" aria-hidden="true" />
              <span className="flex-1 text-sm font-medium">Anomalies</span>
              <span className="text-xs text-muted-foreground">{metrics.openAnomalyCount} open</span>
              <ArrowRight className="size-4 text-muted-foreground" aria-hidden="true" />
            </Link>
            <Link href="/notifications" className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none">
              <Bell className="size-4 text-muted-foreground" aria-hidden="true" />
              <span className="flex-1 text-sm font-medium">Notifications</span>
              <span className="text-xs text-muted-foreground">{metrics.unreadNotificationCount} unread</span>
              <ArrowRight className="size-4 text-muted-foreground" aria-hidden="true" />
            </Link>
          </CardContent>
        </Card>
      </section>
    </div>
  );
}
