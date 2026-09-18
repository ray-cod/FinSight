"use client";

import Link from "next/link";
import {
    AlertTriangle,
    ArrowUpRight,
    BellRing,
    CheckCircle2,
    CircleDollarSign,
    Clock3,
    MailWarning,
    ShieldAlert,
    Sparkles,
    WalletCards,
} from "lucide-react";

import type { Notification } from "@/lib/api/types";
import { formatRelativeDate } from "@/lib/formatting/date";
import {
    isNotificationUnread,
    notificationHref,
    notificationStatusLabel,
    notificationTypeLabel,
} from "@/lib/notifications/notification-utils";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

interface NotificationItemProps {
    readonly notification: Notification;
    readonly onMarkRead: (notificationId: string) => void;
    readonly onDismiss: (notificationId: string) => void;
    readonly isMarkingRead: boolean;
    readonly isDismissing: boolean;
}

function NotificationIcon({ type }: { readonly type: Notification["type"] }) {
    switch (type) {
        case "AnomalyDetected":
            return <ShieldAlert className="size-5" aria-hidden="true" />;
        case "SubscriptionPriceChanged":
            return <CircleDollarSign className="size-5" aria-hidden="true" />;
        case "SubscriptionDetected":
            return <WalletCards className="size-5" aria-hidden="true" />;
        case "FinancialInsight":
            return <Sparkles className="size-5" aria-hidden="true" />;
        case "Security":
            return <AlertTriangle className="size-5" aria-hidden="true" />;
    }
}

function getStatusClass(status: Notification["status"]): string {
    switch (status) {
        case "Failed":
        case "DeadLettered":
            return "bg-destructive/10 text-destructive";
        case "Pending":
            return "bg-muted text-muted-foreground";
        case "Delivered":
            return "bg-secondary text-secondary-foreground";
        case "Read":
        case "Dismissed":
            return "bg-muted text-muted-foreground";
    }
}

/** Renders one notification with delivery state and available navigation. */
export function NotificationItem({
    notification,
    onMarkRead,
    onDismiss,
    isMarkingRead,
    isDismissing,
}: NotificationItemProps) {
    const href = notificationHref(notification);
    const unread = isNotificationUnread(notification);
    const canMarkRead = unread && notification.status !== "Pending";
    const canDismiss = unread && notification.status !== "Pending";
    const deliveryIssue =
        notification.status === "Failed" ||
        notification.status === "DeadLettered";

    return (
        <Card
            className={`overflow-hidden transition-shadow hover:shadow-md ${unread ? "border-primary/20" : ""}`}
        >
            <CardContent className="p-0">
                <div className="flex gap-4 p-5">
                    <div
                        className={`mt-0.5 flex size-10 shrink-0 items-center justify-center rounded-xl ${unread ? "bg-primary/10 text-primary" : "bg-muted text-muted-foreground"}`}
                    >
                        <NotificationIcon type={notification.type} />
                    </div>

                    <div className="min-w-0 flex-1 space-y-3">
                        <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between sm:gap-4">
                            <div className="min-w-0">
                                <div className="flex flex-wrap items-center gap-2">
                                    {unread ? (
                                        <span
                                            className="size-2 rounded-full bg-primary"
                                            aria-hidden="true"
                                        />
                                    ) : null}
                                    <h2 className="text-sm font-semibold tracking-tight">
                                        {notification.title}
                                    </h2>
                                </div>
                                <p className="mt-1 text-xs text-muted-foreground">
                                    {formatRelativeDate(notification.createdAt)}
                                </p>
                            </div>

                            <div className="flex shrink-0 flex-wrap gap-2">
                                <Badge variant="outline">
                                    {notificationTypeLabel(notification.type)}
                                </Badge>
                                <Badge
                                    variant="secondary"
                                    className={getStatusClass(
                                        notification.status,
                                    )}
                                >
                                    {notificationStatusLabel(notification)}
                                </Badge>
                            </div>
                        </div>

                        <p className="max-w-3xl text-sm leading-6 text-muted-foreground">
                            {notification.message}
                        </p>

                        {deliveryIssue ? (
                            <div className="flex items-start gap-2 rounded-lg border border-destructive/20 bg-destructive/5 p-3 text-xs text-destructive">
                                <MailWarning
                                    className="mt-0.5 size-4 shrink-0"
                                    aria-hidden="true"
                                />
                                <p>
                                    This notification could not be delivered
                                    successfully. The event remains visible so
                                    you can review the underlying financial
                                    activity.
                                </p>
                            </div>
                        ) : null}

                        <div className="flex flex-col gap-2 border-t pt-3 sm:flex-row sm:items-center sm:justify-between">
                            <div className="flex flex-wrap gap-2">
                                {href ? (
                                    <Button asChild variant="outline" size="sm">
                                        <Link href={href}>
                                            {notification.type ===
                                            "FinancialInsight"
                                                ? "Review insights"
                                                : "Review"}
                                            <ArrowUpRight aria-hidden="true" />
                                        </Link>
                                    </Button>
                                ) : null}
                            </div>

                            <div className="flex flex-wrap items-center gap-2">
                                {canMarkRead ? (
                                    <Button
                                        type="button"
                                        variant="ghost"
                                        size="sm"
                                        onClick={() =>
                                            onMarkRead(notification.id)
                                        }
                                        disabled={isMarkingRead}
                                    >
                                        {isMarkingRead ? (
                                            <Clock3
                                                className="animate-pulse"
                                                aria-hidden="true"
                                            />
                                        ) : (
                                            <CheckCircle2 aria-hidden="true" />
                                        )}
                                        Mark as read
                                    </Button>
                                ) : null}

                                {canDismiss ? (
                                    <Button
                                        type="button"
                                        variant="ghost"
                                        size="sm"
                                        onClick={() =>
                                            onDismiss(notification.id)
                                        }
                                        disabled={isDismissing}
                                    >
                                        {isDismissing ? (
                                            <Clock3
                                                className="animate-pulse"
                                                aria-hidden="true"
                                            />
                                        ) : (
                                            <BellRing aria-hidden="true" />
                                        )}
                                        Dismiss
                                    </Button>
                                ) : null}

                                {!canMarkRead && !canDismiss ? (
                                    <span className="inline-flex items-center gap-1.5 text-xs text-muted-foreground">
                                        {notification.status === "Pending" ? (
                                            <Clock3
                                                className="size-3.5 animate-pulse"
                                                aria-hidden="true"
                                            />
                                        ) : (
                                            <BellRing
                                                className="size-3.5"
                                                aria-hidden="true"
                                            />
                                        )}
                                        {notification.status === "Pending"
                                            ? "Sending"
                                            : notification.status ===
                                                "Dismissed"
                                              ? "Dismissed"
                                              : "Read"}
                                    </span>
                                ) : null}
                            </div>
                        </div>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}
