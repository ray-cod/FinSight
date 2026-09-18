import type { Notification, NotificationType } from "@/lib/api/types";

/** Returns a stable user-facing label for a notification type. */
export function notificationTypeLabel(type: NotificationType): string {
  switch (type) {
    case "AnomalyDetected":
      return "Anomaly";
    case "SubscriptionPriceChanged":
      return "Subscription price change";
    case "SubscriptionDetected":
      return "New subscription";
    case "FinancialInsight":
      return "Financial insight";
    case "Security":
      return "Security";
  }
}

/** Returns the best destination available from a notification's semantic type. */
export function notificationHref(notification: Notification): string | null {
  switch (notification.type) {
    case "AnomalyDetected":
      return "/anomalies";
    case "SubscriptionPriceChanged":
    case "SubscriptionDetected":
      return "/subscriptions";
    case "FinancialInsight":
      return "/insights";
    case "Security":
      return "/settings";
  }
}

/** Returns whether a notification still needs user review. */
export function isNotificationUnread(notification: Notification): boolean {
  return notification.status !== "Read" && notification.status !== "Dismissed";
}

/** Returns a stable user-facing label for a delivery status. */
export function notificationStatusLabel(notification: Notification): string {
  switch (notification.status) {
    case "Pending":
      return "Sending";
    case "Delivered":
      return "Delivered";
    case "Read":
      return "Read";
    case "Dismissed":
      return "Dismissed";
    case "Failed":
      return "Delivery failed";
    case "DeadLettered":
      return "Delivery stopped";
  }
}
