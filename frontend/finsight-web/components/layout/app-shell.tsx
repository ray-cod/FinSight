import type { ReactNode } from "react";

import { NetworkStatusBanner } from "@/components/layout/network-status-banner";
import { AppHeader } from "@/components/navigation/app-header";
import { Sidebar } from "@/components/navigation/sidebar";

interface AppShellProps {
  readonly title?: string;
  readonly description?: string;
  readonly children: ReactNode;
}

/**
 * Provides the shared authenticated layout used by the FinSight product workspace.
 */
export function AppShell({ title, description, children }: AppShellProps) {
  return (
    <div className="flex min-h-screen bg-background">
      <a
        href="#main-content"
        className="fixed left-4 top-4 z-50 -translate-y-24 rounded-md bg-primary px-3 py-2 text-sm font-medium text-primary-foreground shadow-lg transition-transform focus:translate-y-0"
      >
        Skip to content
      </a>
      <Sidebar />
      <div className="flex min-w-0 flex-1 flex-col">
        <NetworkStatusBanner />
        <AppHeader title={title} description={description} />
        <main id="main-content" className="flex-1 px-4 py-6 sm:px-6 lg:px-8">
          <div className="mx-auto w-full max-w-[1600px]">{children}</div>
        </main>
      </div>
    </div>
  );
}
