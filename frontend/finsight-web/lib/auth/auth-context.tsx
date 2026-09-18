"use client";

import { useQueryClient } from "@tanstack/react-query";

import {
    createContext,
    useCallback,
    useContext,
    useEffect,
    useMemo,
    useRef,
    useState,
    type ReactNode,
} from "react";

import { createApiClient, type ApiClient } from "@/lib/api/client";
import type {
    AuthResponse,
    CurrentUser,
    LoginRequest,
    RegisterRequest,
    ResetPasswordRequest,
} from "@/lib/api/types";
import {
    AuthService,
    type ChangePasswordRequest,
    type ForgotPasswordRequest,
    type UpdateProfileRequest,
} from "@/lib/auth/auth-service";
import { tokenStore } from "@/lib/auth/token-store";
import { ApiError } from "@/lib/errors/api-error";

export type AuthStatus = "loading" | "authenticated" | "unauthenticated";

export interface AuthContextValue {
    readonly status: AuthStatus;
    readonly user: CurrentUser | null;
    readonly api: ApiClient;
    readonly login: (request: LoginRequest) => Promise<CurrentUser>;
    readonly register: (request: RegisterRequest) => Promise<CurrentUser>;
    readonly logout: () => Promise<void>;
    readonly refreshSession: () => Promise<boolean>;
    readonly forgotPassword: (request: ForgotPasswordRequest) => Promise<void>;
    readonly resetPassword: (request: ResetPasswordRequest) => Promise<void>;
    readonly changePassword: (request: ChangePasswordRequest) => Promise<void>;
    readonly updateProfile: (
        request: UpdateProfileRequest,
    ) => Promise<CurrentUser>;
}

const AuthContext = createContext<AuthContextValue | null>(null);
let refreshSessionHandler: (() => Promise<boolean>) | null = null;

/**
 * Owns the browser authentication lifecycle and exposes the authenticated FinSight API client.
 */
export function AuthProvider({ children }: { readonly children: ReactNode }) {
    const queryClient = useQueryClient();
    const [status, setStatus] = useState<AuthStatus>("loading");
    const [user, setUser] = useState<CurrentUser | null>(null);
    const refreshPromiseRef = useRef<Promise<boolean> | null>(null);
    const initializedRef = useRef(false);

    const rawClient = useMemo(() => createApiClient(() => null), []);
    const authService = useMemo(() => new AuthService(rawClient), [rawClient]);

    const refreshSession = useCallback(async (): Promise<boolean> => {
        if (refreshPromiseRef.current) return refreshPromiseRef.current;

        const refreshToken = tokenStore.getRefreshToken();
        if (!refreshToken) {
            tokenStore.clear();
            queryClient.clear();
            setUser(null);
            setStatus("unauthenticated");
            return false;
        }

        const promise = (async () => {
            try {
                const response = await authService.refresh(refreshToken);
                authService.persistTokens(response);
                const currentUser = await authService.currentUser(
                    response.accessToken,
                );
                setUser(currentUser);
                setStatus("authenticated");
                return true;
            } catch {
                tokenStore.clear();
                queryClient.clear();
                setUser(null);
                setStatus("unauthenticated");
                return false;
            } finally {
                refreshPromiseRef.current = null;
            }
        })();

        refreshPromiseRef.current = promise;
        return promise;
    }, [authService, queryClient]);

    useEffect(() => {
        refreshSessionHandler = refreshSession;

        return () => {
            if (refreshSessionHandler === refreshSession) {
                refreshSessionHandler = null;
            }
        };
    }, [refreshSession]);

    const api = useMemo(
        () =>
            createApiClient(
                () => tokenStore.getAccessToken(),
                async () => refreshSessionHandler?.() ?? false,
            ),
        [],
    );

    const authenticate = useCallback(
        async (response: AuthResponse): Promise<CurrentUser> => {
            try {
                authService.persistTokens(response);
                const currentUser = await authService.currentUser(
                    response.accessToken,
                );
                setUser(currentUser);
                setStatus("authenticated");
                return currentUser;
            } catch (error) {
                tokenStore.clear();
                queryClient.clear();
                setUser(null);
                setStatus("unauthenticated");
                throw error;
            }
        },
        [authService, queryClient],
    );

    const login = useCallback(
        async (request: LoginRequest) =>
            authenticate(await authService.login(request)),
        [authService, authenticate],
    );

    const register = useCallback(
        async (request: RegisterRequest) =>
            authenticate(await authService.register(request)),
        [authService, authenticate],
    );

    const logout = useCallback(async () => {
        const refreshToken = tokenStore.getRefreshToken();
        tokenStore.clear();
        queryClient.clear();
        setUser(null);
        setStatus("unauthenticated");

        if (!refreshToken) return;

        try {
            await authService.logout(refreshToken);
        } catch {
            // Local session state is already cleared; revocation is best-effort from the browser.
        }
    }, [authService, queryClient]);

    const forgotPassword = useCallback(
        (request: ForgotPasswordRequest) => authService.forgotPassword(request),
        [authService],
    );

    const resetPassword = useCallback(
        (request: ResetPasswordRequest) => authService.resetPassword(request),
        [authService],
    );

    const changePassword = useCallback(
        async (request: ChangePasswordRequest) => {
            await authService.changePassword(request);
            tokenStore.clear();
            queryClient.clear();
            setUser(null);
            setStatus("unauthenticated");
        },
        [authService, queryClient],
    );

    const updateProfile = useCallback(
        async (request: UpdateProfileRequest) => {
            const updatedUser = await authService.updateProfile(request);
            setUser(updatedUser);
            return updatedUser;
        },
        [authService],
    );

    useEffect(() => {
        if (initializedRef.current) return;
        initializedRef.current = true;

        void (async () => {
            const refreshToken = tokenStore.getRefreshToken();
            if (refreshToken) {
                await refreshSession();
                return;
            }

            tokenStore.clear();
            setStatus("unauthenticated");
        })();
    }, [refreshSession]);

    const value = useMemo<AuthContextValue>(
        () => ({
            api,
            status,
            user,
            login,
            register,
            logout,
            refreshSession,
            forgotPassword,
            resetPassword,
            changePassword,
            updateProfile,
        }),
        [
            api,
            changePassword,
            forgotPassword,
            login,
            logout,
            refreshSession,
            register,
            resetPassword,
            status,
            updateProfile,
            user,
        ],
    );

    return (
        <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
    );
}

/** Returns the current FinSight authentication context. */
export function useAuth(): AuthContextValue {
    const context = useContext(AuthContext);
    if (!context)
        throw new Error("useAuth must be used within an AuthProvider.");
    return context;
}

/** Returns an auth-safe human-readable error message for forms and alerts. */
export function getAuthErrorMessage(error: unknown): string {
    if (error instanceof ApiError) {
        if (error.status === 409 || /already exists/i.test(error.message))
            return "An account with this email already exists.";
        if (error.status === 401)
            return error.message || "The email or password is incorrect.";
        if (error.status === 400)
            return error.message || "Please check the information you entered.";
        if (error.status === 429)
            return "Too many attempts. Please wait a moment and try again.";
        if (error.status === 0)
            return "FinSight could not be reached. Check that the API is running.";
        return error.message || "Something went wrong. Please try again.";
    }

    return "Something went wrong. Please try again.";
}
