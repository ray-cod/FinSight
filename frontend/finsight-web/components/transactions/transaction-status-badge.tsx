import { Badge } from "@/components/ui/badge";

/** Renders transaction lifecycle and classification statuses consistently. */
export function TransactionStatusBadge({ status, kind = "classification" }: { readonly status: string; readonly kind?: "classification" | "transaction" }) {
  const label = status || "Unknown";
  const positive = kind === "classification"
    ? label === "Classified" || label === "UserCorrected"
    : label === "Imported";
  const attention = label === "Uncertain" || label === "Pending" || label === "Failed";
  const destructive = label === "Cancelled";

  return (
    <Badge
      variant={
        destructive ? "destructive" : positive ? "secondary" : attention ? "outline" : "ghost"
      }
      className="font-normal"
    >
      {label === "UserCorrected" ? "User corrected" : label}
    </Badge>
  );
}
