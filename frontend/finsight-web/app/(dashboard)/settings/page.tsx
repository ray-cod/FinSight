import { Suspense } from "react";

import { PageLoader } from "@/components/feedback/page-loader";
import { SettingsWorkspace } from "@/components/settings/settings-workspace";

export default function Page() {
  return (
    <div className="space-y-6">
      <p className="sr-only">Manage your FinSight profile, password, active sessions and notification preferences from one consolidated workspace.</p>
      <Suspense fallback={<PageLoader />}>
        <SettingsWorkspace />
      </Suspense>
    </div>
  );
}
