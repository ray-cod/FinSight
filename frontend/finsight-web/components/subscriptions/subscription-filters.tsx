"use client";

import type { ReactNode } from "react";
import { RotateCcw, Search } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import type { Subscription } from "@/lib/api/types";
import { subscriptionFrequencies, subscriptionStatuses, formatFrequency, formatSubscriptionStatus } from "@/components/subscriptions/subscription-utils";

export interface SubscriptionFilterState {
  readonly search: string;
  readonly status: string;
  readonly frequency: string;
  readonly currency: string;
}

interface SubscriptionFiltersProps {
  readonly filters: SubscriptionFilterState;
  readonly subscriptions: readonly Subscription[];
  readonly onChange: (filters: SubscriptionFilterState) => void;
}

/** Provides URL-backed filtering controls for the subscription workspace. */
export function SubscriptionFilters({ filters, subscriptions, onChange }: SubscriptionFiltersProps) {
  const currencies = [...new Set(subscriptions.map((subscription) => subscription.currency))].sort();

  const update = <K extends keyof SubscriptionFilterState>(key: K, value: SubscriptionFilterState[K]) => {
    onChange({ ...filters, [key]: value });
  };

  const hasFilters = Boolean(filters.search || filters.status !== "all" || filters.frequency !== "all" || filters.currency !== "all");

  return (
    <CardWrapper>
      <div className="flex flex-col gap-3 lg:flex-row lg:items-end">
        <div className="min-w-0 flex-1 space-y-2">
          <label htmlFor="subscription-search" className="text-xs font-medium text-muted-foreground">
            Search subscriptions
          </label>
          <div className="relative">
            <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
            <Input
              id="subscription-search"
              value={filters.search}
              onChange={(event) => update("search", event.target.value)}
              placeholder="Search by merchant"
              className="pl-9"
              autoComplete="off"
            />
          </div>
        </div>

        <FilterSelect label="Status" value={filters.status} onValueChange={(value) => update("status", value)}>
          <SelectItem value="all">All statuses</SelectItem>
          {subscriptionStatuses.map((status) => (
            <SelectItem key={status} value={status}>{formatSubscriptionStatus(status)}</SelectItem>
          ))}
        </FilterSelect>

        <FilterSelect label="Frequency" value={filters.frequency} onValueChange={(value) => update("frequency", value)}>
          <SelectItem value="all">All frequencies</SelectItem>
          {subscriptionFrequencies.map((frequency) => (
            <SelectItem key={frequency} value={frequency}>{formatFrequency(frequency)}</SelectItem>
          ))}
        </FilterSelect>

        <FilterSelect label="Currency" value={filters.currency} onValueChange={(value) => update("currency", value)}>
          <SelectItem value="all">All currencies</SelectItem>
          {currencies.map((currency) => <SelectItem key={currency} value={currency}>{currency}</SelectItem>)}
        </FilterSelect>

        {hasFilters ? (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => onChange({ search: "", status: "all", frequency: "all", currency: "all" })}
            aria-label="Clear subscription filters"
          >
            <RotateCcw aria-hidden="true" />
            Clear
          </Button>
        ) : null}
      </div>
    </CardWrapper>
  );
}

function FilterSelect({
  label,
  value,
  onValueChange,
  children,
}: {
  readonly label: string;
  readonly value: string;
  readonly onValueChange: (value: string) => void;
  readonly children: ReactNode;
}) {
  return (
    <div className="space-y-2">
      <p className="text-xs font-medium text-muted-foreground">{label}</p>
      <Select value={value} onValueChange={(nextValue) => onValueChange(String(nextValue))}>
        <SelectTrigger className="w-full sm:min-w-36">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>{children}</SelectContent>
      </Select>
    </div>
  );
}

function CardWrapper({ children }: { readonly children: ReactNode }) {
  return <div className="rounded-xl border bg-card p-4 shadow-xs">{children}</div>;
}
