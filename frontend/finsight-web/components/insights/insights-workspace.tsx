"use client";

import * as React from "react";
import Link from "next/link";
import {
    ArrowRight,
    CheckCircle2,
    Lightbulb,
    RefreshCw,
    ShieldCheck,
    Sparkles,
} from "lucide-react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";

import { InsightCard } from "@/components/insights/insight-card";
import {
    InsightFilters,
    type InsightFilterState,
} from "@/components/insights/insight-filters";
import { EmptyState } from "@/components/feedback/empty-state";
import { ErrorState } from "@/components/feedback/error-state";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import type { Insight } from "@/lib/api/types";
import { severityRank } from "@/lib/intelligence/utils";
import { useInsights } from "@/lib/insights/use-insights";
import { formatRelativeDate } from "@/lib/formatting/date";

const DEFAULT_FILTERS: InsightFilterState = {
    status: "Active",
    severity: "",
    type: "",
};

/** Renders the complete financial insights workspace. */
export function InsightsWorkspace() {
    const router = useRouter();
    const pathname = usePathname();
    const searchParams = useSearchParams();
    const filters = readFilters(searchParams);
    const insights = useInsights(true, 500);

    const updateFilter = (key: keyof InsightFilterState, value: string) =>
        updateUrl(router, pathname, searchParams, { [key]: value || null });
    const reset = () => router.replace(pathname, { scroll: false });

    const filtered = React.useMemo(
        () =>
            (insights.data ?? [])
                .filter(
                    (item) => !filters.status || item.status === filters.status,
                )
                .filter(
                    (item) =>
                        !filters.severity || item.severity === filters.severity,
                )
                .filter((item) => !filters.type || item.type === filters.type)
                .sort((a, b) => {
                    const severity =
                        severityRank(b.severity) - severityRank(a.severity);
                    if (severity !== 0) return severity;
                    return (
                        new Date(b.occurredAt).getTime() -
                        new Date(a.occurredAt).getTime()
                    );
                }),
        [filters.severity, filters.status, filters.type, insights.data],
    );

    const [now, setNow] = React.useState(() => Date.now());
    React.useEffect(() => {
        const timer = window.setInterval(() => setNow(Date.now()), 60_000);
        return () => window.clearInterval(timer);
    }, []);

    const activeCount = (insights.data ?? []).filter(
        (item) => item.status === "Active",
    ).length;
    const unreadCount = activeCount;
    const highCount = (insights.data ?? []).filter(
        (item) =>
            item.severity === "High" &&
            item.status !== "Dismissed" &&
            item.status !== "Expired",
    ).length;
    const recentCount = (insights.data ?? []).filter(
        (item) =>
            now - new Date(item.createdAt).getTime() <=
            30 * 24 * 60 * 60 * 1000,
    ).length;

    return (
        <div className="space-y-6">
            <section className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
                <div className="space-y-2">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                        Financial intelligence
                    </p>
                    <h1 className="text-3xl font-semibold tracking-tight">
                        Insights
                    </h1>
                    <p className="max-w-3xl text-sm leading-6 text-muted-foreground">
                        Turn detected patterns into explanations you can act on.
                        FinSight links each insight back to the underlying
                        anomaly or transaction so the reasoning remains
                        inspectable.
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <Button asChild variant="outline">
                        <Link href="/anomalies">
                            <ShieldCheck aria-hidden="true" /> Review anomaly
                            signals
                        </Link>
                    </Button>
                    <Button
                        variant="ghost"
                        onClick={() => void insights.refetch()}
                        disabled={insights.isFetching}
                    >
                        <RefreshCw
                            className={
                                insights.isFetching ? "animate-spin" : ""
                            }
                            aria-hidden="true"
                        />{" "}
                        Refresh
                    </Button>
                </div>
            </section>

            <section
                className="grid gap-4 md:grid-cols-2 xl:grid-cols-4"
                aria-label="Insight summary"
            >
                <SummaryCard
                    icon={Lightbulb}
                    label="Active insights"
                    value={activeCount.toString()}
                    detail="Currently available"
                />
                <SummaryCard
                    icon={Sparkles}
                    label="Needs review"
                    value={unreadCount.toString()}
                    detail="Active and unseen"
                />
                <SummaryCard
                    icon={ShieldCheck}
                    label="High priority"
                    value={highCount.toString()}
                    detail="High-severity insights"
                />
                <SummaryCard
                    icon={CheckCircle2}
                    label="Generated this month"
                    value={recentCount.toString()}
                    detail="Last 30 days"
                />
            </section>

            <InsightFilters
                filters={filters}
                onChange={updateFilter}
                onReset={reset}
                hasActiveFilters={hasActiveFilters(filters)}
            />

            {insights.isError && !insights.data ? (
                <ErrorState
                    title="Insights unavailable"
                    description="FinSight could not load your financial explanations. Please try again."
                    onRetry={() => void insights.refetch()}
                />
            ) : null}
            {insights.isLoading ? <InsightSkeletonGrid /> : null}
            {!insights.isLoading &&
            !insights.isError &&
            filtered.length === 0 ? (
                <IntelligenceEmptyState
                    hasFilters={hasActiveFilters(filters)}
                    onReset={reset}
                />
            ) : null}
            {!insights.isLoading && filtered.length > 0 ? (
                <section
                    className="grid gap-5 lg:grid-cols-2"
                    aria-label="Financial insights"
                >
                    {filtered.map((insight) => (
                        <InsightCard key={insight.id} insight={insight} />
                    ))}
                </section>
            ) : null}
            {insights.isFetching && insights.data ? (
                <p className="text-center text-xs text-muted-foreground">
                    Updating financial insights…
                </p>
            ) : null}
        </div>
    );
}

function readFilters(params: Pick<URLSearchParams, "get">): InsightFilterState {
    return {
        ...DEFAULT_FILTERS,
        status: ["Active", "Seen", "Dismissed", "Expired"].includes(
            params.get("status") ?? "",
        )
            ? params.get("status")!
            : DEFAULT_FILTERS.status,
        severity: ["Low", "Medium", "High"].includes(
            params.get("severity") ?? "",
        )
            ? params.get("severity")!
            : "",
        type: params.get("type") ?? "",
    };
}

function hasActiveFilters(filters: InsightFilterState): boolean {
    return (
        filters.status !== DEFAULT_FILTERS.status ||
        Boolean(filters.severity) ||
        Boolean(filters.type)
    );
}

function updateUrl(
    router: ReturnType<typeof useRouter>,
    pathname: string,
    params: Pick<URLSearchParams, "toString">,
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

function SummaryCard({
    icon: Icon,
    label,
    value,
    detail,
}: {
    readonly icon: typeof Lightbulb;
    readonly label: string;
    readonly value: string;
    readonly detail: string;
}) {
    return (
        <Card>
            <CardHeader className="pb-2">
                <div className="flex items-center justify-between gap-3">
                    <CardDescription>{label}</CardDescription>
                    <Icon
                        className="size-4 text-muted-foreground"
                        aria-hidden="true"
                    />
                </div>
            </CardHeader>
            <CardContent>
                <p className="text-2xl font-semibold tracking-tight tabular-nums">
                    {value}
                </p>
                <p className="mt-1 text-xs text-muted-foreground">{detail}</p>
            </CardContent>
        </Card>
    );
}

function InsightSkeletonGrid() {
    return (
        <section
            className="grid gap-5 lg:grid-cols-2"
            aria-label="Loading insights"
            aria-busy="true"
        >
            {Array.from({ length: 4 }, (_, index) => (
                <Card key={index}>
                    <CardHeader>
                        <Skeleton className="h-12 w-full" />
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <Skeleton className="h-24 w-full" />
                        <Skeleton className="h-20 w-full" />
                        <Skeleton className="h-9 w-full" />
                    </CardContent>
                </Card>
            ))}
        </section>
    );
}

function IntelligenceEmptyState({
    hasFilters,
    onReset,
}: {
    readonly hasFilters: boolean;
    readonly onReset: () => void;
}) {
    return (
        <EmptyState
            icon={Lightbulb}
            title={
                hasFilters
                    ? "No insights match these filters"
                    : "No financial insights yet"
            }
            description={
                hasFilters
                    ? "Adjust or clear your filters to review a broader set of explanations."
                    : "As FinSight processes more activity, detected anomalies and trends will generate user-readable insights here."
            }
            action={
                hasFilters
                    ? { label: "Reset filters", onClick: onReset }
                    : { label: "Review transactions", href: "/transactions" }
            }
        />
    );
}
