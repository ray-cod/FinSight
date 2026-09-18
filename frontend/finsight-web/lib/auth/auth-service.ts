import { ApiClient } from "@/lib/api/client";
import type {
    AuthResponse,
    CurrentUser,
    LoginRequest,
    RegisterRequest,
    ResetPasswordRequest,
    RevokeSessionsResponse,
    SecuritySession,
} from "@/lib/api/types";
import { tokenStore } from "@/lib/auth/token-store";

export interface ForgotPasswordRequest {
    readonly email: string;
}

export interface ChangePasswordRequest {
    readonly currentPassword: string;
    readonly newPassword: string;
}

export interface UpdateProfileRequest {
    readonly displayName: string;
}

/**
 * Provides raw authentication API operations that do not depend on an authenticated request interceptor.
 */
export class AuthService {
    private readonly client: ApiClient;

    /** Creates an authentication service for the configured FinSight API. */
    public constructor(client: ApiClient) {
        this.client = client;
    }

    /** Authenticates a user and returns the issued token pair. */
    public login(request: LoginRequest): Promise<AuthResponse> {
        return this.client.post<AuthResponse>("/auth/login", request, {
            retryOnUnauthorized: false,
        });
    }

    /** Registers a new user and returns the issued token pair. */
    public register(request: RegisterRequest): Promise<AuthResponse> {
        return this.client.post<AuthResponse>("/auth/register", request, {
            retryOnUnauthorized: false,
        });
    }

    /** Rotates the current refresh token and returns a new token pair. */
    public refresh(refreshToken: string): Promise<AuthResponse> {
        return this.client.post<AuthResponse>(
            "/auth/refresh",
            { refreshToken },
            {
                retryOnUnauthorized: false,
            },
        );
    }

    /** Revokes the supplied refresh token. */
    public logout(refreshToken: string): Promise<void> {
        return this.client.post<void>(
            "/auth/logout",
            { refreshToken },
            {
                retryOnUnauthorized: false,
            },
        );
    }

    /** Retrieves the authenticated user's current profile. */
    public currentUser(accessToken: string): Promise<CurrentUser> {
        return this.client.get<CurrentUser>("/users/me", { accessToken });
    }

    /** Requests a password reset without disclosing whether an account exists. */
    public forgotPassword(request: ForgotPasswordRequest): Promise<void> {
        return this.client.post<void>("/auth/forgot-password", request, {
            retryOnUnauthorized: false,
        });
    }

    /** Resets a password using an email address and reset token. */
    public resetPassword(request: ResetPasswordRequest): Promise<void> {
        return this.client.post<void>("/auth/reset-password", request, {
            retryOnUnauthorized: false,
        });
    }

    /** Changes the authenticated user's password. */
    public changePassword(request: ChangePasswordRequest): Promise<void> {
        return this.client.post<void>("/auth/change-password", request);
    }
    /** Retrieves the authenticated user's active sessions. */
    public getSessions(): Promise<SecuritySession[]> {
        return this.client.get<SecuritySession[]>("/auth/sessions");
    }

    /** Revokes every active session except the current session. */
    public revokeOtherSessions(): Promise<RevokeSessionsResponse> {
        return this.client.post<RevokeSessionsResponse>(
            "/auth/sessions/revoke-others",
        );
    }

    /** Updates the authenticated user's display name. */
    public updateProfile(request: UpdateProfileRequest): Promise<CurrentUser> {
        return this.client.patch<CurrentUser>("/users/me", request);
    }

    /** Stores tokens returned by the backend in the active browser session. */
    public persistTokens(response: AuthResponse): void {
        tokenStore.setTokens(response.accessToken, response.refreshToken);
    }
}
