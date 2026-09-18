"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { Brand } from "@/components/layout/brand";
import {
  primaryNavigation,
  secondaryNavigation,
} from "@/components/navigation/nav-config";
import { cn } from "@/lib/utils";

function NavLinks() {
  const pathname = usePathname();

  return (
    <>
      <div className="space-y-1">
        <p className="px-3 pb-2 text-[11px] font-semibold uppercase tracking-[0.16em] text-muted-foreground">
          Workspace
        </p>
        {primaryNavigation.map((item) => {
          const Icon = item.icon;
          const active = pathname === item.href || pathname.startsWith(`${item.href}/`);

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "group flex min-h-10 items-center gap-3 rounded-lg px-3 text-sm font-medium transition-colors",
                active
                  ? "bg-primary text-primary-foreground shadow-sm"
                  : "text-muted-foreground hover:bg-accent hover:text-foreground",
              )}
              aria-current={active ? "page" : undefined}
              title={item.description}
            >
              <Icon className="size-4 shrink-0" aria-hidden="true" />
              <span className="truncate">{item.label}</span>
            </Link>
          );
        })}
      </div>

      <div className="mt-auto space-y-1 border-t border-border/70 pt-4">
        {secondaryNavigation.map((item) => {
          const Icon = item.icon;
          const active = pathname === item.href || pathname.startsWith(`${item.href}/`);

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex min-h-10 items-center gap-3 rounded-lg px-3 text-sm font-medium transition-colors",
                active
                  ? "bg-accent text-foreground"
                  : "text-muted-foreground hover:bg-accent hover:text-foreground",
              )}
              aria-current={active ? "page" : undefined}
            >
              <Icon className="size-4 shrink-0" aria-hidden="true" />
              <span>{item.label}</span>
            </Link>
          );
        })}
      </div>
    </>
  );
}

/**
 * Renders the persistent desktop navigation for authenticated FinSight pages.
 */
export function Sidebar() {
  return (
    <aside className="hidden w-64 shrink-0 border-r border-border/70 bg-sidebar md:flex md:flex-col">
      <div className="flex h-16 items-center border-b border-sidebar-border/70 px-5">
        <Brand />
      </div>
      <nav className="flex flex-1 flex-col p-3" aria-label="Primary navigation">
        <NavLinks />
      </nav>
      <div className="border-t border-sidebar-border/70 px-5 py-4">
        <p className="text-xs leading-5 text-muted-foreground">
          Smart subscription and financial anomaly detection.
        </p>
      </div>
    </aside>
  );
}
