/**
 * Backwards-compatible export for the frontend HTTP infrastructure.
 * Prefer importing from `@/lib/api/client` in new modules.
 */
export {
  ApiClient,
  createApiClient,
  type AccessTokenProvider,
  type RequestOptions,
  type UnauthorizedHandler,
} from "@/lib/api/client";
