"use client";

import type { NotificationType } from "@/lib/api/types";
import { notificationTypeLabel } from "@/lib/notifications/notification-utils";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

export type NotificationViewFilter = "all" | "unread";

export interface NotificationFilterState {
  readonly view: NotificationViewFilter;
  readonly type: NotificationType | "";
}

interface NotificationFiltersProps {
  readonly filters: NotificationFilterState;
  readonly onChange: (key: keyof NotificationFilterState, value: string) => void;
}

/** Renders notification list filters. */
export function NotificationFilters({ filters, onChange }: NotificationFiltersProps) {
  return (
    <div className="flex flex-col gap-3 rounded-2xl border bg-card p-3 sm:flex-row sm:items-center sm:justify-between">
      <div className="inline-flex w-full rounded-xl border bg-muted/40 p-1 sm:w-auto" aria-label="Notification view">
        {(["all", "unread"] as const).map((view) => (
          <button
            key={view}
            type="button"
            aria-pressed={filters.view === view}
            className={`min-h-8 flex-1 rounded-lg px-3 text-sm font-medium transition-colors sm:flex-none ${
              filters.view === view ? "bg-background shadow-sm" : "text-muted-foreground hover:text-foreground"
            }`}
            onClick={() => onChange("view", view)}
          >
            {view === "all" ? "All notifications" : "Unread"}
          </button>
        ))}
      </div>

      <Select value={filters.type || "all"} onValueChange={(value) => onChange("type", value === "all" ? "" : value ?? "")}>
        <SelectTrigger className="w-full sm:w-56" aria-label="Filter by notification type">
          <SelectValue placeholder="Notification type" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All types</SelectItem>
          {(["AnomalyDetected", "SubscriptionPriceChanged", "SubscriptionDetected", "FinancialInsight", "Security"] as NotificationType[]).map((type) => (
            <SelectItem key={type} value={type}>{notificationTypeLabel(type)}</SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
