export interface LoginRequest {
    readonly email: string;
    readonly password: string;
}

export interface RegisterRequest {
    readonly email: string;
    readonly password: string;
    readonly displayName: string;
}

export interface ResetPasswordRequest {
    readonly email: string;
    readonly token: string;
    readonly newPassword: string;
}

export interface CurrentUser {
    readonly userId: string;
    readonly email: string;
    readonly displayName: string;
    readonly emailConfirmed: boolean;
    readonly createdAt: string;
}

export interface AuthResponse {
    readonly accessToken: string;
    readonly refreshToken: string;
    readonly accessTokenExpiresAt: string;
    readonly userId: string;
    readonly sessionId: string;
}

export type NotificationType =
    | "AnomalyDetected"
    | "SubscriptionPriceChanged"
    | "SubscriptionDetected"
    | "FinancialInsight"
    | "Security";

export type NotificationChannel = "InApp" | "Email";

export type NotificationStatus =
    | "Pending"
    | "Delivered"
    | "Failed"
    | "DeadLettered"
    | "Read"
    | "Dismissed";

export interface Notification {
    readonly id: string;
    readonly type: NotificationType;
    readonly channel: NotificationChannel;
    readonly title: string;
    readonly message: string;
    readonly status: NotificationStatus;
    readonly createdAt: string;
    readonly readAt: string | null;
}

export interface UnreadNotificationCountResponse {
    readonly count: number;
}

export interface NotificationPreferences {
    readonly inAppEnabled: boolean;
    readonly emailEnabled: boolean;
    readonly anomalyNotificationsEnabled: boolean;
    readonly subscriptionNotificationsEnabled: boolean;
    readonly insightNotificationsEnabled: boolean;
}

export type UpdateNotificationPreferencesRequest = NotificationPreferences;

export interface NotificationReadAllResponse {
    readonly count: number;
    readonly updatedCount: number;
}

export interface DashboardBalance {
    readonly currency: string;
    readonly currentBalance: number;
    readonly availableBalance: number;
    readonly accountCount: number;
}

export interface DashboardTrendPoint {
    readonly label: string;
    readonly startDate: string;
    readonly endDate: string;
    readonly income: number;
    readonly spending: number;
    readonly netCashFlow: number;
}

export interface DashboardCategorySpend {
    readonly categoryId: string | null;
    readonly categoryName: string;
    readonly amount: number;
    readonly percentage: number;
}

export interface DashboardTransaction {
    readonly id: string;
    readonly accountId: string;
    readonly merchantName: string;
    readonly currency: string;
    readonly amount: number;
    readonly transactionDate: string;
    readonly type: string;
    readonly categoryName: string;
    readonly classificationStatus: string;
}

export interface DashboardSubscription {
    readonly id: string;
    readonly merchantName: string;
    readonly currency: string;
    readonly currentAmount: number;
    readonly frequency: string;
    readonly nextExpectedChargeAt: string | null;
}

export interface DashboardAlert {
    readonly id: string;
    readonly kind: string;
    readonly severity: string;
    readonly title: string;
    readonly description: string;
    readonly occurredAt: string;
    readonly href: string;
}

export interface DashboardMetrics {
    readonly currentMonthSpending: number;
    readonly previousMonthSpending: number;
    readonly spendingChangePercentage: number;
    readonly currentMonthIncome: number;
    readonly activeSubscriptionCount: number;
    readonly openAnomalyCount: number;
    readonly activeInsightCount: number;
    readonly unreadNotificationCount: number;
}

export interface DashboardResponse {
    readonly primaryCurrency: string;
    readonly balances: readonly DashboardBalance[];
    readonly spendingTrend: readonly DashboardTrendPoint[];
    readonly categorySpending: readonly DashboardCategorySpend[];
    readonly recentTransactions: readonly DashboardTransaction[];
    readonly upcomingSubscriptions: readonly DashboardSubscription[];
    readonly priorityAlerts: readonly DashboardAlert[];
    readonly metrics: DashboardMetrics;
}

export interface Account {
    readonly id: string;
    readonly connectionId: string;
    readonly institutionId: string;
    readonly name: string;
    readonly type: string;
    readonly currency: string;
    readonly currentBalance: number;
    readonly availableBalance: number;
    readonly status: string;
}

export interface AccountConnection {
    readonly id: string;
    readonly institutionId: string;
    readonly providerCode: string;
    readonly institutionName: string;
    readonly status: string;
    readonly lastSuccessfulSyncAt: string | null;
    readonly lastSyncAttemptAt: string | null;
    readonly lastSyncError: string | null;
    readonly accountCount: number;
}

export interface AccountSyncResponse {
    readonly importedTransactions: number;
}

export interface Institution {
    readonly id: string;
    readonly providerCode: string;
    readonly name: string;
    readonly isActive: boolean;
}

export interface Transaction {
    readonly id: string;
    readonly accountId: string;
    readonly rawDescription: string;
    readonly normalizedDescription: string | null;
    readonly merchantId: string | null;
    readonly categoryId: string | null;
    readonly subcategoryId: string | null;
    readonly amount: number;
    readonly currency: string;
    readonly transactionDate: string;
    readonly type: string;
    readonly status: string;
    readonly classificationStatus: string;
    readonly classificationSource: string | null;
    readonly classificationConfidence: number | null;
    readonly classifiedAt: string | null;
}

export interface Anomaly {
    readonly id: string;
    readonly transactionId: string;
    readonly accountId: string;
    readonly type: string;
    readonly severity: string;
    readonly score: number;
    readonly confidence: number;
    readonly title: string;
    readonly description: string;
    readonly evidence: unknown;
    readonly detectedAt: string;
    readonly status: string;
    readonly resolvedAt: string | null;
}

export interface Insight {
    readonly id: string;
    readonly anomalyId: string | null;
    readonly transactionId: string | null;
    readonly type: string;
    readonly severity: string;
    readonly title: string;
    readonly message: string;
    readonly occurredAt: string;
    readonly expiresAt: string | null;
    readonly status: string;
    readonly createdAt: string;
}

export type AnomalyEvidence = string;

export interface IntelligenceListState {
    readonly includeResolved?: boolean;
    readonly includeDismissed?: boolean;
    readonly limit?: number;
}

export interface Subscription {
    readonly id: string;
    readonly merchantId: string;
    readonly merchantName: string;
    readonly currency: string;
    readonly frequency: string;
    readonly currentAmount: number;
    readonly averageAmount: number;
    readonly detectionConfidence: number;
    readonly firstDetectedAt: string;
    readonly lastChargeAt: string;
    readonly nextExpectedChargeAt: string | null;
    readonly lastPriceChangedAt: string | null;
    readonly status: string;
}

export interface CategorySubcategory {
    readonly id: string;
    readonly code: string;
    readonly name: string;
}

export interface Category {
    readonly id: string;
    readonly code: string;
    readonly name: string;
    readonly type: string;
    readonly subcategories: readonly CategorySubcategory[];
}

export interface TransactionListItem {
    readonly id: string;
    readonly accountId: string;
    readonly accountName: string;
    readonly institutionName: string;
    readonly merchantName: string;
    readonly rawDescription: string;
    readonly normalizedDescription: string;
    readonly categoryId: string | null;
    readonly categoryName: string | null;
    readonly subcategoryId: string | null;
    readonly subcategoryName: string | null;
    readonly amount: number;
    readonly currency: string;
    readonly transactionDate: string;
    readonly type: string;
    readonly status: string;
    readonly classificationStatus: string;
    readonly classificationSource: string;
    readonly classificationConfidence: number | null;
    readonly classifiedAt: string | null;
}

export interface TransactionDetail extends TransactionListItem {
    readonly institutionId: string;
    readonly providerTransactionId: string;
    readonly merchantId: string | null;
    readonly importedAt: string;
    readonly userCorrectedAt: string | null;
}

export interface TransactionListResponse {
    readonly items: readonly TransactionListItem[];
    readonly page: number;
    readonly pageSize: number;
    readonly totalCount: number;
    readonly totalPages: number;
    readonly hasNextPage: boolean;
    readonly hasPreviousPage: boolean;
}

export interface TransactionListQuery {
    readonly page?: number;
    readonly pageSize?: number;
    readonly search?: string;
    readonly accountId?: string;
    readonly categoryId?: string;
    readonly type?: string;
    readonly status?: string;
    readonly classificationStatus?: string;
    readonly from?: string;
    readonly to?: string;
    readonly minAmount?: number;
    readonly maxAmount?: number;
    readonly sortBy?: "date" | "amount" | "merchant";
    readonly descending?: boolean;
}

export interface UpdateTransactionClassificationRequest {
    readonly categoryId: string;
    readonly subcategoryId?: string | null;
}

export interface SubscriptionPriceObservation {
    readonly id: string;
    readonly transactionId: string;
    readonly amount: number;
    readonly observedAt: string;
}

export interface SecuritySession {
    readonly sessionId: string;
    readonly createdAt: string;
    readonly expiresAt: string;
    readonly ipAddress: string | null;
    readonly isCurrent: boolean;
}

export interface RevokeSessionsResponse {
    readonly revokedCount: number;
}

export interface SecurityActivityItem {
    readonly eventType: string;
    readonly occurredAt: string;
}
