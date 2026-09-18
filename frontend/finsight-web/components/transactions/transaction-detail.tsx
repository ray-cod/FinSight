"use client";

import Link from "next/link";
import { ArrowDownRight, ArrowLeft, ArrowUpRight, Bot, CalendarClock, CreditCard, GitBranch, Info, Landmark, Sparkles, UserRoundCheck, type LucideIcon } from "lucide-react";

import { TransactionClassificationEditor } from "@/components/transactions/transaction-classification-editor";
import { TransactionStatusBadge } from "@/components/transactions/transaction-status-badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useTransaction } from "@/lib/transactions/use-transactions";
import { formatDate, formatRelativeDate } from "@/lib/formatting/date";
import type { TransactionDetail as TransactionDetailModel } from "@/lib/api/types";
import { formatTransactionAmount, isExpense, transactionLabel } from "@/components/transactions/transaction-utils";

/** Renders the complete transaction context and classification controls. */
export function TransactionDetail({ transactionId }: { readonly transactionId: string }) {
  const transaction = useTransaction(transactionId);

  if (transaction.isLoading) return <TransactionDetailSkeleton />;

  if (transaction.isError || !transaction.data) {
    return (
      <Card>
        <CardContent className="flex min-h-64 flex-col items-center justify-center px-6 py-12 text-center">
          <CreditCard className="size-5 text-muted-foreground" aria-hidden="true" />
          <h1 className="mt-4 text-lg font-semibold">Transaction unavailable</h1>
          <p className="mt-1 max-w-md text-sm text-muted-foreground">The transaction could not be loaded. It may no longer belong to your FinSight workspace.</p>
          <Button asChild variant="outline" className="mt-5"><Link href="/transactions"><ArrowLeft aria-hidden="true" /> Back to transactions</Link></Button>
        </CardContent>
      </Card>
    );
  }

  return <TransactionDetailView transaction={transaction.data} />;
}

function TransactionDetailView({ transaction }: { readonly transaction: TransactionDetailModel }) {
  const expense = isExpense(transaction);
  const sourceIcon = transaction.classificationSource === "User" ? UserRoundCheck : transaction.classificationSource === "Ai" ? Bot : transaction.classificationSource === "Rule" ? GitBranch : Sparkles;

  return (
    <div className="space-y-6">
      <div><Button asChild variant="ghost" size="sm"><Link href="/transactions"><ArrowLeft aria-hidden="true" /> Back to transactions</Link></Button></div>

      <section className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
        <div className="min-w-0 space-y-3">
          <div className="flex flex-wrap items-center gap-2"><span className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">Transaction detail</span><TransactionStatusBadge status={transaction.status} kind="transaction" /></div>
          <div className="flex items-start gap-4">
            <div className="mt-1 flex size-11 shrink-0 items-center justify-center rounded-full bg-muted">{expense ? <ArrowDownRight className="size-5 text-muted-foreground" aria-hidden="true" /> : <ArrowUpRight className="size-5 text-emerald-700 dark:text-emerald-400" aria-hidden="true" />}</div>
            <div className="min-w-0"><h1 className="truncate text-2xl font-semibold tracking-tight sm:text-3xl">{transactionLabel(transaction)}</h1><p className="mt-1 truncate text-sm text-muted-foreground">{transaction.normalizedDescription || transaction.rawDescription}</p></div>
          </div>
        </div>
        <div className="shrink-0"><TransactionClassificationEditor transactionId={transaction.id} categoryId={transaction.categoryId} subcategoryId={transaction.subcategoryId} /></div>
      </section>

      <section className="grid gap-4 md:grid-cols-3" aria-label="Transaction summary">
        <SummaryCard label="Amount" value={formatTransactionAmount(transaction)} detail={transaction.currency} icon={expense ? ArrowDownRight : ArrowUpRight} positive={!expense} />
        <SummaryCard label="Transaction date" value={formatDate(transaction.transactionDate)} detail={formatRelativeDate(transaction.transactionDate)} icon={CalendarClock} />
        <SummaryCard label="Classification" value={transaction.categoryName ?? "Uncategorized"} detail={transaction.subcategoryName ?? "No subcategory"} icon={sourceIcon} />
      </section>

      <div className="grid gap-6 xl:grid-cols-[1.35fr_0.65fr]">
        <Card>
          <CardHeader><CardTitle>Transaction context</CardTitle><CardDescription>Source, account and classification information.</CardDescription></CardHeader>
          <CardContent className="grid gap-5 sm:grid-cols-2">
            <DetailField label="Account" value={transaction.accountName} icon={CreditCard} />
            <DetailField label="Institution" value={transaction.institutionName} icon={Landmark} />
            <DetailField label="Transaction type" value={transaction.type} />
            <DetailField label="Processing status" value={transaction.status} />
            <DetailField label="Merchant" value={transaction.merchantName ?? "Not resolved"} />
            <DetailField label="Category" value={transaction.categoryName ?? "Uncategorized"} />
            <DetailField label="Subcategory" value={transaction.subcategoryName ?? "Not assigned"} />
            <DetailField label="Provider transaction ID" value={transaction.providerTransactionId} mono />
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle>Classification intelligence</CardTitle><CardDescription>How FinSight arrived at the current classification.</CardDescription></CardHeader>
          <CardContent className="space-y-5">
            <div className="rounded-xl border bg-muted/20 p-4"><div className="flex items-start gap-3"><Sparkles className="mt-0.5 size-5 text-muted-foreground" aria-hidden="true" /><div><p className="text-sm font-medium">{transaction.classificationSource === "User" ? "User correction" : transaction.classificationSource === "Ai" ? "AI classification" : transaction.classificationSource === "Cache" ? "Cached classification" : transaction.classificationSource === "Rule" ? "Rule-based classification" : "Not classified"}</p><p className="mt-1 text-sm text-muted-foreground">{classificationDescription(transaction)}</p></div></div></div>
            <div className="flex items-center justify-between gap-4"><div><p className="text-sm text-muted-foreground">Status</p><div className="mt-1"><TransactionStatusBadge status={transaction.classificationStatus} /></div></div>{transaction.classificationConfidence !== null ? <div className="text-right"><p className="text-sm text-muted-foreground">Confidence</p><p className="mt-1 text-lg font-semibold tabular-nums">{Math.round(transaction.classificationConfidence * 100)}%</p></div> : null}</div>
            {transaction.classifiedAt ? <div className="border-t pt-4 text-sm"><p className="text-muted-foreground">Classified {formatRelativeDate(transaction.classifiedAt)}</p></div> : null}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><CardTitle>Import metadata</CardTitle><CardDescription>Technical provenance retained for support and audit workflows.</CardDescription></CardHeader>
        <CardContent className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          <DetailField label="Transaction ID" value={transaction.id} mono />
          <DetailField label="Account ID" value={transaction.accountId} mono />
          <DetailField label="Imported" value={formatDate(transaction.importedAt)} />
          <DetailField label="Raw description" value={transaction.rawDescription} />
          <DetailField label="Normalized description" value={transaction.normalizedDescription} />
          <DetailField label="User corrected" value={transaction.userCorrectedAt ? formatDate(transaction.userCorrectedAt) : "No"} />
        </CardContent>
      </Card>

      <div className="rounded-xl border border-dashed bg-muted/20 p-4 text-sm text-muted-foreground"><div className="flex items-start gap-3"><Info className="mt-0.5 size-4 shrink-0" aria-hidden="true" /><p>FinSight keeps the original provider description alongside its normalized representation so classification decisions remain explainable and traceable.</p></div></div>
    </div>
  );
}

function classificationDescription(transaction: TransactionDetailModel): string {
  switch (transaction.classificationSource) {
    case "User": return "You explicitly corrected this classification.";
    case "Ai": return "The hybrid categorization pipeline used an AI fallback for this classification.";
    case "Cache": return "The classification was retrieved from FinSight's reusable classification cache.";
    case "Rule": return "A deterministic FinSight categorization rule produced this classification.";
    default: return "This transaction has not received a classification yet.";
  }
}

function SummaryCard({ label, value, detail, icon: Icon, positive = false }: { readonly label: string; readonly value: string; readonly detail: string; readonly icon: LucideIcon; readonly positive?: boolean }) {
  return <Card><CardHeader className="pb-2"><CardDescription>{label}</CardDescription></CardHeader><CardContent className="flex items-end justify-between gap-3"><div className="min-w-0"><p className={positive ? "truncate text-xl font-semibold tracking-tight tabular-nums text-emerald-700 dark:text-emerald-400" : "truncate text-xl font-semibold tracking-tight tabular-nums"}>{value}</p><p className="mt-1 truncate text-xs text-muted-foreground">{detail}</p></div><Icon className="size-4 shrink-0 text-muted-foreground" aria-hidden="true" /></CardContent></Card>;
}

function DetailField({ label, value, icon: Icon, mono = false }: { readonly label: string; readonly value: string; readonly icon?: LucideIcon; readonly mono?: boolean }) {
  return <div className="space-y-1.5"><p className="text-xs font-medium text-muted-foreground">{label}</p><div className="flex min-w-0 items-center gap-2">{Icon ? <Icon className="size-4 shrink-0 text-muted-foreground" aria-hidden="true" /> : null}<p className={mono ? "min-w-0 truncate font-mono text-xs" : "min-w-0 break-words text-sm font-medium"}>{value}</p></div></div>;
}

function TransactionDetailSkeleton() {
  return <div className="space-y-6" aria-busy="true"><Skeleton className="h-7 w-36" /><div className="space-y-3"><Skeleton className="h-4 w-32" /><Skeleton className="h-10 w-72 max-w-full" /><Skeleton className="h-4 w-96 max-w-full" /></div><div className="grid gap-4 md:grid-cols-3">{Array.from({ length: 3 }, (_, index) => <Skeleton key={index} className="h-28 rounded-xl" />)}</div><div className="grid gap-6 xl:grid-cols-[1.35fr_0.65fr]"><Skeleton className="h-80 rounded-xl" /><Skeleton className="h-80 rounded-xl" /></div></div>;
}
