const ACCESS_TOKEN_KEY = "finsight.access-token";
const REFRESH_TOKEN_KEY = "finsight.refresh-token";

function getStorage(): Storage | null {
  if (typeof window === "undefined") return null;

  try {
    return window.sessionStorage;
  } catch {
    return null;
  }
}

/**
 * Provides browser-session storage for the authentication token pair.
 *
 * The backend currently returns refresh tokens in its JSON contract rather than
 * setting a secure, HttpOnly cookie. Session storage limits persistence to the
 * browser session and keeps token handling behind one replaceable abstraction.
 */
export const tokenStore = {
  /** Returns the current access token, if one exists. */
  getAccessToken(): string | null {
    return getStorage()?.getItem(ACCESS_TOKEN_KEY) ?? null;
  },

  /** Returns the current refresh token, if one exists. */
  getRefreshToken(): string | null {
    return getStorage()?.getItem(REFRESH_TOKEN_KEY) ?? null;
  },

  /** Stores a newly issued token pair for the active browser session. */
  setTokens(accessToken: string, refreshToken: string): void {
    const storage = getStorage();
    if (!storage) return;

    storage.setItem(ACCESS_TOKEN_KEY, accessToken);
    storage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  },

  /** Replaces only the access token after a successful refresh. */
  setAccessToken(accessToken: string): void {
    const storage = getStorage();
    if (!storage) return;
    storage.setItem(ACCESS_TOKEN_KEY, accessToken);
  },

  /** Clears all authentication tokens from the browser session. */
  clear(): void {
    const storage = getStorage();
    if (!storage) return;
    storage.removeItem(ACCESS_TOKEN_KEY);
    storage.removeItem(REFRESH_TOKEN_KEY);
  },
};
