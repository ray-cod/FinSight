import { format, formatDistanceToNow } from "date-fns";

/**
 * Formats a date for a compact financial activity display.
 */
export function formatDate(value: string | Date): string {
  return format(new Date(value), "MMM d, yyyy");
}

/**
 * Formats a date as relative activity text.
 */
export function formatRelativeDate(value: string | Date): string {
  return formatDistanceToNow(new Date(value), { addSuffix: true });
}
