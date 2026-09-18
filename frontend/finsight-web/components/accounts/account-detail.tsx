"use client";

import Link from "next/link";
import { ArrowLeft, CircleDollarSign, Clock3, CreditCard, Landmark, RefreshCw, WalletCards, type LucideIcon } from "lucide-react";

import { AccountConnectionStatus } from "@/components/accounts/account-status";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useAccount, useAccountConnections, useAccountTransactions, useSyncAccount } from "@/lib/accounts/use-accounts";
import type { AccountTransaction } from "@/lib/accounts/account-service";
import { formatCurrency } from "@/lib/formatting/currency";
import { formatDate } from "@/lib/formatting/date";

function formatTransactionAmount(transaction: AccountTransaction) {
  const amount = formatCurrency(Math.abs(transaction.amount), transaction.currency);
  const expense = transaction.type === "Purchase" || transaction.type === "Fee" || transaction.amount < 0;
  return expense ? `-${amount}` : `+${amount}`;
}

function LoadingAccountDetail() {
  return (
    <div className="space-y-6" aria-busy="true">
      <Skeleton className="h-6 w-32" />
      <div className="grid gap-4 lg:grid-cols-[1.5fr_1fr]">
        <Skeleton className="h-64 rounded-xl" />
        <Skeleton className="h-64 rounded-xl" />
      </div>
      <Skeleton className="h-96 rounded-xl" />
    </div>
  );
}

/** Renders a single financial account with synchronization and recent activity. */
export function AccountDetail({ accountId }: { readonly accountId: string }) {
  const account = useAccount(accountId);
  const transactions = useAccountTransactions(accountId);
  const connections = useAccountConnections();
  const sync = useSyncAccount();

  if (account.isLoading || transactions.isLoading || connections.isLoading) return <LoadingAccountDetail />;
  if (account.isError || !account.data || transactions.isError || connections.isError) {
    return (
      <Card>
        <CardContent className="flex min-h-48 flex-col items-center justify-center px-6 py-10 text-center">
          <WalletCards className="size-5 text-muted-foreground" aria-hidden="true" />
          <h2 className="mt-3 font-semibold">Account unavailable</h2>
          <p className="mt-1 max-w-md text-sm text-muted-foreground">The account could not be loaded. It may no longer belong to your active FinSight workspace.</p>
          <Button asChild variant="outline" className="mt-5"><Link href="/accounts"><ArrowLeft aria-hidden="true" /> Back to accounts</Link></Button>
        </CardContent>
      </Card>
    );
  }

  const data = account.data;
  const connection = connections.data?.find((item) => item.id === data.connectionId);

  return (
    <div className="space-y-6">
      <div>
        <Button asChild variant="ghost" size="sm"><Link href="/accounts"><ArrowLeft aria-hidden="true" /> Back to accounts</Link></Button>
      </div>

      <section className="flex flex-col justify-between gap-5 lg:flex-row lg:items-end">
        <div className="space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">{connection?.institutionName ?? "Financial institution"}</p>
            {connection ? <AccountConnectionStatus status={connection.status} /> : null}
          </div>
          <h1 className="text-2xl font-semibold tracking-tight sm:text-3xl">{data.name}</h1>
          <p className="text-sm text-muted-foreground">{data.type} · {data.currency} · Account {data.id.slice(0, 8)}…</p>
        </div>
        {connection ? (
          <Button variant="outline" onClick={() => sync.mutate(connection.id)} disabled={sync.isPending || connection.status === "Syncing"}>
            <RefreshCw className={sync.isPending ? "animate-spin" : ""} aria-hidden="true" />
            {sync.isPending ? "Synchronizing…" : "Sync now"}
          </Button>
        ) : null}
      </section>

      {sync.isError ? <p role="alert" className="rounded-lg border border-destructive/20 bg-destructive/5 p-3 text-sm text-destructive">{sync.error instanceof Error ? sync.error.message : "Synchronization failed."}</p> : null}

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4" aria-label="Account summary">
        <SummaryCard icon={CircleDollarSign} label="Current balance" value={formatCurrency(data.currentBalance, data.currency)} />
        <SummaryCard icon={WalletCards} label="Available balance" value={formatCurrency(data.availableBalance, data.currency)} />
        <SummaryCard icon={Landmark} label="Account type" value={data.type} />
        <SummaryCard icon={Clock3} label="Connection" value={connection ? (connection.lastSuccessfulSyncAt ? formatDate(connection.lastSuccessfulSyncAt) : "Never synced") : "Unknown"} />
      </section>

      <Card>
        <CardHeader>
          <CardTitle>Recent transactions</CardTitle>
          <CardDescription>The latest imported activity associated with this account.</CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          {(transactions.data?.length ?? 0) === 0 ? (
            <div className="m-6 rounded-lg border border-dashed p-8 text-center">
              <CreditCard className="mx-auto size-5 text-muted-foreground" aria-hidden="true" />
              <p className="mt-3 font-medium">No transactions imported yet</p>
              <p className="mt-1 text-sm text-muted-foreground">Run a synchronization to fetch the latest activity.</p>
            </div>
          ) : (
            <div className="divide-y">
              {transactions.data?.map((transaction) => <AccountTransactionRow key={transaction.id} transaction={transaction} />)}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function SummaryCard({ icon: Icon, label, value }: { readonly icon: LucideIcon; readonly label: string; readonly value: string }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardDescription>{label}</CardDescription>
      </CardHeader>
      <CardContent className="flex items-end justify-between gap-3">
        <p className="max-w-[15rem] truncate text-lg font-semibold tracking-tight tabular-nums">{value}</p>
        <Icon className="size-4 text-muted-foreground" aria-hidden="true" />
      </CardContent>
    </Card>
  );
}

function AccountTransactionRow({ transaction }: { readonly transaction: AccountTransaction }) {
  const positive = !formatTransactionAmount(transaction).startsWith("-");
  return (
    <Link href={`/transactions/${transaction.id}`} className="flex items-center gap-4 px-5 py-4 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none sm:px-6">
      <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted"><CreditCard className="size-4 text-muted-foreground" aria-hidden="true" /></div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium">{transaction.normalizedDescription || transaction.rawDescription}</p>
        <p className="truncate text-xs text-muted-foreground">{transaction.classificationStatus} · {formatDate(transaction.transactionDate)}</p>
      </div>
      <div className="shrink-0 text-right">
        <p className={positive ? "text-sm font-semibold tabular-nums text-emerald-700 dark:text-emerald-400" : "text-sm font-semibold tabular-nums"}>{formatTransactionAmount(transaction)}</p>
        <p className="text-xs text-muted-foreground">{transaction.currency}</p>
      </div>
    </Link>
  );
}
