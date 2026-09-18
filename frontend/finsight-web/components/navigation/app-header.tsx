"use client";

import { usePathname } from "next/navigation";

import { NotificationBell } from "@/components/notifications/notification-bell";
import { Breadcrumbs } from "@/components/navigation/breadcrumbs";
import { MobileNav } from "@/components/navigation/mobile-nav";
import { UserMenu } from "@/components/navigation/user-menu";
import { primaryNavigation, secondaryNavigation } from "@/components/navigation/nav-config";
import { Separator } from "@/components/ui/separator";

interface AppHeaderProps {
  readonly title?: string;
  readonly description?: string;
}

/** Renders the shared authenticated application header. */
export function AppHeader({ title, description }: AppHeaderProps) {
  const pathname = usePathname();
  const navigationItem = [...primaryNavigation, ...secondaryNavigation].find(
    (item) => pathname === item.href || pathname.startsWith(`${item.href}/`),
  );
  const resolvedTitle = title ?? navigationItem?.label ?? "FinSight";
  const resolvedDescription = description ?? navigationItem?.description;

  return (
    <header className="sticky top-0 z-20 border-b border-border/70 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/80">
      <div className="flex min-h-16 items-center justify-between gap-4 px-4 sm:px-6 lg:px-8">
        <div className="flex min-w-0 items-center gap-3">
          <MobileNav />
          <Separator orientation="vertical" className="hidden h-6 md:block" />
          <div className="min-w-0">
            <h1 className="truncate text-sm font-semibold tracking-tight sm:text-base">{resolvedTitle}</h1>
            {resolvedDescription ? <p className="hidden truncate text-xs text-muted-foreground sm:block">{resolvedDescription}</p> : null}
            <Breadcrumbs />
          </div>
        </div>

        <div className="flex items-center gap-1">
          <NotificationBell />
          <UserMenu />
        </div>
      </div>
    </header>
  );
}
