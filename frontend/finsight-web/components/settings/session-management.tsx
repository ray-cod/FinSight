"use client";

import * as React from "react";
import {
    CheckCircle2,
    Clock3,
    Globe2,
    Monitor,
    ShieldCheck,
    UserRound,
    X,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
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
    DialogTrigger,
} from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { formatDate, formatRelativeDate } from "@/lib/formatting/date";
import {
    useRevokeOtherSessions,
    useSecuritySessions,
} from "@/lib/security/use-security";

function formatIp(ipAddress: string | null): string {
    return ipAddress ?? "Unavailable";
}

/** Renders active authentication sessions and lets the user revoke every other session. */
export function SessionManagementPanel() {
    const sessions = useSecuritySessions();
    const revoke = useRevokeOtherSessions();
    const [dialogOpen, setDialogOpen] = React.useState(false);

    const otherCount =
        sessions.data?.filter((session) => !session.isCurrent).length ?? 0;

    const confirmRevoke = async () => {
        try {
            await revoke.mutateAsync();
            setDialogOpen(false);
        } catch {
            // Mutation state renders the actionable error below.
        }
    };

    return (
        <Card>
            <CardHeader>
                <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                    <div className="space-y-1.5">
                        <CardTitle className="flex items-center gap-2">
                            <Monitor className="size-5" aria-hidden="true" />{" "}
                            Active sessions
                        </CardTitle>
                        <CardDescription>
                            These are the refresh-token sessions currently able
                            to establish an authenticated FinSight session.
                        </CardDescription>
                    </div>
                    <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
                        <DialogTrigger
                            render={
                                <Button
                                    variant="outline"
                                    disabled={
                                        otherCount === 0 || revoke.isPending
                                    }
                                />
                            }
                        >
                            <X aria-hidden="true" />
                            {revoke.isPending
                                ? "Signing out…"
                                : "Sign out other sessions"}
                        </DialogTrigger>
                        <DialogContent>
                            <DialogHeader>
                                <DialogTitle>
                                    Sign out other sessions?
                                </DialogTitle>
                                <DialogDescription>
                                    This will revoke {otherCount} other active{" "}
                                    {otherCount === 1 ? "session" : "sessions"}.
                                    Your current session will remain active.
                                </DialogDescription>
                            </DialogHeader>
                            <DialogFooter showCloseButton>
                                <Button
                                    variant="destructive"
                                    onClick={() => void confirmRevoke()}
                                    disabled={revoke.isPending}
                                >
                                    {revoke.isPending
                                        ? "Revoking…"
                                        : "Sign out other sessions"}
                                </Button>
                            </DialogFooter>
                        </DialogContent>
                    </Dialog>
                </div>
            </CardHeader>
            <CardContent className="space-y-4">
                {revoke.isError ? (
                    <p className="rounded-xl border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
                        The other sessions could not be revoked. Try again.
                    </p>
                ) : null}

                {sessions.isLoading ? (
                    <div className="space-y-3">
                        {Array.from({ length: 2 }, (_, index) => (
                            <Skeleton key={index} className="h-28 w-full" />
                        ))}
                    </div>
                ) : sessions.isError ? (
                    <div className="rounded-xl border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
                        Active sessions could not be loaded. Refresh the page
                        and try again.
                    </div>
                ) : (sessions.data?.length ?? 0) === 0 ? (
                    <div className="rounded-xl border border-dashed p-6 text-center text-sm text-muted-foreground">
                        No active sessions are currently available.
                    </div>
                ) : (
                    <div className="space-y-3">
                        {(sessions.data ?? []).map((session) => (
                            <div
                                key={session.sessionId}
                                className="rounded-xl border p-4"
                            >
                                <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                                    <div className="flex min-w-0 gap-3">
                                        <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-muted">
                                            {session.isCurrent ? (
                                                <ShieldCheck
                                                    className="size-5 text-foreground"
                                                    aria-hidden="true"
                                                />
                                            ) : (
                                                <UserRound
                                                    className="size-5 text-muted-foreground"
                                                    aria-hidden="true"
                                                />
                                            )}
                                        </div>
                                        <div className="min-w-0">
                                            <div className="flex flex-wrap items-center gap-2">
                                                <p className="text-sm font-semibold">
                                                    {session.isCurrent
                                                        ? "Current session"
                                                        : "Active session"}
                                                </p>
                                                {session.isCurrent ? (
                                                    <Badge variant="secondary">
                                                        <CheckCircle2 aria-hidden="true" />
                                                        Current
                                                    </Badge>
                                                ) : null}
                                            </div>
                                            <div className="mt-2 grid gap-2 text-xs text-muted-foreground sm:grid-cols-2">
                                                <span className="inline-flex items-center gap-1.5">
                                                    <Clock3
                                                        className="size-3.5"
                                                        aria-hidden="true"
                                                    />
                                                    Started{" "}
                                                    {formatRelativeDate(
                                                        session.createdAt,
                                                    )}
                                                </span>
                                                <span className="inline-flex items-center gap-1.5">
                                                    <Globe2
                                                        className="size-3.5"
                                                        aria-hidden="true"
                                                    />
                                                    IP{" "}
                                                    {formatIp(
                                                        session.ipAddress,
                                                    )}
                                                </span>
                                                <span>
                                                    Created{" "}
                                                    {formatDate(
                                                        session.createdAt,
                                                    )}
                                                </span>
                                                <span>
                                                    Expires{" "}
                                                    {formatDate(
                                                        session.expiresAt,
                                                    )}
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}

                <p className="text-xs leading-5 text-muted-foreground">
                    Signing out other sessions does not affect your current
                    browser session. Password changes revoke all refresh-token
                    sessions and require you to sign in again.
                </p>
            </CardContent>
        </Card>
    );
}
