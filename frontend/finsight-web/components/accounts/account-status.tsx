import { AlertCircle, CheckCircle2, Clock3, Link2Off, RefreshCw } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import type { AccountConnection } from "@/lib/api/types";

const labels: Record<string, string> = {
  Connected: "Connected",
  Syncing: "Syncing",
  Failed: "Sync failed",
  ReauthorizationRequired: "Action required",
  Disconnected: "Disconnected",
};

function getStatusMeta(status: string) {
  switch (status) {
    case "Syncing":
      return { icon: RefreshCw, variant: "outline" as const, className: "text-foreground", spin: true };
    case "Failed":
      return { icon: AlertCircle, variant: "destructive" as const, className: "", spin: false };
    case "ReauthorizationRequired":
      return { icon: Link2Off, variant: "destructive" as const, className: "", spin: false };
    case "Connected":
      return { icon: CheckCircle2, variant: "outline" as const, className: "", spin: false };
    default:
      return { icon: Clock3, variant: "outline" as const, className: "", spin: false };
  }
}

/** Renders a connection state badge with an accessible textual status. */
export function AccountConnectionStatus({ status }: { readonly status: AccountConnection["status"] }) {
  const meta = getStatusMeta(status);
  const Icon = meta.icon;

  return (
    <Badge variant={meta.variant} className="gap-1.5">
      <Icon className={meta.spin ? "size-3 animate-spin" : "size-3"} aria-hidden="true" />
      <span>{labels[status] ?? status}</span>
    </Badge>
  );
}
