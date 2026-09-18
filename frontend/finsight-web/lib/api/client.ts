import { clientEnv } from "@/lib/config/env";
import { ApiError, type ApiProblemDetails } from "@/lib/errors/api-error";

const API_PREFIX = "/api/v1";
const DEFAULT_TIMEOUT_MS = 15_000;

export type AccessTokenProvider = () => string | null | Promise<string | null>;
export type UnauthorizedHandler = () => Promise<boolean>;

export interface RequestOptions extends Omit<RequestInit, "body"> {
    readonly body?: unknown;
    readonly query?: object;
    readonly accessToken?: string | null;
    readonly timeoutMs?: number;
    readonly retryOnUnauthorized?: boolean;
}

function buildQuery(query: RequestOptions["query"]): string {
    if (!query) return "";

    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(query)) {
        if (value !== null && value !== undefined)
            params.set(key, String(value));
    }

    const encoded = params.toString();
    return encoded ? `?${encoded}` : "";
}

function createCorrelationId(): string {
    if (typeof crypto !== "undefined" && "randomUUID" in crypto) {
        return crypto.randomUUID();
    }
    return `finsight-${Date.now()}-${Math.random().toString(36).slice(2)}`;
}

async function parseResponseBody(response: Response): Promise<unknown> {
    if (response.status === 204) return undefined;

    const contentType = response.headers.get("content-type") ?? "";
    if (
        contentType.includes("application/json") ||
        contentType.includes("application/problem+json")
    ) {
        try {
            return await response.json();
        } catch {
            return undefined;
        }
    }

    const text = await response.text();
    return text || undefined;
}

/**
 * Provides a typed HTTP client for the versioned FinSight API, including one-time
 * access-token refresh/retry handling for authenticated requests.
 */
export class ApiClient {
    private readonly baseUrl: string;
    private readonly accessTokenProvider?: AccessTokenProvider;
    private readonly unauthorizedHandler?: UnauthorizedHandler;

    /** Creates an API client. */
    public constructor(
        baseUrl = clientEnv.NEXT_PUBLIC_API_BASE_URL,
        accessTokenProvider?: AccessTokenProvider,
        unauthorizedHandler?: UnauthorizedHandler,
    ) {
        this.baseUrl = baseUrl.replace(/\/$/, "");
        this.accessTokenProvider = accessTokenProvider;
        this.unauthorizedHandler = unauthorizedHandler;
    }

    /** Performs a typed HTTP request against the FinSight API. */
    public async request<T>(
        path: string,
        options: RequestOptions = {},
    ): Promise<T> {
        return this.execute<T>(path, options, false);
    }

    private async execute<T>(
        path: string,
        options: RequestOptions,
        hasRetried: boolean,
    ): Promise<T> {
        const {
            body,
            query: requestQuery,
            accessToken: optionAccessToken,
            timeoutMs = DEFAULT_TIMEOUT_MS,
            retryOnUnauthorized = true,
            ...requestInit
        } = options;

        const controller = new AbortController();
        const timeout = window.setTimeout(() => controller.abort(), timeoutMs);
        const correlationId = createCorrelationId();
        const accessToken =
            optionAccessToken ?? (await this.accessTokenProvider?.()) ?? null;
        const normalizedPath = path.startsWith("/") ? path : `/${path}`;
        const apiPath = normalizedPath.startsWith(API_PREFIX)
            ? normalizedPath
            : `${API_PREFIX}${normalizedPath}`;
        const headers = new Headers(requestInit.headers);

        headers.set("Accept", "application/json");
        headers.set("X-Correlation-ID", correlationId);

        if (body !== undefined) {
            headers.set("Content-Type", "application/json");
        }

        if (accessToken) {
            headers.set("Authorization", `Bearer ${accessToken}`);
        }

        try {
            const response = await fetch(
                `${this.baseUrl}${apiPath}${buildQuery(requestQuery)}`,
                {
                    ...requestInit,
                    body: body === undefined ? undefined : JSON.stringify(body),
                    credentials: "include",
                    headers,
                    signal: controller.signal,
                },
            );

            if (
                response.status === 401 &&
                retryOnUnauthorized &&
                !hasRetried &&
                this.unauthorizedHandler
            ) {
                const refreshed = await this.unauthorizedHandler();
                if (refreshed) return this.execute<T>(path, options, true);
            }

            const responseBody = await parseResponseBody(response);

            if (!response.ok) {
                const problem =
                    responseBody && typeof responseBody === "object"
                        ? (responseBody as ApiProblemDetails)
                        : null;
                const message =
                    problem?.detail ??
                    problem?.title ??
                    (typeof responseBody === "string"
                        ? responseBody
                        : `Request failed with status ${response.status}.`);

                throw new ApiError(
                    message,
                    response.status,
                    problem,
                    response.headers.get("X-Correlation-ID") ?? correlationId,
                );
            }

            return responseBody as T;
        } catch (error) {
            if (error instanceof ApiError) throw error;

            if (error instanceof DOMException && error.name === "AbortError") {
                throw new ApiError(
                    "The request timed out. Please try again.",
                    408,
                    null,
                    correlationId,
                );
            }

            throw new ApiError(
                "Unable to reach the FinSight API. Check your connection and try again.",
                0,
                null,
                correlationId,
            );
        } finally {
            window.clearTimeout(timeout);
        }
    }

    /** Performs a GET request. */
    public get<T>(
        path: string,
        options?: Omit<RequestOptions, "body" | "method">,
    ): Promise<T> {
        return this.request<T>(path, { ...options, method: "GET" });
    }

    /** Performs a POST request. */
    public post<T>(
        path: string,
        body?: unknown,
        options?: Omit<RequestOptions, "body" | "method">,
    ): Promise<T> {
        return this.request<T>(path, { ...options, method: "POST", body });
    }

    /** Performs a PUT request. */
    public put<T>(
        path: string,
        body?: unknown,
        options?: Omit<RequestOptions, "body" | "method">,
    ): Promise<T> {
        return this.request<T>(path, { ...options, method: "PUT", body });
    }

    /** Performs a PATCH request. */
    public patch<T>(
        path: string,
        body?: unknown,
        options?: Omit<RequestOptions, "body" | "method">,
    ): Promise<T> {
        return this.request<T>(path, { ...options, method: "PATCH", body });
    }

    /** Performs a DELETE request. */
    public delete<T>(
        path: string,
        options?: Omit<RequestOptions, "body" | "method">,
    ): Promise<T> {
        return this.request<T>(path, { ...options, method: "DELETE" });
    }
}

/** Creates an API client for the configured FinSight API origin. */
export function createApiClient(
    accessTokenProvider?: AccessTokenProvider,
    unauthorizedHandler?: UnauthorizedHandler,
): ApiClient {
    return new ApiClient(
        clientEnv.NEXT_PUBLIC_API_BASE_URL,
        accessTokenProvider,
        unauthorizedHandler,
    );
}
