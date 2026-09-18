"use client";

import * as React from "react";
import {
    Bell,
    CheckCircle2,
    Lightbulb,
    Mail,
    Save,
    ShieldAlert,
    Sparkles,
    WalletCards,
} from "lucide-react";

import type {
    NotificationPreferences,
    UpdateNotificationPreferencesRequest,
} from "@/lib/api/types";
import {
    useNotificationPreferences,
    useUpdateNotificationPreferences,
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
import { useAppToast } from "@/components/feedback/use-app-toast";

interface ToggleSettingProps {
    readonly checked: boolean;
    readonly disabled?: boolean;
    readonly onChange: (checked: boolean) => void;
    readonly title: string;
    readonly description: string;
    readonly icon: React.ComponentType<{
        className?: string;
        "aria-hidden"?: boolean;
    }>;
}

function ToggleSetting({
    checked,
    disabled,
    onChange,
    title,
    description,
    icon: Icon,
}: ToggleSettingProps) {
    return (
        <div className="flex items-start justify-between gap-4 rounded-xl border p-4">
            <div className="flex min-w-0 gap-3">
                <div className="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-lg bg-muted">
                    <Icon
                        className="size-4 text-muted-foreground"
                        aria-hidden={true}
                    />
                </div>
                <div className="min-w-0">
                    <p className="text-sm font-medium">{title}</p>
                    <p className="mt-1 text-xs leading-5 text-muted-foreground">
                        {description}
                    </p>
                </div>
            </div>
            <button
                type="button"
                role="switch"
                aria-checked={checked}
                aria-label={`${title}: ${checked ? "enabled" : "disabled"}`}
                disabled={disabled}
                onClick={() => onChange(!checked)}
                className={`relative mt-0.5 inline-flex h-6 w-11 shrink-0 rounded-full border p-0.5 transition-colors focus-visible:outline-none focus-visible:ring-3 focus-visible:ring-ring/50 ${checked ? "bg-primary" : "bg-muted"} disabled:cursor-not-allowed disabled:opacity-50`}
            >
                <span
                    className={`size-5 rounded-full bg-background shadow-sm transition-transform ${checked ? "translate-x-5" : "translate-x-0"}`}
                />
            </button>
        </div>
    );
}

const DEFAULTS: NotificationPreferences = {
    inAppEnabled: true,
    emailEnabled: false,
    anomalyNotificationsEnabled: true,
    subscriptionNotificationsEnabled: true,
    insightNotificationsEnabled: true,
};

/** Renders the user's notification channel and category preferences. */
export function NotificationPreferencesPanel() {
    const preferences = useNotificationPreferences();
    const update = useUpdateNotificationPreferences();
    const [localDraft, setLocalDraft] =
        React.useState<UpdateNotificationPreferencesRequest>(DEFAULTS);
    const [hasLocalEdits, setHasLocalEdits] = React.useState(false);
    const toast = useAppToast();

    const draft = hasLocalEdits ? localDraft : (preferences.data ?? DEFAULTS);
    const dirty = preferences.data
        ? JSON.stringify(preferences.data) !== JSON.stringify(draft)
        : false;

    const updateDraft = <K extends keyof UpdateNotificationPreferencesRequest>(
        key: K,
        value: UpdateNotificationPreferencesRequest[K],
    ) => {
        const next = {
            ...(hasLocalEdits ? localDraft : (preferences.data ?? DEFAULTS)),
            [key]: value,
        };
        setLocalDraft(next);
        setHasLocalEdits(true);
    };

    React.useEffect(() => {
        if (!dirty) return;
        const handleBeforeUnload = (event: BeforeUnloadEvent) => {
            event.preventDefault();
            event.returnValue = "";
        };
        window.addEventListener("beforeunload", handleBeforeUnload);
        return () =>
            window.removeEventListener("beforeunload", handleBeforeUnload);
    }, [dirty]);

    const save = () =>
        update.mutate(draft, {
            onSuccess: () => {
                setHasLocalEdits(false);
                setLocalDraft(preferences.data ?? DEFAULTS);
                toast.success({
                    title: "Preferences saved",
                    description: "Your notification choices are up to date.",
                });
            },
            onError: (error) =>
                toast.error({
                    title: "Preferences could not be saved",
                    description:
                        error instanceof Error
                            ? error.message
                            : "Please try again.",
                }),
        });

    return (
        <Card>
            <CardHeader>
                <div className="flex items-start justify-between gap-4">
                    <div className="space-y-1.5">
                        <CardTitle className="flex items-center gap-2">
                            <Bell className="size-5" aria-hidden="true" />{" "}
                            Notification preferences
                        </CardTitle>
                        <CardDescription>
                            Choose which FinSight alerts you receive in the app
                            and by email. Security notifications remain enabled
                            in-app so account-protection events cannot be
                            silently disabled; email delivery can still be
                            controlled separately.
                        </CardDescription>
                    </div>
                    {dirty ? (
                        <span className="rounded-full border bg-muted px-2.5 py-1 text-xs font-medium text-muted-foreground">
                            Unsaved changes
                        </span>
                    ) : null}
                </div>
            </CardHeader>

            <CardContent className="space-y-6">
                {preferences.isError ? (
                    <div className="rounded-xl border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
                        Notification preferences could not be loaded. Refresh
                        the page and try again.
                    </div>
                ) : null}

                {preferences.isLoading ? (
                    <div className="space-y-3">
                        <Skeleton className="h-24 w-full" />
                        <Skeleton className="h-24 w-full" />
                        <Skeleton className="h-24 w-full" />
                        <Skeleton className="h-24 w-full" />
                    </div>
                ) : (
                    <>
                        <section
                            className="space-y-3"
                            aria-labelledby="notification-channels-heading"
                        >
                            <div>
                                <h3
                                    id="notification-channels-heading"
                                    className="text-sm font-semibold"
                                >
                                    Delivery channels
                                </h3>
                                <p className="mt-1 text-xs text-muted-foreground">
                                    Control where eligible FinSight
                                    notifications are delivered.
                                </p>
                            </div>
                            <div className="grid gap-3 lg:grid-cols-2">
                                <ToggleSetting
                                    icon={Bell}
                                    checked={draft.inAppEnabled}
                                    onChange={(value) =>
                                        updateDraft("inAppEnabled", value)
                                    }
                                    title="In-app notifications"
                                    description="Show actionable financial alerts in your FinSight notification center."
                                />
                                <ToggleSetting
                                    icon={Mail}
                                    checked={draft.emailEnabled}
                                    onChange={(value) =>
                                        updateDraft("emailEnabled", value)
                                    }
                                    title="Email notifications"
                                    description="Send eligible financial alerts to your verified account email address."
                                />
                            </div>
                        </section>

                        <section
                            className="space-y-3"
                            aria-labelledby="notification-categories-heading"
                        >
                            <div>
                                <h3
                                    id="notification-categories-heading"
                                    className="text-sm font-semibold"
                                >
                                    Alert categories
                                </h3>
                                <p className="mt-1 text-xs text-muted-foreground">
                                    Choose which financial intelligence
                                    categories may create notifications.
                                </p>
                            </div>
                            <div className="grid gap-3 lg:grid-cols-3">
                                <ToggleSetting
                                    icon={ShieldAlert}
                                    checked={draft.anomalyNotificationsEnabled}
                                    onChange={(value) =>
                                        updateDraft(
                                            "anomalyNotificationsEnabled",
                                            value,
                                        )
                                    }
                                    title="Anomalies"
                                    description="Unusual or unexpected financial activity detected in your accounts."
                                />
                                <ToggleSetting
                                    icon={WalletCards}
                                    checked={
                                        draft.subscriptionNotificationsEnabled
                                    }
                                    onChange={(value) =>
                                        updateDraft(
                                            "subscriptionNotificationsEnabled",
                                            value,
                                        )
                                    }
                                    title="Subscriptions"
                                    description="New recurring payments and subscription price changes."
                                />
                                <ToggleSetting
                                    icon={Sparkles}
                                    checked={draft.insightNotificationsEnabled}
                                    onChange={(value) =>
                                        updateDraft(
                                            "insightNotificationsEnabled",
                                            value,
                                        )
                                    }
                                    title="Financial insights"
                                    description="Explanations generated from detected financial patterns."
                                />
                            </div>
                        </section>

                        <div className="rounded-xl border border-dashed bg-muted/20 p-4 text-sm text-muted-foreground">
                            <div className="flex gap-3">
                                <Lightbulb
                                    className="mt-0.5 size-4 shrink-0"
                                    aria-hidden="true"
                                />
                                <p>
                                    Turning off in-app notifications affects
                                    future notification creation; existing
                                    notifications remain available until you
                                    mark them as read.
                                </p>
                            </div>
                        </div>

                        <div className="flex flex-col gap-3 border-t pt-5 sm:flex-row sm:items-center sm:justify-between">
                            <div className="text-xs text-muted-foreground">
                                {update.isSuccess && !dirty ? (
                                    <span className="inline-flex items-center gap-1.5 text-foreground">
                                        <CheckCircle2
                                            className="size-3.5"
                                            aria-hidden="true"
                                        />{" "}
                                        Preferences saved.
                                    </span>
                                ) : (
                                    "Changes are saved together so channel and category settings stay consistent."
                                )}
                            </div>
                            <Button
                                type="button"
                                onClick={save}
                                disabled={
                                    !dirty ||
                                    update.isPending ||
                                    preferences.isError
                                }
                            >
                                <Save aria-hidden="true" />{" "}
                                {update.isPending
                                    ? "Saving…"
                                    : "Save preferences"}
                            </Button>
                        </div>
                        {update.isError ? (
                            <p className="text-sm text-destructive">
                                Your preference changes could not be saved.
                                Please try again.
                            </p>
                        ) : null}
                    </>
                )}
            </CardContent>
        </Card>
    );
}
