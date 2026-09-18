"use client";

import { useEffect, type ReactNode } from "react";
import { usePathname, useRouter } from "next/navigation";

import { PageLoader } from "@/components/feedback/page-loader";
import { useAuth } from "@/lib/auth/auth-context";

/**
 * Protects authenticated application routes and preserves the requested path for post-login navigation.
 */
export function AuthGuard({ children }: { readonly children: ReactNode }) {
  const { status } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (status !== "unauthenticated") return;

    const returnUrl = pathname && pathname !== "/login" ? pathname : "/dashboard";
    router.replace(`/login?returnUrl=${encodeURIComponent(returnUrl)}`);
  }, [pathname, router, status]);

  if (status === "loading") {
    return <PageLoader />;
  }

  if (status === "unauthenticated") {
    return <PageLoader />;
  }

  return <>{children}</>;
}
