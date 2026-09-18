import Link from "next/link";
import { ArrowRight, Building2, CircleDollarSign, CreditCard, Landmark, PiggyBank, TrendingUp, WalletCards } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import type { Account } from "@/lib/api/types";
import { formatCurrency } from "@/lib/formatting/currency";

const iconByType = {
  Checking: Landmark,
  Savings: PiggyBank,
  CreditCard,
  Investment: TrendingUp,
  Other: WalletCards,
} as const;

const typeLabels: Record<string, string> = {
  Checking: "Checking",
  Savings: "Savings",
  CreditCard: "Credit card",
  Investment: "Investment",
  Other: "Other",
};

/** Renders a financial account summary card. */
export function AccountCard({ account, institutionName }: { readonly account: Account; readonly institutionName: string }) {
  const Icon = iconByType[account.type as keyof typeof iconByType] ?? WalletCards;
  return (
    <Link href={`/accounts/${account.id}`} className="group block rounded-xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
      <Card className="h-full transition-[transform,box-shadow] duration-200 group-hover:-translate-y-0.5 group-hover:shadow-md">
        <CardHeader>
          <div className="flex items-start justify-between gap-4">
            <div className="flex min-w-0 items-center gap-3">
              <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-muted text-foreground">
                <Icon className="size-5" aria-hidden="true" />
              </div>
              <div className="min-w-0">
                <CardTitle className="truncate">{account.name}</CardTitle>
                <CardDescription className="mt-0.5 truncate">{institutionName} · {typeLabels[account.type] ?? account.type}</CardDescription>
              </div>
            </div>
            <Badge variant={account.status === "Active" ? "outline" : "secondary"}>{account.status}</Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="rounded-lg border bg-muted/20 p-4">
            <div className="flex items-center gap-2 text-xs text-muted-foreground">
              <CircleDollarSign className="size-3.5" aria-hidden="true" />
              Current balance
            </div>
            <p className="mt-1 text-2xl font-semibold tracking-tight tabular-nums">
              {formatCurrency(account.currentBalance, account.currency)}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">Available {formatCurrency(account.availableBalance, account.currency)}</p>
          </div>
          <div className="flex items-center justify-between gap-4 text-xs text-muted-foreground">
            <span className="flex items-center gap-1.5"><Building2 className="size-3.5" aria-hidden="true" />{account.currency}</span>
            <span className="flex items-center gap-1 font-medium text-foreground group-hover:gap-2 transition-[gap]">View account <ArrowRight className="size-3.5" aria-hidden="true" /></span>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}
