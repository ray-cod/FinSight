"use client";

import * as React from "react";
import { CheckCircle2, WifiOff } from "lucide-react";

import { useOnlineStatus } from "@/lib/hooks/use-online-status";

/** Provides lightweight offline/online feedback without blocking application interaction. */
export function NetworkStatusBanner() {
    const online = useOnlineStatus();
    const wasOffline = React.useRef(false);
    const [showRestored, setShowRestored] = React.useState(false);
    const restoreTimerRef = React.useRef<number | null>(null);

    React.useEffect(() => {
        const handleOffline = () => {
            wasOffline.current = true;
            setShowRestored(false);
            if (restoreTimerRef.current !== null) {
                window.clearTimeout(restoreTimerRef.current);
                restoreTimerRef.current = null;
            }
        };

        const handleOnline = () => {
            if (!wasOffline.current) return;
            wasOffline.current = false;
            setShowRestored(true);
            if (restoreTimerRef.current !== null)
                window.clearTimeout(restoreTimerRef.current);
            restoreTimerRef.current = window.setTimeout(
                () => setShowRestored(false),
                3500,
            );
        };

        window.addEventListener("offline", handleOffline);
        window.addEventListener("online", handleOnline);

        return () => {
            window.removeEventListener("offline", handleOffline);
            window.removeEventListener("online", handleOnline);
            if (restoreTimerRef.current !== null) {
                window.clearTimeout(restoreTimerRef.current);
            }
        };
    }, []);

    if (!online) {
        return (
            <div
                role="status"
                aria-live="polite"
                className="border-b border-amber-500/20 bg-amber-500/10 px-4 py-2 text-xs text-amber-950 dark:text-amber-100 sm:px-6 lg:px-8"
            >
                <div className="mx-auto flex max-w-[1600px] items-center gap-2">
                    <WifiOff className="size-3.5 shrink-0" aria-hidden="true" />
                    <span>
                        You are offline. Existing data remains available, but
                        actions may fail until your connection returns.
                    </span>
                </div>
            </div>
        );
    }

    if (!showRestored) return null;

    return (
        <div
            role="status"
            aria-live="polite"
            className="border-b border-emerald-500/20 bg-emerald-500/10 px-4 py-2 text-xs text-emerald-950 dark:text-emerald-100 sm:px-6 lg:px-8"
        >
            <div className="mx-auto flex max-w-[1600px] items-center gap-2">
                <CheckCircle2
                    className="size-3.5 shrink-0"
                    aria-hidden="true"
                />
                <span>
                    Your connection is back. You can continue using FinSight
                    normally.
                </span>
            </div>
        </div>
    );
}
