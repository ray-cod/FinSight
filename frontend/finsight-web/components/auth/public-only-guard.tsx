"use client";

import { useEffect, type ReactNode } from "react";
import { usePathname, useRouter } from "next/navigation";

import { PageLoader } from "@/components/feedback/page-loader";
import { useAuth } from "@/lib/auth/auth-context";

/** Prevents authenticated users from returning to login and registration screens. */
export function PublicOnlyGuard({ children }: { readonly children: ReactNode }) {
  const { status } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (status === "authenticated" && pathname !== "/reset-password") {
      router.replace("/dashboard");
    }
  }, [pathname, router, status]);

  if (status === "loading") return <PageLoader />;
  if (status === "authenticated" && pathname !== "/reset-password") return <PageLoader />;

  return <>{children}</>;
}
