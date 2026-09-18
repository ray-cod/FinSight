import type { Metadata } from "next";

import { AnomaliesWorkspace } from "@/components/anomalies/anomalies-workspace";

export const metadata: Metadata = {
  title: "Anomalies",
  description: "Review unusual financial activity detected by FinSight.",
};

/** Renders the authenticated anomaly intelligence workspace. */
export default function AnomaliesPage() {
  return <AnomaliesWorkspace />;
}
