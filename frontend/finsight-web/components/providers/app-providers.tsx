"use client";

import { type ReactNode } from "react";

import { ClientRuntimeTelemetry } from "@/components/providers/client-runtime-telemetry";
import { QueryProvider } from "@/components/providers/query-provider";
import { Toaster, toast } from "@/components/ui/toast";
import { AuthProvider } from "@/lib/auth/auth-context";
import { ThemeProvider } from "@/lib/theme/theme-provider";

interface AppProvidersProps {
  readonly children: ReactNode;
}

/** Composes client-side authentication, data-fetching, theme, feedback and runtime diagnostics. */
export function AppProviders({ children }: AppProvidersProps) {
  return (
    <QueryProvider>
      <AuthProvider>
        <ThemeProvider>
          <Toaster toastManager={toast} limit={3} timeout={5000}>
            <ClientRuntimeTelemetry />
            {children}
          </Toaster>
        </ThemeProvider>
      </AuthProvider>
    </QueryProvider>
  );
}
