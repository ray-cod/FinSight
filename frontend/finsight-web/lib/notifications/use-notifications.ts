"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { useAuth } from "@/lib/auth/auth-context";
import { NotificationService } from "@/lib/notifications/notification-service";
import { queryKeys } from "@/lib/queries/query-keys";
import type { UpdateNotificationPreferencesRequest } from "@/lib/api/types";

/** Loads recent in-app notifications for the authenticated user. */
export function useNotifications(includeRead = true, limit = 100) {
  const { api } = useAuth();
  const service = new NotificationService(api);

  return useQuery({
    queryKey: queryKeys.notificationsList(includeRead, limit),
    queryFn: () => service.getNotifications(includeRead, limit),
    staleTime: 15_000,
  });
}

/** Loads the unread in-app notification count for the application chrome. */
export function useUnreadNotificationCount() {
  const { api } = useAuth();
  const service = new NotificationService(api);

  return useQuery({
    queryKey: queryKeys.notificationUnreadCount(),
    queryFn: () => service.getUnreadCount(),
    staleTime: 15_000,
    refetchInterval: 60_000,
  });
}

/** Marks one notification as read and refreshes dependent views. */
export function useMarkNotificationRead() {
  const { api } = useAuth();
  const service = new NotificationService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (notificationId: string) => service.markRead(notificationId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.notifications() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
      ]);
    },
  });
}

/** Dismisses one notification and refreshes dependent views. */
export function useDismissNotification() {
  const { api } = useAuth();
  const service = new NotificationService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (notificationId: string) => service.dismiss(notificationId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.notifications() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
      ]);
    },
  });
}

/** Marks all eligible notifications as read and refreshes dependent views. */
export function useMarkAllNotificationsRead() {
  const { api } = useAuth();
  const service = new NotificationService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => service.markAllRead(),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.notifications() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard() }),
      ]);
    },
  });
}

/** Loads the current user's notification preferences. */
export function useNotificationPreferences() {
  const { api } = useAuth();
  const service = new NotificationService(api);

  return useQuery({
    queryKey: queryKeys.notificationPreferences(),
    queryFn: () => service.getPreferences(),
    staleTime: 60_000,
  });
}

/** Saves the current user's notification preferences. */
export function useUpdateNotificationPreferences() {
  const { api } = useAuth();
  const service = new NotificationService(api);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateNotificationPreferencesRequest) => service.updatePreferences(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.notificationPreferences() });
    },
  });
}
