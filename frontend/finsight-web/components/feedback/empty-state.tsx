import Link from "next/link";
import type { LucideIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

interface EmptyStateProps {
  /** Icon that communicates the empty resource type. */
  readonly icon: LucideIcon;
  /** Heading shown to the user. */
  readonly title: string;
  /** Supporting explanation for why the resource is empty. */
  readonly description: string;
  /** Optional recovery or next-step action. */
  readonly action?: {
    readonly label: string;
    readonly onClick?: () => void;
    readonly href?: string;
  };
}

/** Renders the shared empty-state treatment used across resource workspaces. */
export function EmptyState({ icon: Icon, title, description, action }: EmptyStateProps) {
  return (
    <Card className="border-dashed bg-muted/[0.08]">
      <CardContent className="flex min-h-64 flex-col items-center justify-center px-6 py-12 text-center">
        <div className="flex size-12 items-center justify-center rounded-xl bg-muted text-muted-foreground">
          <Icon className="size-5" aria-hidden="true" />
        </div>
        <h2 className="mt-4 text-base font-semibold tracking-tight">{title}</h2>
        <p className="mt-1 max-w-lg text-sm leading-6 text-muted-foreground">{description}</p>
        {action?.href ? (
          <Button asChild variant="outline" className="mt-5">
            <Link href={action.href}>{action.label}</Link>
          </Button>
        ) : action?.onClick ? (
          <Button type="button" variant="outline" className="mt-5" onClick={action.onClick}>
            {action.label}
          </Button>
        ) : null}
      </CardContent>
    </Card>
  );
}
