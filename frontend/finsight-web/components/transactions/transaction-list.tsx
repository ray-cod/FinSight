"use client";

import Link from "next/link";
import { ArrowDownRight, ArrowUpRight, ChevronLeft, ChevronRight, CreditCard } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { EmptyState } from "@/components/feedback/empty-state";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { TransactionStatusBadge } from "@/components/transactions/transaction-status-badge";
import { formatTransactionAmount, isExpense, transactionLabel } from "@/components/transactions/transaction-utils";
import type { TransactionListItem, TransactionListResponse } from "@/lib/api/types";
import { formatDate } from "@/lib/formatting/date";

interface Props {
  readonly data: TransactionListResponse | undefined;
  readonly isLoading: boolean;
  readonly onPageChange: (page: number) => void;
}

/** Renders the paginated transaction result with accessible desktop and mobile layouts. */
export function TransactionList({ data, isLoading, onPageChange }: Props) {
  if (isLoading && !data) return <TransactionListSkeleton />;

  if (!data || data.totalCount === 0) {
    return (
      <EmptyState
        icon={CreditCard}
        title="No transactions found"
        description="Try clearing a filter or connect and synchronize a financial account to import activity into FinSight."
        action={{ label: "View accounts", href: "/accounts" }}
      />
    );
  }

  return (
    <Card className="overflow-hidden">
      <div className="hidden md:block">
        <Table>
          <TableHeader>
            <TableRow className="hover:bg-transparent">
              <TableHead className="pl-6">Transaction</TableHead>
              <TableHead>Account</TableHead>
              <TableHead>Category</TableHead>
              <TableHead>Date</TableHead>
              <TableHead className="text-right">Amount</TableHead>
              <TableHead className="pr-6">Classification</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.map((transaction) => <DesktopRow key={transaction.id} transaction={transaction} />)}
          </TableBody>
        </Table>
      </div>

      <div className="divide-y md:hidden">
        {data.items.map((transaction) => <MobileRow key={transaction.id} transaction={transaction} />)}
      </div>

      <footer className="flex flex-col gap-3 border-t bg-muted/20 px-4 py-3 sm:flex-row sm:items-center sm:justify-between sm:px-6">
        <p className="text-xs text-muted-foreground">
          Showing <span className="font-medium text-foreground">{((data.page - 1) * data.pageSize) + 1}–{Math.min(data.page * data.pageSize, data.totalCount)}</span> of <span className="font-medium text-foreground">{data.totalCount}</span>
        </p>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={() => onPageChange(data.page - 1)} disabled={!data.hasPreviousPage}><ChevronLeft aria-hidden="true" /> Previous</Button>
          <Badge variant="outline">Page {data.page} of {data.totalPages}</Badge>
          <Button variant="outline" size="sm" onClick={() => onPageChange(data.page + 1)} disabled={!data.hasNextPage}>Next <ChevronRight aria-hidden="true" /></Button>
        </div>
      </footer>
    </Card>
  );
}

function DesktopRow({ transaction }: { readonly transaction: TransactionListItem }) {
  const expense = isExpense(transaction);
  return (
    <TableRow>
      <TableCell className="pl-6">
        <Link href={`/transactions/${transaction.id}`} className="flex min-w-0 items-center gap-3 rounded-md focus-visible:outline-none focus-visible:ring-3 focus-visible:ring-ring/30">
          <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted"><TransactionDirectionIcon expense={expense} /></div>
          <div className="min-w-0">
            <p className="max-w-64 truncate font-medium">{transactionLabel(transaction)}</p>
            <p className="mt-0.5 max-w-64 truncate text-xs text-muted-foreground">{transaction.normalizedDescription || transaction.rawDescription}</p>
          </div>
        </Link>
      </TableCell>
      <TableCell>
        <div className="max-w-44 truncate text-sm">{transaction.accountName}</div>
        <div className="max-w-44 truncate text-xs text-muted-foreground">{transaction.institutionName}</div>
      </TableCell>
      <TableCell>
        <div className="max-w-36 truncate text-sm">{transaction.categoryName ?? "Uncategorized"}</div>
        {transaction.subcategoryName ? <div className="max-w-36 truncate text-xs text-muted-foreground">{transaction.subcategoryName}</div> : null}
      </TableCell>
      <TableCell>
        <div className="text-sm">{formatDate(transaction.transactionDate)}</div>
        <div className="text-xs text-muted-foreground">{transaction.type}</div>
      </TableCell>
      <TableCell className="text-right">
        <p className={expense ? "font-semibold tabular-nums" : "font-semibold tabular-nums text-emerald-700 dark:text-emerald-400"}>{formatTransactionAmount(transaction)}</p>
        <p className="text-xs text-muted-foreground">{transaction.currency}</p>
      </TableCell>
      <TableCell className="pr-6">
        <TransactionStatusBadge status={transaction.classificationStatus} />
        <div className="mt-1 flex items-center gap-1 text-[11px] text-muted-foreground">
          {transaction.classificationSource !== "None" ? transaction.classificationSource : "Awaiting classification"}
          {transaction.classificationConfidence !== null ? ` · ${Math.round(transaction.classificationConfidence * 100)}%` : null}
        </div>
      </TableCell>
    </TableRow>
  );
}

function MobileRow({ transaction }: { readonly transaction: TransactionListItem }) {
  const expense = isExpense(transaction);
  return (
    <Link href={`/transactions/${transaction.id}`} className="block p-4 transition-colors hover:bg-muted/30 focus-visible:bg-muted/30 focus-visible:outline-none sm:p-5">
      <div className="flex items-start gap-3">
        <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted"><TransactionDirectionIcon expense={expense} /></div>
        <div className="min-w-0 flex-1">
          <div className="flex items-start justify-between gap-4">
            <div className="min-w-0">
              <p className="truncate text-sm font-medium">{transactionLabel(transaction)}</p>
              <p className="mt-0.5 truncate text-xs text-muted-foreground">{transaction.accountName} · {formatDate(transaction.transactionDate)}</p>
            </div>
            <p className={expense ? "shrink-0 text-sm font-semibold tabular-nums" : "shrink-0 text-sm font-semibold tabular-nums text-emerald-700 dark:text-emerald-400"}>{formatTransactionAmount(transaction)}</p>
          </div>
          <div className="mt-3 flex flex-wrap items-center gap-2">
            <Badge variant="outline">{transaction.categoryName ?? "Uncategorized"}</Badge>
            <TransactionStatusBadge status={transaction.classificationStatus} />
            {transaction.classificationConfidence !== null ? <span className="text-xs text-muted-foreground">{Math.round(transaction.classificationConfidence * 100)}% confidence</span> : null}
          </div>
        </div>
      </div>
    </Link>
  );
}

function TransactionDirectionIcon({ expense }: { readonly expense: boolean }) {
  return expense ? <ArrowDownRight className="size-4 text-muted-foreground" aria-label="Outgoing" /> : <ArrowUpRight className="size-4 text-emerald-700 dark:text-emerald-400" aria-label="Incoming" />;
}

function TransactionListSkeleton() {
  return (
    <Card>
      <div className="hidden space-y-0 md:block">
        {Array.from({ length: 8 }, (_, index) => <div key={index} className="flex items-center gap-4 border-b px-6 py-4 last:border-b-0"><Skeleton className="size-9 rounded-full" /><div className="flex-1 space-y-2"><Skeleton className="h-4 w-52" /><Skeleton className="h-3 w-36" /></div><Skeleton className="h-4 w-24" /><Skeleton className="h-5 w-24" /></div>)}
      </div>
      <div className="divide-y md:hidden">
        {Array.from({ length: 6 }, (_, index) => <div key={index} className="flex gap-3 p-4"><Skeleton className="size-9 rounded-full" /><div className="flex-1 space-y-2"><Skeleton className="h-4 w-40" /><Skeleton className="h-3 w-28" /><Skeleton className="h-5 w-24" /></div></div>)}
      </div>
    </Card>
  );
}
