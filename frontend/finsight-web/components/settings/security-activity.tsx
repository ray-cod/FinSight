"use client";

import {
    CheckCircle2,
    KeyRound,
    LockKeyhole,
    LogIn,
    LogOut,
    ShieldCheck,
    UserRound,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { formatRelativeDate } from "@/lib/formatting/date";
import { useSecurityActivity } from "@/lib/security/use-security";

function getActivityPresentation(eventType: string) {
    switch (eventType) {
        case "LoginSucceeded":
            return {
                label: "Signed in",
                description: "A successful sign-in was recorded.",
                icon: LogIn,
                variant: "secondary" as const,
            };
        case "LoginFailed":
            return {
                label: "Failed sign-in",
                description: "An unsuccessful sign-in attempt was recorded.",
                icon: LogIn,
                variant: "destructive" as const,
            };
        case "PasswordChanged":
            return {
                label: "Password changed",
                description: "Your account password was changed.",
                icon: KeyRound,
                variant: "secondary" as const,
            };
        case "PasswordResetCompleted":
            return {
                label: "Password reset",
                description: "A password reset was completed.",
                icon: KeyRound,
                variant: "secondary" as const,
            };
        case "PasswordResetRequested":
            return {
                label: "Password reset requested",
                description: "A password reset flow was requested.",
                icon: KeyRound,
                variant: "outline" as const,
            };
        case "RefreshTokenRevoked":
            return {
                label: "Session signed out",
                description: "An authentication session was revoked.",
                icon: LogOut,
                variant: "outline" as const,
            };
        case "OtherSessionsRevoked":
            return {
                label: "Other sessions revoked",
                description:
                    "All active sessions except the current one were signed out.",
                icon: ShieldCheck,
                variant: "secondary" as const,
            };
        case "UserRegistered":
            return {
                label: "Account created",
                description: "Your FinSight account was created.",
                icon: UserRound,
                variant: "secondary" as const,
            };
        case "ProfileUpdated":
            return {
                label: "Profile updated",
                description: "Your profile information was updated.",
                icon: CheckCircle2,
                variant: "secondary" as const,
            };
        default:
            return {
                label: eventType,
                description: "A security event was recorded.",
                icon: LockKeyhole,
                variant: "outline" as const,
            };
    }
}

/** Renders recent user-scoped security activity without exposing raw audit metadata. */
export function SecurityActivityPanel() {
    const activity = useSecurityActivity(25);

    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-center gap-2">
                    <ShieldCheck className="size-5" aria-hidden="true" /> Recent
                    security activity
                </CardTitle>
                <CardDescription>
                    Review recent authentication and account-security events.
                    Sensitive audit metadata is never shown here.
                </CardDescription>
            </CardHeader>
            <CardContent>
                {activity.isLoading ? (
                    <div className="space-y-3">
                        {Array.from({ length: 5 }, (_, index) => (
                            <Skeleton key={index} className="h-16 w-full" />
                        ))}
                    </div>
                ) : activity.isError ? (
                    <p className="rounded-xl border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
                        Security activity could not be loaded. Refresh the page
                        and try again.
                    </p>
                ) : (activity.data?.length ?? 0) === 0 ? (
                    <div className="rounded-xl border border-dashed p-6 text-center text-sm text-muted-foreground">
                        No security activity has been recorded yet.
                    </div>
                ) : (
                    <div className="divide-y">
                        {(activity.data ?? []).map((item, index) => {
                            const presentation = getActivityPresentation(
                                item.eventType,
                            );
                            const Icon = presentation.icon;
                            return (
                                <div
                                    key={`${item.occurredAt}-${item.eventType}-${index}`}
                                    className="flex gap-3 py-4 first:pt-0 last:pb-0"
                                >
                                    <div className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-muted">
                                        <Icon
                                            className="size-4 text-muted-foreground"
                                            aria-hidden="true"
                                        />
                                    </div>
                                    <div className="min-w-0 flex-1">
                                        <div className="flex flex-wrap items-center gap-2">
                                            <p className="text-sm font-medium">
                                                {presentation.label}
                                            </p>
                                            <Badge
                                                variant={presentation.variant}
                                            >
                                                {formatRelativeDate(
                                                    item.occurredAt,
                                                )}
                                            </Badge>
                                        </div>
                                        <p className="mt-1 text-xs leading-5 text-muted-foreground">
                                            {presentation.description}
                                        </p>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                )}
            </CardContent>
        </Card>
    );
}
