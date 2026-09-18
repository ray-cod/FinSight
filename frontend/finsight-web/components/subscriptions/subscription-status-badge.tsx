import { Badge } from "@/components/ui/badge";
import { formatSubscriptionStatus, getStatusVariant } from "@/components/subscriptions/subscription-utils";

/** Renders subscription lifecycle status with a consistent visual treatment. */
export function SubscriptionStatusBadge({ status }: { readonly status: string }) {
  return (
    <Badge variant={getStatusVariant(status)} className="font-normal">
      {formatSubscriptionStatus(status)}
    </Badge>
  );
}
