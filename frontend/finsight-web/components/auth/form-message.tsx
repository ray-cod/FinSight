import { AlertCircle } from "lucide-react";

import { Alert, AlertDescription } from "@/components/ui/alert";

/** Renders a consistent non-field form error. */
export function FormMessage({ message }: { readonly message?: string }) {
  if (!message) return null;

  return (
    <Alert variant="destructive">
      <AlertCircle />
      <AlertDescription>{message}</AlertDescription>
    </Alert>
  );
}
