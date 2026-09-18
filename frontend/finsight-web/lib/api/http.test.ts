import { afterEach, describe, expect, it, vi } from "vitest";

import { ApiClient } from "@/lib/api/http";

afterEach(() => {
  vi.restoreAllMocks();
});

describe("ApiClient", () => {
  it("builds versioned requests with correlation and authorization headers", async () => {
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(JSON.stringify({ ok: true }), {
        status: 200,
        headers: { "content-type": "application/json" },
      }),
    );

    const client = new ApiClient("https://api.example.test", () => "token");

    await expect(
      client.get<{ ok: boolean }>("/accounts", {
        query: { limit: 10, includeResolved: false },
      }),
    ).resolves.toEqual({ ok: true });

    const [, request] = fetchMock.mock.calls[0] ?? [];
    const headers = new Headers((request as RequestInit).headers);

    expect(fetchMock.mock.calls[0]?.[0]).toBe(
      "https://api.example.test/api/v1/accounts?limit=10&includeResolved=false",
    );
    expect(headers.get("Authorization")).toBe("Bearer token");
    expect(headers.get("X-Correlation-ID")).toBeTruthy();
  });

  it("normalizes problem-details responses", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(
        JSON.stringify({
          title: "Unauthorized",
          status: 401,
          detail: "Authentication is required.",
        }),
        {
          status: 401,
          headers: { "content-type": "application/problem+json" },
        },
      ),
    );

    const client = new ApiClient("https://api.example.test");

    await expect(client.get("/users/me")).rejects.toMatchObject({
      status: 401,
      message: "Authentication is required.",
    });
  });
});

describe("ApiClient refresh behavior", () => {
  it("refreshes once after a 401 and retries the original request", async () => {
    let callCount = 0;
    const fetchMock = vi.spyOn(globalThis, "fetch").mockImplementation(async () => {
      callCount += 1;
      if (callCount === 1) {
        return new Response(JSON.stringify({ title: "Unauthorized" }), {
          status: 401,
          headers: { "content-type": "application/problem+json" },
        });
      }

      return new Response(JSON.stringify({ ok: true }), {
        status: 200,
        headers: { "content-type": "application/json" },
      });
    });

    let token = "expired-token";
    const refresh = vi.fn(async () => {
      token = "fresh-token";
      return true;
    });
    const client = new ApiClient("https://api.example.test", () => token, refresh);

    await expect(client.get<{ ok: boolean }>("/users/me")).resolves.toEqual({ ok: true });
    expect(refresh).toHaveBeenCalledOnce();
    expect(fetchMock).toHaveBeenCalledTimes(2);

    const secondRequest = fetchMock.mock.calls[1]?.[1];
    const headers = new Headers((secondRequest as RequestInit).headers);
    expect(headers.get("Authorization")).toBe("Bearer fresh-token");
  });

  it("does not refresh an authentication endpoint", async () => {
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(JSON.stringify({ title: "Unauthorized" }), {
        status: 401,
        headers: { "content-type": "application/problem+json" },
      }),
    );
    const refresh = vi.fn(async () => true);
    const client = new ApiClient("https://api.example.test", undefined, refresh);

    await expect(
      client.post("/auth/login", { email: "alex@example.com", password: "bad" }, { retryOnUnauthorized: false }),
    ).rejects.toMatchObject({ status: 401 });

    expect(refresh).not.toHaveBeenCalled();
    expect(fetchMock).toHaveBeenCalledOnce();
  });
});
