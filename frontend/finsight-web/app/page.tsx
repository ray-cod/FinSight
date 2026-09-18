"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

import { PageLoader } from "@/components/feedback/page-loader";
import { useAuth } from "@/lib/auth/auth-context";

export default function HomePage() {
  const { status } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (status === "authenticated") {
      router.replace("/dashboard");
    } else if (status === "unauthenticated") {
      router.replace("/login");
    }
  }, [router, status]);

  return <PageLoader />;
}
