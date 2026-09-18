import Link from "next/link";
import { ArrowUpRight } from "lucide-react";

import { cn } from "@/lib/utils";

interface BrandProps {
  readonly compact?: boolean;
  readonly className?: string;
}

/**
 * Renders the FinSight product mark and wordmark.
 */
export function Brand({ compact = false, className }: BrandProps) {
  return (
    <Link
      href="/dashboard"
      className={cn("group inline-flex items-center gap-3", className)}
      aria-label="FinSight dashboard"
    >
      <span className="relative flex size-9 shrink-0 items-center justify-center overflow-hidden rounded-xl bg-primary text-primary-foreground shadow-sm">
        <span className="absolute -right-2 -top-3 size-8 rounded-full bg-primary-foreground/10" />
        <span className="absolute bottom-0 left-1/2 h-1/2 w-px -translate-x-1/2 bg-primary-foreground/25" />
        <span className="relative text-sm font-semibold tracking-tight">F</span>
      </span>
      {!compact ? (
        <span className="flex min-w-0 items-center gap-1">
          <span className="truncate text-sm font-semibold tracking-tight">FinSight</span>
          <ArrowUpRight className="size-3.5 opacity-40 transition-transform group-hover:-translate-y-0.5 group-hover:translate-x-0.5" aria-hidden="true" />
        </span>
      ) : null}
    </Link>
  );
}
