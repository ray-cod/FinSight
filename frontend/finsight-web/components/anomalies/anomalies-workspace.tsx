"use client";

import * as React from "react";
import Link from "next/link";
import {
    AlertTriangle,
    ArrowRight,
    CircleCheck,
    RefreshCw,
    ShieldAlert,
} from "lucide-react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";

import { AnomalyCard } from "@/components/anomalies/anomaly-card";
import {
    AnomalyFilters,
    type AnomalyFilterState,
} from "@/components/anomalies/anomaly-filters";
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
import type { Anomaly } from "@/lib/api/types";
import { severityRank } from "@/lib/intelligence/utils";
import { useAnomalies } from "@/lib/anomalies/use-anomalies";
import { formatRelativeDate } from "@/lib/formatting/date";

const DEFAULT_FILTERS: AnomalyFilterState = {
    status: "Open",
    severity: "",
    type: "",
};

/** Renders the complete anomaly review workspace. */
export function AnomaliesWorkspace() {
    const router = useRouter();
    const pathname = usePathname();
    const searchParams = useSearchParams();
    const filters = readFilters(searchParams);
    const includeResolved = filters.status !== "Open";
    const anomalies = useAnomalies(includeResolved, 500);

    const updateFilter = (key: keyof AnomalyFilterState, value: string) => {
        updateUrl(router, pathname, searchParams, { [key]: value || null });
    };

    const reset = () => router.replace(pathname, { scroll: false });

    const filtered = React.useMemo(() => {
        const items = anomalies.data ?? [];
        return items
            .filter((item) => !filters.status || item.status === filters.status)
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
                    new Date(b.detectedAt).getTime() -
                    new Date(a.detectedAt).getTime()
                );
            });
    }, [anomalies.data, filters.severity, filters.status, filters.type]);

    const [now, setNow] = React.useState(() => Date.now());
    React.useEffect(() => {
        const timer = window.setInterval(() => setNow(Date.now()), 60_000);
        return () => window.clearInterval(timer);
    }, []);

    const openCount = (anomalies.data ?? []).filter(
        (item) => item.status === "Open",
    ).length;
    const priorityCount = (anomalies.data ?? []).filter(
        (item) => item.status === "Open" && severityRank(item.severity) >= 3,
    ).length;
    const recentCount = (anomalies.data ?? []).filter(
        (item) =>
            now - new Date(item.detectedAt).getTime() <=
            7 * 24 * 60 * 60 * 1000,
    ).length;
    const lastDetected = [...(anomalies.data ?? [])].sort(
        (a, b) =>
            new Date(b.detectedAt).getTime() - new Date(a.detectedAt).getTime(),
    )[0];

    return (
        <div className="space-y-6">
            <section className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
                <div className="space-y-2">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                        Financial intelligence
                    </p>
                    <h1 className="text-3xl font-semibold tracking-tight">
                        Anomalies
                    </h1>
                    <p className="max-w-3xl text-sm leading-6 text-muted-foreground">
                        Review unusual financial activity detected from your
                        transaction history. Each signal includes its detection
                        confidence, statistical evidence, and a direct path back
                        to the source transaction.
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <Button asChild variant="outline">
                        <Link href="/transactions">
                            <AlertTriangle aria-hidden="true" /> Review
                            transactions
                        </Link>
                    </Button>
                    <Button
                        variant="ghost"
                        onClick={() => void anomalies.refetch()}
                        disabled={anomalies.isFetching}
                    >
                        <RefreshCw
                            className={
                                anomalies.isFetching ? "animate-spin" : ""
                            }
                            aria-hidden="true"
                        />{" "}
                        Refresh
                    </Button>
                </div>
            </section>

            <section
                className="grid gap-4 md:grid-cols-2 xl:grid-cols-4"
                aria-label="Anomaly summary"
            >
                <SummaryCard
                    icon={ShieldAlert}
                    label="Open signals"
                    value={openCount.toString()}
                    detail="Awaiting review"
                />
                <SummaryCard
                    icon={AlertTriangle}
                    label="Priority signals"
                    value={priorityCount.toString()}
                    detail="High or critical"
                />
                <SummaryCard
                    icon={CircleCheck}
                    label="Detected this week"
                    value={recentCount.toString()}
                    detail="Across your history"
                />
                <SummaryCard
                    icon={RefreshCw}
                    label="Latest detection"
                    value={
                        lastDetected
                            ? formatRelativeDate(lastDetected.detectedAt)
                            : "—"
                    }
                    detail="Most recent signal"
                />
            </section>

            <AnomalyFilters
                filters={filters}
                onChange={updateFilter}
                onReset={reset}
                hasActiveFilters={hasActiveFilters(filters)}
            />

            {anomalies.isError && !anomalies.data ? (
                <ErrorState
                    title="Anomalies unavailable"
                    description="FinSight could not load your financial anomaly signals. Please try again."
                    onRetry={() => void anomalies.refetch()}
                />
            ) : null}

            {anomalies.isLoading ? <AnomalySkeletonGrid /> : null}

            {!anomalies.isLoading &&
            !anomalies.isError &&
            filtered.length === 0 ? (
                <IntelligenceEmptyState
                    hasFilters={hasActiveFilters(filters)}
                    onReset={reset}
                />
            ) : null}

            {!anomalies.isLoading && filtered.length > 0 ? (
                <section
                    className="grid gap-5 lg:grid-cols-2"
                    aria-label="Anomaly signals"
                >
                    {filtered.map((anomaly) => (
                        <AnomalyCard key={anomaly.id} anomaly={anomaly} />
                    ))}
                </section>
            ) : null}

            {anomalies.isFetching && anomalies.data ? (
                <p className="text-center text-xs text-muted-foreground">
                    Updating anomaly intelligence…
                </p>
            ) : null}
        </div>
    );
}

function readFilters(params: Pick<URLSearchParams, "get">): AnomalyFilterState {
    return {
        ...DEFAULT_FILTERS,
        status: ["Open", "Resolved", "Dismissed"].includes(
            params.get("status") ?? "",
        )
            ? params.get("status")!
            : DEFAULT_FILTERS.status,
        severity: ["Low", "Medium", "High", "Critical"].includes(
            params.get("severity") ?? "",
        )
            ? params.get("severity")!
            : "",
        type: params.get("type") ?? "",
    };
}

function hasActiveFilters(filters: AnomalyFilterState): boolean {
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
    readonly icon: typeof ShieldAlert;
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

function AnomalySkeletonGrid() {
    return (
        <section
            className="grid gap-5 lg:grid-cols-2"
            aria-label="Loading anomalies"
            aria-busy="true"
        >
            {Array.from({ length: 4 }, (_, index) => (
                <Card key={index}>
                    <CardHeader>
                        <Skeleton className="h-10 w-full" />
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <Skeleton className="h-16 w-full" />
                        <div className="grid grid-cols-2 gap-3">
                            <Skeleton className="h-20" />
                            <Skeleton className="h-20" />
                        </div>
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
            icon={ShieldAlert}
            title={
                hasFilters
                    ? "No anomalies match these filters"
                    : "No unusual activity detected"
            }
            description={
                hasFilters
                    ? "Adjust or clear your filters to review a broader set of financial signals."
                    : "FinSight has not found an open signal in the available transaction history. New patterns will appear here as activity is processed."
            }
            action={
                hasFilters
                    ? { label: "Reset filters", onClick: onReset }
                    : { label: "Review transactions", href: "/transactions" }
            }
        />
    );
}
