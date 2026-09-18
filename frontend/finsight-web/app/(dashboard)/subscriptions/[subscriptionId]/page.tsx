import { SubscriptionDetail } from "@/components/subscriptions/subscription-detail";

interface SubscriptionPageProps {
  readonly params: Promise<{ subscriptionId: string }>;
}

/** Renders detailed subscription intelligence for one user-owned subscription. */
export default async function SubscriptionPage({ params }: SubscriptionPageProps) {
  const { subscriptionId } = await params;
  return <SubscriptionDetail subscriptionId={subscriptionId} />;
}
