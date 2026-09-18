import {
    differenceInCalendarDays,
    formatDistanceToNow,
    isAfter,
    isBefore,
} from "date-fns";

import type {
    Subscription,
    SubscriptionPriceObservation,
} from "@/lib/api/types";
import { formatCurrency } from "@/lib/formatting/currency";

export const subscriptionFrequencies = [
    "Weekly",
    "BiWeekly",
    "Monthly",
    "Quarterly",
    "SemiAnnual",
    "Annual",
    "Unknown",
] as const;

export const subscriptionStatuses = [
    "Active",
    "Inactive",
    "Dismissed",
] as const;

export function formatFrequency(frequency: string): string {
    const labels: Record<string, string> = {
        Weekly: "Weekly",
        BiWeekly: "Every 2 weeks",
        Monthly: "Monthly",
        Quarterly: "Quarterly",
        SemiAnnual: "Twice a year",
        Annual: "Annual",
        Unknown: "Frequency uncertain",
    };

    return (labels[frequency] ?? frequency) || "Unknown";
}

export function getFrequencyDescription(frequency: string): string {
    const descriptions: Record<string, string> = {
        Weekly: "Expected about every 7 days",
        BiWeekly: "Expected about every 14 days",
        Monthly: "Expected about every 30 days",
        Quarterly: "Expected about every 3 months",
        SemiAnnual: "Expected about every 6 months",
        Annual: "Expected about every 12 months",
        Unknown: "Cadence is still being evaluated",
    };

    return (
        descriptions[frequency] ??
        "Recurring cadence detected from transaction history"
    );
}

export function formatSubscriptionStatus(status: string): string {
    const labels: Record<string, string> = {
        Active: "Active",
        Inactive: "Inactive",
        Dismissed: "Dismissed",
    };

    return (labels[status] ?? status) || "Unknown";
}

export function getStatusVariant(
    status: string,
): "default" | "secondary" | "outline" | "destructive" {
    if (status === "Active") return "default";
    if (status === "Dismissed") return "outline";
    return "secondary";
}

export function getNextChargeState(nextExpectedChargeAt: string | null): {
    readonly label: string;
    readonly detail: string;
    readonly urgent: boolean;
} {
    if (!nextExpectedChargeAt) {
        return {
            label: "Not projected",
            detail: "No next charge has been estimated yet",
            urgent: false,
        };
    }

    const date = new Date(nextExpectedChargeAt);
    const now = new Date();
    const days = differenceInCalendarDays(date, now);

    if (days < 0) {
        return {
            label: "Overdue",
            detail: `${formatDistanceToNow(date, { addSuffix: true })}`,
            urgent: true,
        };
    }

    if (days === 0) {
        return {
            label: "Due today",
            detail: "Expected today",
            urgent: true,
        };
    }

    if (days === 1) {
        return {
            label: "Due tomorrow",
            detail: "Expected tomorrow",
            urgent: true,
        };
    }

    return {
        label: `In ${days} days`,
        detail: formatDistanceToNow(date, { addSuffix: true }),
        urgent: days <= 7,
    };
}

export function monthlyEquivalent(
    amount: number,
    frequency: string,
): number | null {
    const multipliers: Record<string, number> = {
        Weekly: 52 / 12,
        BiWeekly: 26 / 12,
        Monthly: 1,
        Quarterly: 1 / 3,
        SemiAnnual: 1 / 6,
        Annual: 1 / 12,
    };

    const multiplier = multipliers[frequency];
    return multiplier ? amount * multiplier : null;
}

export function annualEquivalent(
    amount: number,
    frequency: string,
): number | null {
    const multipliers: Record<string, number> = {
        Weekly: 52,
        BiWeekly: 26,
        Monthly: 12,
        Quarterly: 4,
        SemiAnnual: 2,
        Annual: 1,
    };

    const multiplier = multipliers[frequency];
    return multiplier ? amount * multiplier : null;
}

export interface PriceChangeSummary {
    readonly current: number;
    readonly previous: number | null;
    readonly percentage: number | null;
    readonly direction: "increase" | "decrease" | "unchanged" | "unknown";
    readonly observedAt: string | null;
}

export function getLatestPriceChange(
    history: readonly SubscriptionPriceObservation[],
): PriceChangeSummary {
    const sorted = [...history].sort(
        (a, b) =>
            new Date(b.observedAt).getTime() - new Date(a.observedAt).getTime(),
    );
    const latest = sorted[0];
    const previous = sorted[1];

    if (!latest) {
        return {
            current: 0,
            previous: null,
            percentage: null,
            direction: "unknown",
            observedAt: null,
        };
    }

    if (!previous || previous.amount === 0) {
        return {
            current: latest.amount,
            previous: null,
            percentage: null,
            direction: "unknown",
            observedAt: latest.observedAt,
        };
    }

    const percentage =
        ((latest.amount - previous.amount) / previous.amount) * 100;
    const direction =
        percentage > 0.0001
            ? "increase"
            : percentage < -0.0001
              ? "decrease"
              : "unchanged";

    return {
        current: latest.amount,
        previous: previous.amount,
        percentage,
        direction,
        observedAt: latest.observedAt,
    };
}

export function formatSignedPercentage(value: number): string {
    const sign = value > 0 ? "+" : "";
    return `${sign}${value.toFixed(1)}%`;
}

export function getChangeLabel(
    summary: PriceChangeSummary,
    currency: string,
): string {
    if (summary.percentage === null || summary.previous === null)
        return "No comparable prior price";
    return `${formatCurrency(summary.previous, currency)} → ${formatCurrency(summary.current, currency)}`;
}

export function isRecentPriceChange(
    subscription: Subscription,
    days = 90,
): boolean {
    if (!subscription.lastPriceChangedAt) return false;
    const changedAt = new Date(subscription.lastPriceChangedAt);
    const cutoff = new Date();
    cutoff.setDate(cutoff.getDate() - days);
    return isAfter(changedAt, cutoff);
}

export function isUpcomingWithinDays(
    subscription: Subscription,
    days: number,
): boolean {
    if (!subscription.nextExpectedChargeAt || subscription.status !== "Active")
        return false;
    const now = new Date();
    const limit = new Date();
    limit.setDate(limit.getDate() + days);
    const date = new Date(subscription.nextExpectedChargeAt);
    return !isBefore(date, now) && !isAfter(date, limit);
}

export function groupedMonthlyCommitments(
    subscriptions: readonly Subscription[],
): ReadonlyMap<string, number> {
    const result = new Map<string, number>();

    for (const subscription of subscriptions) {
        if (subscription.status !== "Active") continue;
        const amount = monthlyEquivalent(
            subscription.currentAmount,
            subscription.frequency,
        );
        if (amount === null) continue;
        result.set(
            subscription.currency,
            (result.get(subscription.currency) ?? 0) + amount,
        );
    }

    return result;
}
