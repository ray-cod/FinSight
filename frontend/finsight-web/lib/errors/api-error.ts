/**
 * Represents a normalized API problem response.
 */
export interface ApiProblemDetails {
  readonly type?: string;
  readonly title?: string;
  readonly status?: number;
  readonly detail?: string;
  readonly instance?: string;
  readonly traceId?: string;
  readonly errors?: Record<string, readonly string[]>;
}

/**
 * Represents an HTTP failure returned by the FinSight API.
 */
export class ApiError extends Error {
  public readonly status: number;
  public readonly problem: ApiProblemDetails | null;
  public readonly correlationId: string | null;

  /**
   * Creates an API error.
   */
  public constructor(
    message: string,
    status: number,
    problem: ApiProblemDetails | null = null,
    correlationId: string | null = null,
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
    this.correlationId = correlationId;
  }
}

/**
 * Determines whether an unknown error was produced by the API client.
 */
export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError;
}
