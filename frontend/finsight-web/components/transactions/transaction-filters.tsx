"use client";

import { CalendarDays, Filter, RotateCcw, Search } from "lucide-react";
import * as React from "react";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import type { Account, Category } from "@/lib/api/types";

export interface TransactionFilterState {
    readonly search: string;
    readonly accountId: string;
    readonly categoryId: string;
    readonly type: string;
    readonly status: string;
    readonly classificationStatus: string;
    readonly from: string;
    readonly to: string;
    readonly minAmount: string;
    readonly maxAmount: string;
    readonly sortBy: "date" | "amount" | "merchant";
    readonly descending: boolean;
}

interface Props {
    readonly filters: TransactionFilterState;
    readonly accounts: readonly Account[] | undefined;
    readonly categories: readonly Category[] | undefined;
    readonly hasActiveFilters: boolean;
    readonly onChange: <K extends keyof TransactionFilterState>(
        key: K,
        value: TransactionFilterState[K],
    ) => void;
    readonly onReset: () => void;
}

/** Renders server-backed transaction filters with compact responsive behavior. */
export function TransactionFilters({
    filters,
    accounts,
    categories,
    hasActiveFilters,
    onChange,
    onReset,
}: Props) {
    const [showAdvanced, setShowAdvanced] = React.useState(false);
    const isAdvancedVisible = showAdvanced || hasActiveFilters;

    return (
        <Card>
            <CardContent className="space-y-4 pt-4">
                <div className="flex flex-col gap-3 lg:flex-row lg:items-center">
                    <div className="relative min-w-0 flex-1">
                        <Search
                            className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
                            aria-hidden="true"
                        />
                        <Input
                            value={filters.search}
                            onChange={(event) =>
                                onChange("search", event.target.value)
                            }
                            placeholder="Search merchants, descriptions or accounts"
                            aria-label="Search transactions"
                            className="pl-9"
                        />
                    </div>
                    <Button
                        variant="outline"
                        onClick={() => setShowAdvanced((value) => !value)}
                        aria-expanded={showAdvanced}
                    >
                        <Filter aria-hidden="true" />
                        Filters
                        {hasActiveFilters ? (
                            <span className="rounded-full bg-primary px-1.5 py-0.5 text-[10px] text-primary-foreground">
                                Active
                            </span>
                        ) : null}
                    </Button>
                    {hasActiveFilters ? (
                        <Button variant="ghost" onClick={onReset}>
                            <RotateCcw aria-hidden="true" />
                            Reset
                        </Button>
                    ) : null}
                </div>

                {isAdvancedVisible ? (
                    <div className="grid gap-4 border-t pt-4 md:grid-cols-2 xl:grid-cols-4">
                        <FilterField label="Account">
                            <select
                                value={filters.accountId}
                                onChange={(event) =>
                                    onChange("accountId", event.target.value)
                                }
                                className="h-9 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30"
                            >
                                <option value="">All accounts</option>
                                {accounts?.map((account) => (
                                    <option key={account.id} value={account.id}>
                                        {account.name} · {account.currency}
                                    </option>
                                ))}
                            </select>
                        </FilterField>

                        <FilterField label="Category">
                            <select
                                value={filters.categoryId}
                                onChange={(event) =>
                                    onChange("categoryId", event.target.value)
                                }
                                className="h-9 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30"
                            >
                                <option value="">All categories</option>
                                {categories?.map((category) => (
                                    <option
                                        key={category.id}
                                        value={category.id}
                                    >
                                        {category.name}
                                    </option>
                                ))}
                            </select>
                        </FilterField>

                        <FilterField label="Type">
                            <select
                                value={filters.type}
                                onChange={(event) =>
                                    onChange("type", event.target.value)
                                }
                                className="h-9 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30"
                            >
                                <option value="">All types</option>
                                {[
                                    "Purchase",
                                    "Deposit",
                                    "Transfer",
                                    "Fee",
                                    "Adjustment",
                                    "Other",
                                ].map((value) => (
                                    <option key={value} value={value}>
                                        {value}
                                    </option>
                                ))}
                            </select>
                        </FilterField>

                        <FilterField label="Classification">
                            <select
                                value={filters.classificationStatus}
                                onChange={(event) =>
                                    onChange(
                                        "classificationStatus",
                                        event.target.value,
                                    )
                                }
                                className="h-9 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30"
                            >
                                <option value="">All classifications</option>
                                {[
                                    "Classified",
                                    "Uncertain",
                                    "Pending",
                                    "Failed",
                                    "UserCorrected",
                                ].map((value) => (
                                    <option key={value} value={value}>
                                        {value === "UserCorrected"
                                            ? "User corrected"
                                            : value}
                                    </option>
                                ))}
                            </select>
                        </FilterField>

                        <FilterField label="Transaction status">
                            <select
                                value={filters.status}
                                onChange={(event) =>
                                    onChange("status", event.target.value)
                                }
                                className="h-9 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30"
                            >
                                <option value="">All statuses</option>
                                {["Imported", "Pending", "Cancelled"].map(
                                    (value) => (
                                        <option key={value} value={value}>
                                            {value}
                                        </option>
                                    ),
                                )}
                            </select>
                        </FilterField>

                        <FilterField label="From">
                            <DateInput
                                value={filters.from}
                                onChange={(value) => onChange("from", value)}
                            />
                        </FilterField>

                        <FilterField label="To">
                            <DateInput
                                value={filters.to}
                                onChange={(value) => onChange("to", value)}
                            />
                        </FilterField>

                        <div className="grid grid-cols-2 gap-2">
                            <FilterField label="Min amount">
                                <Input
                                    inputMode="decimal"
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={filters.minAmount}
                                    onChange={(event) =>
                                        onChange(
                                            "minAmount",
                                            event.target.value,
                                        )
                                    }
                                    placeholder="0.00"
                                />
                            </FilterField>
                            <FilterField label="Max amount">
                                <Input
                                    inputMode="decimal"
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={filters.maxAmount}
                                    onChange={(event) =>
                                        onChange(
                                            "maxAmount",
                                            event.target.value,
                                        )
                                    }
                                    placeholder="0.00"
                                />
                            </FilterField>
                        </div>

                        <FilterField label="Sort">
                            <select
                                value={`${filters.sortBy}:${filters.descending ? "desc" : "asc"}`}
                                onChange={(event) => {
                                    const [sortBy, direction] =
                                        event.target.value.split(":") as [
                                            "date" | "amount" | "merchant",
                                            "asc" | "desc",
                                        ];
                                    onChange("sortBy", sortBy);
                                    onChange(
                                        "descending",
                                        direction === "desc",
                                    );
                                }}
                                className="h-9 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30"
                            >
                                <option value="date:desc">Newest first</option>
                                <option value="date:asc">Oldest first</option>
                                <option value="amount:desc">
                                    Largest amount
                                </option>
                                <option value="amount:asc">
                                    Smallest amount
                                </option>
                                <option value="merchant:asc">
                                    Merchant A–Z
                                </option>
                                <option value="merchant:desc">
                                    Merchant Z–A
                                </option>
                            </select>
                        </FilterField>
                    </div>
                ) : null}
            </CardContent>
        </Card>
    );
}

function FilterField({
    label,
    children,
}: {
    readonly label: string;
    readonly children: React.ReactNode;
}) {
    return (
        <label className="space-y-1.5">
            <span className="text-xs font-medium text-muted-foreground">
                {label}
            </span>
            {children}
        </label>
    );
}

function DateInput({
    value,
    onChange,
}: {
    readonly value: string;
    readonly onChange: (value: string) => void;
}) {
    return (
        <div className="relative">
            <CalendarDays
                className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
                aria-hidden="true"
            />
            <Input
                type="date"
                value={value}
                onChange={(event) => onChange(event.target.value)}
                className="pl-9"
            />
        </div>
    );
}
