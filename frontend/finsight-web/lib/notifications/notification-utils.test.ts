import { describe, expect, it } from "vitest";

import { isNotificationUnread, notificationHref, notificationStatusLabel, notificationTypeLabel } from "@/lib/notifications/notification-utils";
import type { Notification } from "@/lib/api/types";

const base: Notification = {
  id: "notification-1",
  type: "FinancialInsight",
  channel: "InApp",
  title: "Insight available",
  message: "A new insight is ready.",
  status: "Delivered",
  createdAt: "2026-09-17T12:00:00Z",
  readAt: null,
};

describe("notification utilities", () => {
  it("maps semantic notification types to labels and destinations", () => {
    expect(notificationTypeLabel("SubscriptionPriceChanged")).toBe("Subscription price change");
    expect(notificationHref({ ...base, type: "AnomalyDetected" })).toBe("/anomalies");
    expect(notificationHref({ ...base, type: "SubscriptionDetected" })).toBe("/subscriptions");
  });

  it("identifies unread and delivery states", () => {
    expect(isNotificationUnread(base)).toBe(true);
    expect(notificationStatusLabel({ ...base, status: "Read" })).toBe("Read");
    expect(notificationStatusLabel({ ...base, status: "DeadLettered" })).toBe("Delivery stopped");
    expect(isNotificationUnread({ ...base, status: "Dismissed" })).toBe(false);
  });
});
