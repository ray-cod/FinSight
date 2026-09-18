import type { ApiClient } from "@/lib/api/client";
import type {
  Notification,
  NotificationPreferences,
  NotificationReadAllResponse,
  UnreadNotificationCountResponse,
  UpdateNotificationPreferencesRequest,
} from "@/lib/api/types";

/** Provides the authenticated notification API surface. */
export class NotificationService {
  /** Creates a notification service for the supplied API client. */
  public constructor(private readonly api: ApiClient) {}

  /** Gets the most recent in-app notifications. */
  public getNotifications(includeRead = true, limit = 100): Promise<readonly Notification[]> {
    return this.api.get<readonly Notification[]>("/notifications", {
      query: { includeRead, limit },
    });
  }

  /** Gets the current unread in-app notification count. */
  public getUnreadCount(): Promise<UnreadNotificationCountResponse> {
    return this.api.get<UnreadNotificationCountResponse>("/notifications/unread-count");
  }

  /** Marks one in-app notification as read. */
  public async markRead(notificationId: string): Promise<void> {
    await this.api.post<void>(`/notifications/${notificationId}/read`);
  }

  /** Dismisses one in-app notification without deleting its history. */
  public async dismiss(notificationId: string): Promise<void> {
    await this.api.post<void>(`/notifications/${notificationId}/dismiss`);
  }

  /** Marks all eligible delivered in-app notifications as read. */
  public markAllRead(): Promise<NotificationReadAllResponse> {
    return this.api.post<NotificationReadAllResponse>("/notifications/read-all");
  }

  /** Gets the current user's notification preferences. */
  public getPreferences(): Promise<NotificationPreferences> {
    return this.api.get<NotificationPreferences>("/notifications/preferences");
  }

  /** Replaces the current user's notification preferences. */
  public async updatePreferences(request: UpdateNotificationPreferencesRequest): Promise<void> {
    await this.api.put<void>("/notifications/preferences", request);
  }
}
