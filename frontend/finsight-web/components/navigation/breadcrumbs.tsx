"use client";

import Link from "next/link";
import { ChevronRight, Home } from "lucide-react";
import { usePathname, useSearchParams } from "next/navigation";

import { primaryNavigation, secondaryNavigation } from "@/components/navigation/nav-config";
import { cn } from "@/lib/utils";

const navigation = [...primaryNavigation, ...secondaryNavigation];

function resolveLabels(pathname: string, search: string): readonly { label: string; href?: string }[] {
  const base = navigation.find((item) => pathname === item.href || pathname.startsWith(`${item.href}/`));
  if (!base || pathname === base.href) return [];

  if (base.href === "/settings") {
    const section = new URLSearchParams(search).get("section");
    if (section) {
      return [
        { label: base.label, href: base.href },
        { label: section.charAt(0).toUpperCase() + section.slice(1) },
      ];
    }
  }

  return [{ label: base.label, href: base.href }, { label: "Details" }];
}

/** Renders a compact accessible breadcrumb trail for nested workspace routes. */
export function Breadcrumbs() {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const items = resolveLabels(pathname, searchParams.toString());
  if (items.length === 0) return null;

  return (
    <nav aria-label="Breadcrumb" className="mt-1 flex min-w-0 items-center gap-1 text-[11px] text-muted-foreground">
      <Link href="/dashboard" className="inline-flex shrink-0 items-center rounded-sm hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/50">
        <Home className="size-3" aria-hidden="true" />
        <span className="sr-only">Overview</span>
      </Link>
      {items.map((item, index) => (
        <span key={`${item.label}-${index}`} className="inline-flex min-w-0 items-center gap-1">
          <ChevronRight className="size-3 shrink-0" aria-hidden="true" />
          {item.href ? (
            <Link href={item.href} className="truncate hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/50">
              {item.label}
            </Link>
          ) : (
            <span className={cn("truncate", index === items.length - 1 && "text-foreground/75")}>{item.label}</span>
          )}
        </span>
      ))}
    </nav>
  );
}
