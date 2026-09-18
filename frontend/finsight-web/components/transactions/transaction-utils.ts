import type { TransactionListItem } from "@/lib/api/types";
import { formatCurrency } from "@/lib/formatting/currency";

/** Returns a stable display label for a transaction. */
export function transactionLabel(transaction: TransactionListItem): string {
  return transaction.merchantName || transaction.normalizedDescription || transaction.rawDescription;
}

/** Formats a signed transaction amount while retaining financial semantics. */
export function formatTransactionAmount(transaction: Pick<TransactionListItem, "amount" | "currency" | "type">): string {
  const amount = formatCurrency(Math.abs(transaction.amount), transaction.currency);
  const isExpense = transaction.amount < 0 || transaction.type === "Purchase" || transaction.type === "Fee";
  return isExpense ? `-${amount}` : `+${amount}`;
}

/** Indicates whether a transaction should be presented as an outgoing amount. */
export function isExpense(transaction: Pick<TransactionListItem, "amount" | "type">): boolean {
  return transaction.amount < 0 || transaction.type === "Purchase" || transaction.type === "Fee";
}
