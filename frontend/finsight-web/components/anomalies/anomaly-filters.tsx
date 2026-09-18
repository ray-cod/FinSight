"use client";

import { RotateCcw } from "lucide-react";
import type { ReactNode } from "react";

import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

export interface AnomalyFilterState {
  readonly status: string;
  readonly severity: string;
  readonly type: string;
}

interface AnomalyFiltersProps {
  readonly filters: AnomalyFilterState;
  readonly onChange: (key: keyof AnomalyFilterState, value: string) => void;
  readonly onReset: () => void;
  readonly hasActiveFilters: boolean;
}

/** Provides URL-friendly filters for the anomaly workspace. */
export function AnomalyFilters({ filters, onChange, onReset, hasActiveFilters }: AnomalyFiltersProps) {
  return (
    <section className="rounded-2xl border bg-card p-4 shadow-sm" aria-label="Anomaly filters">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end">
        <FilterField label="Status">
          <Select value={filters.status || "all"} onValueChange={(value) => onChange("status", value === "all" ? "" : value ?? "")}>
            <SelectTrigger className="w-full lg:w-44"><SelectValue placeholder="All statuses" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All statuses</SelectItem>
              <SelectItem value="Open">Open</SelectItem>
              <SelectItem value="Resolved">Resolved</SelectItem>
              <SelectItem value="Dismissed">Dismissed</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <FilterField label="Severity">
          <Select value={filters.severity || "all"} onValueChange={(value) => onChange("severity", value === "all" ? "" : value ?? "")}>
            <SelectTrigger className="w-full lg:w-44"><SelectValue placeholder="All severities" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All severities</SelectItem>
              <SelectItem value="Critical">Critical</SelectItem>
              <SelectItem value="High">High</SelectItem>
              <SelectItem value="Medium">Medium</SelectItem>
              <SelectItem value="Low">Low</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <FilterField label="Signal type">
          <Select value={filters.type || "all"} onValueChange={(value) => onChange("type", value === "all" ? "" : value ?? "")}>
            <SelectTrigger className="w-full lg:w-60"><SelectValue placeholder="All signal types" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All signal types</SelectItem>
              <SelectItem value="LargeTransaction">Large transaction</SelectItem>
              <SelectItem value="MerchantSpendingSpike">Merchant spending spike</SelectItem>
              <SelectItem value="CategorySpendingSpike">Category spending spike</SelectItem>
              <SelectItem value="NewMerchant">New merchant</SelectItem>
              <SelectItem value="DuplicateTransaction">Possible duplicate</SelectItem>
              <SelectItem value="MerchantFrequencySpike">Merchant frequency spike</SelectItem>
              <SelectItem value="SubscriptionPriceChange">Subscription price change</SelectItem>
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
