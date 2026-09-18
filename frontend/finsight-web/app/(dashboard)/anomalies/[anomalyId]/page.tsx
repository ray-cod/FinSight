import type { Metadata } from "next";

import { AnomalyDetail } from "@/components/anomalies/anomaly-detail";

export const metadata: Metadata = {
  title: "Anomaly detail",
};

/** Renders an individual financial anomaly. */
export default async function AnomalyDetailPage({ params }: { readonly params: Promise<{ anomalyId: string }> }) {
  const { anomalyId } = await params;
  return <AnomalyDetail anomalyId={anomalyId} />;
}
