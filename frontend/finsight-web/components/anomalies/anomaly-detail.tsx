"use client";

import * as React from "react";
import Link from "next/link";
import {
    AlertTriangle,
    ArrowDownRight,
    ArrowLeft,
    ArrowRight,
    Banknote,
    CalendarClock,
    CheckCircle2,
    Clock3,
    EyeOff,
    Info,
    Landmark,
    Loader2,
    ShieldCheck,
    Sparkles,
} from "lucide-react";

import {
    IntelligenceLifecycleBadge,
    IntelligenceSeverityBadge,
} from "@/components/intelligence/intelligence-badges";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { useAccounts } from "@/lib/accounts/use-accounts";
import {
    useAnomaly,
    useDismissAnomaly,
    useResolveAnomaly,
} from "@/lib/anomalies/use-anomalies";
import { useTransaction } from "@/lib/transactions/use-transactions";
import { formatAnomalyType, severityRank } from "@/lib/intelligence/utils";
import { formatCurrency } from "@/lib/formatting/currency";
import { formatDate, formatRelativeDate } from "@/lib/formatting/date";
import { useAppToast } from "@/components/feedback/use-app-toast";

/** Renders complete anomaly evidence and user lifecycle actions. */
export function AnomalyDetail({ anomalyId }: { readonly anomalyId: string }) {
    const anomaly = useAnomaly(anomalyId);
    const transaction = useTransaction(anomaly.data?.transactionId ?? "");
    const accounts = useAccounts();
    const resolve = useResolveAnomaly();
    const dismiss = useDismissAnomaly();
    const [action, setAction] = React.useState<"resolve" | "dismiss" | null>(
        null,
    );
    const pending = resolve.isPending || dismiss.isPending;
    const toast = useAppToast();

    if (anomaly.isLoading) return <DetailSkeleton />;
    if (anomaly.isError || !anomaly.data) {
        return (
            <Card>
                <CardContent className="flex min-h-72 flex-col items-center justify-center px-6 py-12 text-center">
                    <AlertTriangle
                        className="size-6 text-muted-foreground"
                        aria-hidden="true"
                    />
                    <h1 className="mt-4 text-lg font-semibold">
                        Anomaly unavailable
                    </h1>
                    <p className="mt-1 max-w-md text-sm text-muted-foreground">
                        This anomaly could not be loaded. It may have been
                        removed or may no longer belong to your workspace.
                    </p>
                    <Button asChild variant="outline" className="mt-5">
                        <Link href="/anomalies">
                            <ArrowLeft aria-hidden="true" /> Back to anomalies
                        </Link>
                    </Button>
                </CardContent>
            </Card>
        );
    }

    const data = anomaly.data;
    const evidenceText =
        typeof data.evidence === "string"
            ? data.evidence
            : typeof data.evidence === "number" ||
                typeof data.evidence === "boolean"
              ? String(data.evidence)
              : data.evidence
                ? JSON.stringify(data.evidence, null, 2)
                : "No supporting evidence was provided.";
    const sourceAccount = accounts.data?.find(
        (account) => account.id === data.accountId,
    );
    const execute = async () => {
        try {
            if (action === "resolve") {
                await resolve.mutateAsync(data.id);
                toast.success({
                    title: "Anomaly resolved",
                    description:
                        "The anomaly has been removed from your open review queue.",
                });
            }
            if (action === "dismiss") {
                await dismiss.mutateAsync(data.id);
                toast.success({
                    title: "Anomaly dismissed",
                    description:
                        "The anomaly remains in history but is no longer active.",
                });
            }
            setAction(null);
        } catch (error) {
            toast.error({
                title: "Anomaly update failed",
                description:
                    error instanceof Error
                        ? error.message
                        : "Please try again.",
            });
        }
    };

    return (
        <div className="space-y-6">
            <Button asChild variant="ghost" size="sm">
                <Link href="/anomalies">
                    <ArrowLeft aria-hidden="true" /> Back to anomalies
                </Link>
            </Button>

            <section className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
                <div className="min-w-0 space-y-3">
                    <div className="flex flex-wrap items-center gap-2">
                        <span className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                            Anomaly detail
                        </span>
                        <IntelligenceSeverityBadge severity={data.severity} />
                        <IntelligenceLifecycleBadge status={data.status} />
                    </div>
                    <div className="flex items-start gap-4">
                        <div className="mt-1 flex size-12 shrink-0 items-center justify-center rounded-2xl bg-muted">
                            <AlertTriangle
                                className="size-6 text-muted-foreground"
                                aria-hidden="true"
                            />
                        </div>
                        <div className="min-w-0">
                            <h1 className="text-2xl font-semibold tracking-tight sm:text-3xl">
                                {data.title}
                            </h1>
                            <p className="mt-1 text-sm text-muted-foreground">
                                {formatAnomalyType(data.type)} · detected{" "}
                                {formatRelativeDate(data.detectedAt)}
                            </p>
                        </div>
                    </div>
                </div>
                {data.status === "Open" ? (
                    <div className="flex flex-wrap items-center gap-2">
                        <Button
                            variant="outline"
                            onClick={() => setAction("resolve")}
                        >
                            <CheckCircle2 aria-hidden="true" /> Resolve
                        </Button>
                        <Button
                            variant="ghost"
                            onClick={() => setAction("dismiss")}
                        >
                            <EyeOff aria-hidden="true" /> Dismiss
                        </Button>
                    </div>
                ) : null}
            </section>

            <section
                className="grid gap-4 md:grid-cols-2 xl:grid-cols-4"
                aria-label="Anomaly summary"
            >
                <MetricCard
                    icon={ShieldCheck}
                    label="Confidence"
                    value={`${Math.round(data.confidence * 100)}%`}
                    detail="Detection confidence"
                />
                <MetricCard
                    icon={Sparkles}
                    label="Detection score"
                    value={`${Math.round(data.score * 100)}%`}
                    detail="Normalized anomaly score"
                />
                <MetricCard
                    icon={CalendarClock}
                    label="Detected"
                    value={formatDate(data.detectedAt)}
                    detail={formatRelativeDate(data.detectedAt)}
                />
                <MetricCard
                    icon={Clock3}
                    label="Status"
                    value={data.status}
                    detail={
                        data.resolvedAt
                            ? `Updated ${formatRelativeDate(data.resolvedAt)}`
                            : "Awaiting review"
                    }
                />
            </section>

            <div className="grid gap-6 xl:grid-cols-[minmax(0,1.35fr)_minmax(300px,0.65fr)]">
                <Card>
                    <CardHeader>
                        <CardTitle>Why FinSight flagged this</CardTitle>
                        <CardDescription>{data.description}</CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-5">
                        <div className="rounded-xl border bg-muted/20 p-5">
                            <div className="flex items-start gap-3">
                                <Info
                                    className="mt-0.5 size-5 shrink-0 text-muted-foreground"
                                    aria-hidden="true"
                                />
                                <div>
                                    <p className="text-sm font-medium">
                                        Supporting evidence
                                    </p>
                                    <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-muted-foreground">
                                        {evidenceText}
                                    </p>
                                </div>
                            </div>
                        </div>
                        <div className="grid gap-3 sm:grid-cols-2">
                            <ScoreBar
                                label="Detection score"
                                value={data.score}
                            />
                            <ScoreBar
                                label="Confidence"
                                value={data.confidence}
                            />
                        </div>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader>
                        <CardTitle>Source context</CardTitle>
                        <CardDescription>
                            Trace the signal back to the financial records
                            behind it.
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-3">
                        {transaction.isLoading ? (
                            <>
                                <Skeleton className="h-16 w-full" />
                                <Skeleton className="h-16 w-full" />
                            </>
                        ) : transaction.data ? (
                            <Link
                                href={`/transactions/${transaction.data.id}`}
                                className="block rounded-xl border p-4 transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none"
                            >
                                <div className="flex items-center gap-3">
                                    <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted">
                                        <Banknote
                                            className="size-4"
                                            aria-hidden="true"
                                        />
                                    </div>
                                    <div className="min-w-0 flex-1">
                                        <p className="truncate text-sm font-medium">
                                            {transaction.data.merchantName}
                                        </p>
                                        <p className="mt-1 truncate text-xs text-muted-foreground">
                                            {transaction.data.categoryName ??
                                                "Uncategorized"}{" "}
                                            ·{" "}
                                            {formatDate(
                                                transaction.data
                                                    .transactionDate,
                                            )}
                                        </p>
                                    </div>
                                    <div className="text-right">
                                        <p className="text-sm font-semibold tabular-nums">
                                            {formatCurrency(
                                                transaction.data.amount,
                                                transaction.data.currency,
                                            )}
                                        </p>
                                        <p className="text-xs text-muted-foreground">
                                            View transaction{" "}
                                            <ArrowRight
                                                className="inline size-3"
                                                aria-hidden="true"
                                            />
                                        </p>
                                    </div>
                                </div>
                            </Link>
                        ) : (
                            <p className="rounded-lg border border-dashed p-4 text-sm text-muted-foreground">
                                The source transaction is currently unavailable.
                            </p>
                        )}
                        <Link
                            href={`/accounts/${data.accountId}`}
                            className="flex items-center gap-3 rounded-xl border p-4 text-sm transition-colors hover:bg-muted/40 focus-visible:bg-muted/40 focus-visible:outline-none"
                        >
                            <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted">
                                <Landmark
                                    className="size-4"
                                    aria-hidden="true"
                                />
                            </div>
                            <div className="min-w-0 flex-1">
                                <p className="font-medium">
                                    {sourceAccount?.name ?? "Source account"}
                                </p>
                                <p className="mt-1 text-xs text-muted-foreground">
                                    {sourceAccount?.currency ?? "Account"}
                                </p>
                            </div>
                            <ArrowRight
                                className="size-4 text-muted-foreground"
                                aria-hidden="true"
                            />
                        </Link>
                    </CardContent>
                    <CardFooter className="border-t bg-muted/10">
                        <p className="text-xs leading-5 text-muted-foreground">
                            Detection type: {formatAnomalyType(data.type)}.
                        </p>
                    </CardFooter>
                </Card>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Signal lifecycle</CardTitle>
                    <CardDescription>
                        How this anomaly has moved through FinSight’s review
                        workflow.
                    </CardDescription>
                </CardHeader>
                <CardContent className="grid gap-5 sm:grid-cols-3">
                    <TimelineField
                        title="Detected"
                        value={formatDate(data.detectedAt)}
                        detail="Signal created from processed transaction data"
                    />
                    <TimelineField
                        title="Current state"
                        value={data.status}
                        detail={
                            data.status === "Open"
                                ? "Awaiting user review"
                                : "User review completed"
                        }
                    />
                    <TimelineField
                        title="Resolved / dismissed"
                        value={
                            data.resolvedAt
                                ? formatDate(data.resolvedAt)
                                : "Not yet"
                        }
                        detail={
                            data.resolvedAt
                                ? formatRelativeDate(data.resolvedAt)
                                : "No lifecycle action recorded"
                        }
                    />
                </CardContent>
            </Card>

            <Dialog
                open={action !== null}
                onOpenChange={(open) => {
                    if (!open && !pending) setAction(null);
                }}
            >
                <DialogContent className="sm:max-w-md">
                    <DialogHeader>
                        <DialogTitle>
                            {action === "resolve"
                                ? "Resolve anomaly?"
                                : "Dismiss anomaly?"}
                        </DialogTitle>
                        <DialogDescription>
                            {action === "resolve"
                                ? "This marks the signal as reviewed and removes it from the open-anomaly queue."
                                : "This dismisses the signal while retaining it in the anomaly history."}
                        </DialogDescription>
                    </DialogHeader>
                    {resolve.isError || dismiss.isError ? (
                        <p
                            role="alert"
                            className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive"
                        >
                            The anomaly could not be updated. Please try again.
                        </p>
                    ) : null}
                    <DialogFooter>
                        <Button
                            variant="outline"
                            onClick={() => setAction(null)}
                            disabled={pending}
                        >
                            Cancel
                        </Button>
                        <Button
                            variant={
                                action === "dismiss" ? "destructive" : "default"
                            }
                            onClick={() => void execute()}
                            disabled={pending}
                        >
                            {pending ? (
                                <>
                                    <Loader2
                                        className="animate-spin"
                                        aria-hidden="true"
                                    />{" "}
                                    Updating…
                                </>
                            ) : action === "resolve" ? (
                                "Resolve anomaly"
                            ) : (
                                "Dismiss anomaly"
                            )}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </div>
    );
}

function MetricCard({
    icon: Icon,
    label,
    value,
    detail,
}: {
    readonly icon: typeof ShieldCheck;
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
                <p className="truncate text-lg font-semibold tabular-nums sm:text-xl">
                    {value}
                </p>
                <p className="mt-1 text-xs text-muted-foreground">{detail}</p>
            </CardContent>
        </Card>
    );
}

function ScoreBar({
    label,
    value,
}: {
    readonly label: string;
    readonly value: number;
}) {
    const percent = Math.round(value * 100);
    return (
        <div className="rounded-lg border p-3">
            <div className="flex items-center justify-between gap-3 text-xs">
                <span className="text-muted-foreground">{label}</span>
                <span className="font-semibold tabular-nums">{percent}%</span>
            </div>
            <div className="mt-2 h-2 rounded-full bg-muted">
                <div
                    className="h-2 rounded-full bg-foreground transition-all"
                    style={{ width: `${percent}%` }}
                />
            </div>
        </div>
    );
}

function TimelineField({
    title,
    value,
    detail,
}: {
    readonly title: string;
    readonly value: string;
    readonly detail: string;
}) {
    return (
        <div className="rounded-xl border bg-muted/20 p-4">
            <p className="text-xs text-muted-foreground">{title}</p>
            <p className="mt-1 font-medium tabular-nums">{value}</p>
            <p className="mt-2 text-xs leading-5 text-muted-foreground">
                {detail}
            </p>
        </div>
    );
}

function DetailSkeleton() {
    return (
        <div className="space-y-6" aria-busy="true">
            {Array.from({ length: 8 }, (_, index) => (
                <Skeleton
                    key={index}
                    className={index < 2 ? "h-10" : "h-28 rounded-xl"}
                />
            ))}
        </div>
    );
}
