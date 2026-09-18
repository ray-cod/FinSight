import type { ReactNode } from "react";

import { PublicOnlyGuard } from "@/components/auth/public-only-guard";

export default function AuthLayout({ children }: { readonly children: ReactNode }) {
  return <PublicOnlyGuard>{children}</PublicOnlyGuard>;
}
