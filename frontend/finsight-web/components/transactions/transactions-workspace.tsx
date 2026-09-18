"use client";

import * as React from "react";
import Link from "next/link";
import { CreditCard, ReceiptText, RefreshCw } from "lucide-react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";

import {
    TransactionFilters,
    type TransactionFilterState,
} from "@/components/transactions/transaction-filters";
import { TransactionList } from "@/components/transactions/transaction-list";
import { ErrorState } from "@/components/feedback/error-state";
import { Button } from "@/components/ui/button";
import { useAccounts } from "@/lib/accounts/use-accounts";
import { useCategories } from "@/lib/categories/use-categories";
import { useTransactions } from "@/lib/transactions/use-transactions";
import type { TransactionListQuery } from "@/lib/api/types";

const DEFAULT_FILTERS: TransactionFilterState = {
    search: "",
    accountId: "",
    categoryId: "",
    type: "",
    status: "",
    classificationStatus: "",
    from: "",
    to: "",
    minAmount: "",
    maxAmount: "",
    sortBy: "date",
    descending: true,
};

/** Renders the complete transaction list and filter workspace. */
export function TransactionsWorkspace() {
    const searchParams = useSearchParams();
    const router = useRouter();
    const pathname = usePathname();
    const accounts = useAccounts();
    const categories = useCategories();
    const filters = readFilters(searchParams);
    const searchInput = filters.search;

    React.useEffect(() => {
        const timer = window.setTimeout(() => {
            const current = searchParams.get("search") ?? "";
            if (searchInput === current) return;
            updateUrl(router, pathname, searchParams, {
                search: searchInput.trim() || null,
                page: "1",
            });
        }, 350);
        return () => window.clearTimeout(timer);
    }, [pathname, router, searchInput, searchParams]);

    const query = buildQuery(searchParams);
    const transactions = useTransactions(query);
    const hasActiveFilters =
        filters.search.length > 0 ||
        filters.accountId.length > 0 ||
        filters.categoryId.length > 0 ||
        filters.type.length > 0 ||
        filters.status.length > 0 ||
        filters.classificationStatus.length > 0 ||
        filters.from.length > 0 ||
        filters.to.length > 0 ||
        filters.minAmount.length > 0 ||
        filters.maxAmount.length > 0 ||
        filters.sortBy !== "date" ||
        !filters.descending;

    const setFilter = <K extends keyof TransactionFilterState>(
        key: K,
        value: TransactionFilterState[K],
    ) => {
        updateUrl(router, pathname, searchParams, {
            [key]: serializeFilter(key, value),
            page: "1",
        });
    };

    const reset = () => {
        const next = new URLSearchParams();
        router.replace(
            `${pathname}${next.toString() ? `?${next.toString()}` : ""}`,
            { scroll: false },
        );
    };

    const changePage = (nextPage: number) =>
        updateUrl(router, pathname, searchParams, { page: String(nextPage) });

    return (
        <div className="space-y-6">
            <section className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
                <div className="space-y-2">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                        Financial activity
                    </p>
                    <h1 className="text-3xl font-semibold tracking-tight">
                        Transactions
                    </h1>
                    <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
                        Review imported activity, understand how FinSight
                        classified it, and correct classifications when your
                        intent differs from the automated result.
                    </p>
                </div>
                <div className="flex shrink-0 items-center gap-2">
                    <Button asChild variant="outline">
                        <Link href="/accounts">
                            <CreditCard aria-hidden="true" /> Manage accounts
                        </Link>
                    </Button>
                </div>
            </section>

            {transactions.isError && !transactions.data ? (
                <ErrorState
                    title="Transactions unavailable"
                    description="FinSight could not load your transaction activity. Please try again."
                    onRetry={() => void transactions.refetch()}
                />
            ) : null}

            <TransactionFilters
                filters={filters}
                accounts={accounts.data}
                categories={categories.data}
                hasActiveFilters={hasActiveFilters}
                onChange={setFilter}
                onReset={reset}
            />

            <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <ReceiptText className="size-4" aria-hidden="true" />
                    <span>
                        {transactions.data?.totalCount ?? 0} transaction
                        {transactions.data?.totalCount === 1 ? "" : "s"}
                    </span>
                    {transactions.isFetching && transactions.data ? (
                        <span className="inline-flex items-center gap-1 text-xs">
                            <RefreshCw
                                className="size-3 animate-spin"
                                aria-hidden="true"
                            />{" "}
                            Updating
                        </span>
                    ) : null}
                </div>
            </div>

            <TransactionList
                data={transactions.data}
                isLoading={transactions.isLoading}
                onPageChange={changePage}
            />
        </div>
    );
}

function positiveInt(value: string | null, fallback: number): number {
    const parsed = Number(value);
    return Number.isInteger(parsed) && parsed > 0 ? parsed : fallback;
}
function boundedPageSize(value: string | null): number {
    const parsed = positiveInt(value, 25);
    return [25, 50, 100].includes(parsed) ? parsed : 25;
}

type SearchParamsLike = Pick<URLSearchParams, "get" | "toString">;

function readFilters(params: SearchParamsLike): TransactionFilterState {
    const sortBy = params.get("sortBy");
    const direction = params.get("descending");
    return {
        ...DEFAULT_FILTERS,
        search: params.get("search") ?? "",
        accountId: params.get("accountId") ?? "",
        categoryId: params.get("categoryId") ?? "",
        type: params.get("type") ?? "",
        status: params.get("status") ?? "",
        classificationStatus: params.get("classificationStatus") ?? "",
        from: params.get("from") ?? "",
        to: params.get("to") ?? "",
        minAmount: params.get("minAmount") ?? "",
        maxAmount: params.get("maxAmount") ?? "",
        sortBy: sortBy === "amount" || sortBy === "merchant" ? sortBy : "date",
        descending: direction !== "false",
    };
}

function buildQuery(params: SearchParamsLike): TransactionListQuery {
    const query: TransactionListQuery = {
        page: positiveInt(params.get("page"), 1),
        pageSize: boundedPageSize(params.get("pageSize")),
        search: optionalString(params.get("search")),
        accountId: optionalString(params.get("accountId")),
        categoryId: optionalString(params.get("categoryId")),
        type: optionalString(params.get("type")),
        status: optionalString(params.get("status")),
        classificationStatus: optionalString(
            params.get("classificationStatus"),
        ),
        from: toApiStartDate(params.get("from")),
        to: toApiEndDate(params.get("to")),
        minAmount: optionalPositiveNumber(params.get("minAmount")),
        maxAmount: optionalPositiveNumber(params.get("maxAmount")),
        sortBy:
            params.get("sortBy") === "amount" ||
            params.get("sortBy") === "merchant"
                ? (params.get("sortBy") as "amount" | "merchant")
                : "date",
        descending: params.get("descending") !== "false",
    };
    return query;
}

function optionalString(value: string | null): string | undefined {
    return value?.trim() || undefined;
}
function optionalPositiveNumber(value: string | null): number | undefined {
    if (!value?.trim()) return undefined;
    const parsed = Number(value);
    return Number.isFinite(parsed) && parsed >= 0 ? parsed : undefined;
}
function toApiStartDate(value: string | null): string | undefined {
    if (!value) return undefined;
    return new Date(`${value}T00:00:00`).toISOString();
}
function toApiEndDate(value: string | null): string | undefined {
    if (!value) return undefined;
    const date = new Date(`${value}T00:00:00`);
    date.setDate(date.getDate() + 1);
    return date.toISOString();
}

function serializeFilter<K extends keyof TransactionFilterState>(
    key: K,
    value: TransactionFilterState[K],
): string | null {
    if (key === "descending") return value === true ? null : "false";
    return String(value).trim() || null;
}

function updateUrl(
    router: ReturnType<typeof useRouter>,
    pathname: string,
    params: SearchParamsLike,
    changes: Record<string, string | null>,
) {
    const next = new URLSearchParams(params.toString());
    Object.entries(changes).forEach(([key, value]) => {
        if (!value) next.delete(key);
        else next.set(key, value);
    });
    const encoded = next.toString();
    router.replace(`${pathname}${encoded ? `?${encoded}` : ""}`, {
        scroll: false,
    });
}
