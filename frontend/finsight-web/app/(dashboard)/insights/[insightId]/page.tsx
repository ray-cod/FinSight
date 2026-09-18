import type { Metadata } from "next";

import { InsightDetail } from "@/components/insights/insight-detail";

export const metadata: Metadata = {
  title: "Insight detail",
};

/** Renders an individual financial insight. */
export default async function InsightDetailPage({ params }: { readonly params: Promise<{ insightId: string }> }) {
  const { insightId } = await params;
  return <InsightDetail insightId={insightId} />;
}
