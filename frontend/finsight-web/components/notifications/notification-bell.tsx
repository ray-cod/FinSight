"use client";

import Link from "next/link";
import { Bell } from "lucide-react";

import { useUnreadNotificationCount } from "@/lib/notifications/use-notifications";
import { Button } from "@/components/ui/button";

/** Renders the global notification affordance with a live unread count. */
export function NotificationBell() {
  const unread = useUnreadNotificationCount();
  const count = unread.data?.count ?? 0;
  const label = count > 0 ? `Notifications, ${count} unread` : "Notifications";

  return (
    <Button asChild variant="ghost" size="icon" className="relative" aria-label={label}>
      <Link href="/notifications">
        <Bell className="size-4" aria-hidden="true" />
        <span className="sr-only">Notifications</span>
        {count > 0 ? (
          <span
            aria-hidden="true"
            className="absolute -right-0.5 -top-0.5 flex min-w-4 items-center justify-center rounded-full border-2 border-background bg-primary px-1 text-[10px] font-semibold leading-3 text-primary-foreground"
          >
            {count > 99 ? "99+" : count}
          </span>
        ) : null}
      </Link>
    </Button>
  );
}
