import { differenceInCalendarDays } from "date-fns";

/** Converts backend intelligence type names into human-readable labels. */
export function humanizeToken(value: string): string {
  return value
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/([A-Z]+)([A-Z][a-z])/g, "$1 $2")
    .trim();
}

/** Formats anomaly types for user-facing views. */
export function formatAnomalyType(type: string): string {
  const labels: Record<string, string> = {
    LargeTransaction: "Unusually large transaction",
    MerchantSpendingSpike: "Merchant spending spike",
    CategorySpendingSpike: "Category spending spike",
    NewMerchant: "New merchant",
    DuplicateTransaction: "Possible duplicate",
    MerchantFrequencySpike: "Merchant frequency spike",
    SubscriptionPriceChange: "Subscription price change",
  };
  return labels[type] ?? humanizeToken(type);
}

/** Formats insight types for user-facing views. */
export function formatInsightType(type: string): string {
  const labels: Record<string, string> = {
    UnusualTransaction: "Unusual transaction",
    CategorySpendingIncrease: "Category spending increase",
    MerchantSpendingIncrease: "Merchant spending increase",
    NewMerchant: "New merchant",
    PossibleDuplicate: "Possible duplicate",
    SubscriptionPriceIncrease: "Subscription price increase",
    SubscriptionPriceDecrease: "Subscription price decrease",
    CategoryTrend: "Category trend",
  };
  return labels[type] ?? humanizeToken(type);
}

/** Returns a normalized numeric severity rank. */
export function severityRank(value: string): number {
  return { Low: 1, Medium: 2, High: 3, Critical: 4 }[value] ?? 0;
}

/** Returns the CSS classes used for an intelligence severity indicator. */
export function severityClasses(value: string): string {
  switch (value) {
    case "Critical":
      return "border-destructive/30 bg-destructive/10 text-destructive";
    case "High":
      return "border-orange-500/30 bg-orange-500/10 text-orange-700 dark:text-orange-300";
    case "Medium":
      return "border-amber-500/30 bg-amber-500/10 text-amber-700 dark:text-amber-300";
    default:
      return "border-border bg-muted/40 text-muted-foreground";
  }
}

/** Returns a compact lifecycle indicator style. */
export function lifecycleClasses(value: string): string {
  switch (value) {
    case "Open":
    case "Active":
      return "border-primary/20 bg-primary/5 text-primary";
    case "Seen":
    case "Resolved":
      return "border-border bg-muted/40 text-muted-foreground";
    case "Dismissed":
    case "Expired":
      return "border-border bg-muted/20 text-muted-foreground";
    default:
      return "border-border bg-background text-muted-foreground";
  }
}

/** Determines whether an insight expiration is close enough to call out. */
export function isInsightExpiringSoon(expiresAt: string | null, now = new Date()): boolean {
  if (!expiresAt) return false;
  const days = differenceInCalendarDays(new Date(expiresAt), now);
  return days >= 0 && days <= 7;
}
