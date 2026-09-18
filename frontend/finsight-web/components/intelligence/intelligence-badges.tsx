import { Badge } from "@/components/ui/badge";
import { lifecycleClasses, severityClasses } from "@/lib/intelligence/utils";

/** Renders a consistent severity badge for anomaly and insight surfaces. */
export function IntelligenceSeverityBadge({ severity }: { readonly severity: string }) {
  return (
    <Badge variant="outline" className={severityClasses(severity)}>
      {severity}
    </Badge>
  );
}

/** Renders a consistent lifecycle badge for anomaly and insight surfaces. */
export function IntelligenceLifecycleBadge({ status }: { readonly status: string }) {
  return (
    <Badge variant="outline" className={lifecycleClasses(status)}>
      {status}
    </Badge>
  );
}
