"use client";

import { RotateCcw } from "lucide-react";
import type { ReactNode } from "react";

import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

export interface InsightFilterState {
  readonly status: string;
  readonly severity: string;
  readonly type: string;
}

interface InsightFiltersProps {
  readonly filters: InsightFilterState;
  readonly onChange: (key: keyof InsightFilterState, value: string) => void;
  readonly onReset: () => void;
  readonly hasActiveFilters: boolean;
}

/** Provides URL-friendly filters for the financial insight workspace. */
export function InsightFilters({ filters, onChange, onReset, hasActiveFilters }: InsightFiltersProps) {
  return (
    <section className="rounded-2xl border bg-card p-4 shadow-sm" aria-label="Insight filters">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end">
        <FilterField label="Status">
          <Select value={filters.status || "all"} onValueChange={(value) => onChange("status", value === "all" ? "" : value ?? "")}>
            <SelectTrigger className="w-full lg:w-44"><SelectValue placeholder="All statuses" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All statuses</SelectItem>
              <SelectItem value="Active">Active</SelectItem>
              <SelectItem value="Seen">Seen</SelectItem>
              <SelectItem value="Dismissed">Dismissed</SelectItem>
              <SelectItem value="Expired">Expired</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <FilterField label="Severity">
          <Select value={filters.severity || "all"} onValueChange={(value) => onChange("severity", value === "all" ? "" : value ?? "")}>
            <SelectTrigger className="w-full lg:w-44"><SelectValue placeholder="All severities" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All severities</SelectItem>
              <SelectItem value="High">High</SelectItem>
              <SelectItem value="Medium">Medium</SelectItem>
              <SelectItem value="Low">Low</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <FilterField label="Insight type">
          <Select value={filters.type || "all"} onValueChange={(value) => onChange("type", value === "all" ? "" : value ?? "")}>
            <SelectTrigger className="w-full lg:w-72"><SelectValue placeholder="All insight types" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All insight types</SelectItem>
              <SelectItem value="UnusualTransaction">Unusual transaction</SelectItem>
              <SelectItem value="CategorySpendingIncrease">Category spending increase</SelectItem>
              <SelectItem value="MerchantSpendingIncrease">Merchant spending increase</SelectItem>
              <SelectItem value="NewMerchant">New merchant</SelectItem>
              <SelectItem value="PossibleDuplicate">Possible duplicate</SelectItem>
              <SelectItem value="SubscriptionPriceIncrease">Subscription price increase</SelectItem>
              <SelectItem value="SubscriptionPriceDecrease">Subscription price decrease</SelectItem>
              <SelectItem value="CategoryTrend">Category trend</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <div className="lg:ml-auto">
          <Button type="button" variant="ghost" onClick={onReset} disabled={!hasActiveFilters}>
            <RotateCcw aria-hidden="true" /> Reset filters
          </Button>
        </div>
      </div>
    </section>
  );
}

function FilterField({ label, children }: { readonly label: string; readonly children: ReactNode }) {
  return (
    <label className="grid gap-2 text-sm">
      <span className="text-xs font-medium text-muted-foreground">{label}</span>
      {children}
    </label>
  );
}
