import { NextResponse } from "next/server";

/**
 * Returns the lightweight health status of the Next.js web runtime.
 *
 * @returns A non-cacheable JSON health response.
 */
export function GET(): NextResponse {
  return NextResponse.json(
    {
      status: "ok",
      service: "finsight-web",
      version: process.env.BUILD_VERSION ?? "unknown",
    },
    {
      status: 200,
      headers: { "Cache-Control": "no-store" },
    },
  );
}
