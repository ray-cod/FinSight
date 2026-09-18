import type { Metadata } from "next";

import { InsightsWorkspace } from "@/components/insights/insights-workspace";

export const metadata: Metadata = {
  title: "Insights",
  description: "Review FinSight's financial intelligence and explanations.",
};

/** Renders the authenticated financial insights workspace. */
export default function InsightsPage() {
  return <InsightsWorkspace />;
}
