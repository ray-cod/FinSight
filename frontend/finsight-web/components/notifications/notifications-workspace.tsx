"use client";

import * as React from "react";
import Link from "next/link";
import { Bell, CheckCheck, Clock3, MailWarning, RefreshCw } from "lucide-react";

import { formatRelativeDate } from "@/lib/formatting/date";
import {
    useDismissNotification,
    useMarkAllNotificationsRead,
    useMarkNotificationRead,
    useNotifications,
    useUnreadNotificationCount,
} from "@/lib/notifications/use-notifications";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState as SharedEmptyState } from "@/components/feedback/empty-state";
import { ErrorState } from "@/components/feedback/error-state";
import {
    NotificationFilters,
    type NotificationFilterState,
} from "@/components/notifications/notification-filters";
import { NotificationItem } from "@/components/notifications/notification-item";
import { useAppToast } from "@/components/feedback/use-app-toast";

const DEFAULT_FILTERS: NotificationFilterState = { view: "all", type: "" };

/** Renders the authenticated notification center. */
export function NotificationsWorkspace() {
    const notifications = useNotifications(true, 100);
    const unread = useUnreadNotificationCount();
    const markRead = useMarkNotificationRead();
    const dismiss = useDismissNotification();
    const markAll = useMarkAllNotificationsRead();
    const [filters, setFilters] =
        React.useState<NotificationFilterState>(DEFAULT_FILTERS);
    const toast = useAppToast();

    const items = React.useMemo(() => {
        return [...(notifications.data ?? [])]
            .filter((item) => filters.view === "all" || item.status !== "Read")
            .filter((item) => !filters.type || item.type === filters.type)
            .sort(
                (a, b) =>
                    new Date(b.createdAt).getTime() -
                    new Date(a.createdAt).getTime(),
            );
    }, [filters.type, filters.view, notifications.data]);

    const [now, setNow] = React.useState(() => Date.now());
    React.useEffect(() => {
        const timer = window.setInterval(() => setNow(Date.now()), 60_000);
        return () => window.clearInterval(timer);
    }, []);

    const recentCount = (notifications.data ?? []).filter(
        (item) =>
            now - new Date(item.createdAt).getTime() <= 7 * 24 * 60 * 60 * 1000,
    ).length;
    const deliveryIssueCount = (notifications.data ?? []).filter(
        (item) => item.status === "Failed" || item.status === "DeadLettered",
    ).length;
    const handleFilterChange = (
        key: keyof NotificationFilterState,
        value: string,
    ) => {
        setFilters(
            (current) =>
                ({ ...current, [key]: value }) as NotificationFilterState,
        );
    };

    return (
        <div className="space-y-6">
            <section className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
                <div className="space-y-2">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                        Alerts and updates
                    </p>
                    <h1 className="text-3xl font-semibold tracking-tight">
                        Notifications
                    </h1>
                    <p className="max-w-3xl text-sm leading-6 text-muted-foreground">
                        Keep financial events actionable without turning your
                        inbox into another dashboard. FinSight separates unread
                        attention from delivery state and keeps every alert tied
                        to the capability that generated it.
                    </p>
                </div>
                <div className="flex flex-wrap items-center gap-2">
                    <Button
                        variant="ghost"
                        onClick={() => void notifications.refetch()}
                        disabled={notifications.isFetching}
                    >
                        <RefreshCw
                            className={
                                notifications.isFetching ? "animate-spin" : ""
                            }
                            aria-hidden="true"
                        />{" "}
                        Refresh
                    </Button>
                    <Button
                        variant="outline"
                        onClick={() =>
                            markAll.mutate(undefined, {
                                onSuccess: (result) =>
                                    toast.success({
                                        title: "Notifications marked as read",
                                        description: `${result.updatedCount} notification${result.updatedCount === 1 ? "" : "s"} updated.`,
                                    }),
                                onError: (error) =>
                                    toast.error({
                                        title: "Notifications could not be updated",
                                        description:
                                            error instanceof Error
                                                ? error.message
                                                : "Please try again.",
                                    }),
                            })
                        }
                        disabled={
                            markAll.isPending || (unread.data?.count ?? 0) === 0
                        }
                    >
                        <CheckCheck aria-hidden="true" />{" "}
                        {markAll.isPending ? "Marking…" : "Mark all read"}
                    </Button>
                </div>
            </section>

            <section
                className="grid gap-4 md:grid-cols-3"
                aria-label="Notification summary"
            >
                <SummaryCard
                    icon={Bell}
                    label="Unread"
                    value={(unread.data?.count ?? 0).toString()}
                    detail="Requires attention"
                />
                <SummaryCard
                    icon={Clock3}
                    label="Recent"
                    value={recentCount.toString()}
                    detail="Last 7 days"
                />
                <SummaryCard
                    icon={MailWarning}
                    label="Delivery issues"
                    value={deliveryIssueCount.toString()}
                    detail="Requires no action to review"
                />
            </section>

            <NotificationFilters
                filters={filters}
                onChange={handleFilterChange}
            />

            {notifications.isError && !notifications.data ? (
                <ErrorState
                    title="Notifications unavailable"
                    description="FinSight could not load your notification center. Please try again."
                    onRetry={() => void notifications.refetch()}
                />
            ) : null}

            {notifications.isLoading ? <NotificationSkeleton /> : null}

            {!notifications.isLoading &&
            !notifications.isError &&
            items.length === 0 ? (
                <EmptyState
                    hasFilters={filters.view !== "all" || Boolean(filters.type)}
                />
            ) : null}

            {!notifications.isLoading && items.length > 0 ? (
                <section className="space-y-3" aria-label="Notification list">
                    {items.map((item) => (
                        <NotificationItem
                            key={item.id}
                            notification={item}
                            onMarkRead={(notificationId) =>
                                markRead.mutate(notificationId, {
                                    onSuccess: () =>
                                        toast.success({
                                            title: "Notification marked as read",
                                        }),
                                    onError: (error) =>
                                        toast.error({
                                            title: "Notification could not be updated",
                                            description:
                                                error instanceof Error
                                                    ? error.message
                                                    : "Please try again.",
                                        }),
                                })
                            }
                            onDismiss={(notificationId) =>
                                dismiss.mutate(notificationId, {
                                    onSuccess: () =>
                                        toast.success({
                                            title: "Notification dismissed",
                                        }),
                                    onError: (error) =>
                                        toast.error({
                                            title: "Notification could not be dismissed",
                                            description:
                                                error instanceof Error
                                                    ? error.message
                                                    : "Please try again.",
                                        }),
                                })
                            }
                            isMarkingRead={markRead.isPending}
                            isDismissing={dismiss.isPending}
                        />
                    ))}
                    {(notifications.data?.length ?? 0) >= 100 ? (
                        <p className="pt-2 text-center text-xs text-muted-foreground">
                            Showing the 100 most recent notifications.
                        </p>
                    ) : null}
                </section>
            ) : null}

            {notifications.isFetching && notifications.data ? (
                <p className="text-center text-xs text-muted-foreground">
                    Updating notifications…
                </p>
            ) : null}
        </div>
    );
}

function SummaryCard({
    icon: Icon,
    label,
    value,
    detail,
}: {
    readonly icon: typeof Bell;
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

function NotificationSkeleton() {
    return (
        <section
            className="space-y-3"
            aria-label="Loading notifications"
            aria-busy="true"
        >
            {Array.from({ length: 4 }, (_, index) => (
                <Card key={index}>
                    <CardContent className="flex gap-4 p-5">
                        <Skeleton className="size-10 shrink-0 rounded-xl" />
                        <div className="min-w-0 flex-1 space-y-3">
                            <Skeleton className="h-5 w-1/2" />
                            <Skeleton className="h-16 w-full" />
                            <Skeleton className="h-8 w-32" />
                        </div>
                    </CardContent>
                </Card>
            ))}
        </section>
    );
}

function EmptyState({ hasFilters }: { readonly hasFilters: boolean }) {
    return (
        <SharedEmptyState
            icon={Bell}
            title={
                hasFilters
                    ? "No notifications match this view"
                    : "You're all caught up"
            }
            description={
                hasFilters
                    ? "Try switching to all notifications or clearing the type filter."
                    : "FinSight will surface important anomalies, subscription changes, insights and security events here."
            }
            action={
                !hasFilters
                    ? {
                          label: "Review notification preferences",
                          href: "/settings?section=notifications",
                      }
                    : undefined
            }
        />
    );
}
