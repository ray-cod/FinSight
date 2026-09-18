import type { ReactNode } from "react";

import { Brand } from "@/components/layout/brand";

interface AuthPageProps {
  readonly eyebrow: string;
  readonly title: string;
  readonly description: string;
  readonly children: ReactNode;
  readonly footer?: ReactNode;
}

/** Provides the shared, responsive presentation shell for public authentication pages. */
export function AuthPage({ eyebrow, title, description, children, footer }: AuthPageProps) {
  return (
    <main className="min-h-screen bg-background">
      <div className="mx-auto grid min-h-screen max-w-7xl lg:grid-cols-[0.9fr_1.1fr]">
        <section className="hidden border-r border-border/70 bg-sidebar px-10 py-10 lg:flex lg:flex-col lg:justify-between">
          <Brand />
          <div className="max-w-md space-y-6">
            <div className="space-y-3">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-muted-foreground">
                Financial intelligence
              </p>
              <h2 className="text-4xl font-semibold tracking-tight text-balance">
                See where your money is going before it surprises you.
              </h2>
              <p className="text-base leading-7 text-muted-foreground">
                FinSight brings accounts, transactions, recurring payments and unusual spending together in one clear financial workspace.
              </p>
            </div>
            <div className="grid gap-3 text-sm text-muted-foreground">
              <div className="rounded-xl border bg-background/70 px-4 py-3">AI-assisted transaction categorization</div>
              <div className="rounded-xl border bg-background/70 px-4 py-3">Subscription and price-change detection</div>
              <div className="rounded-xl border bg-background/70 px-4 py-3">Financial anomaly and insight alerts</div>
            </div>
          </div>
          <p className="text-xs text-muted-foreground">FinSight · Smart subscription and financial anomaly engine</p>
        </section>

        <section className="flex min-h-screen items-center justify-center px-4 py-10 sm:px-6">
          <div className="w-full max-w-md">
            <div className="mb-8 lg:hidden">
              <Brand />
            </div>
            <div className="space-y-2">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-muted-foreground">{eyebrow}</p>
              <h1 className="text-3xl font-semibold tracking-tight">{title}</h1>
              <p className="text-sm leading-6 text-muted-foreground">{description}</p>
            </div>
            <div className="mt-8">{children}</div>
            {footer ? <div className="mt-6 text-sm text-muted-foreground">{footer}</div> : null}
          </div>
        </section>
      </div>
    </main>
  );
}
