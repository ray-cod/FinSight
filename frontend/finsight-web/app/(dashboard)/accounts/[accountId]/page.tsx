import { AccountDetail } from "@/components/accounts/account-detail";

interface AccountPageProps {
  readonly params: Promise<{ accountId: string }>;
}

/** Renders the authenticated detail view for one financial account. */
export default async function AccountPage({ params }: AccountPageProps) {
  const { accountId } = await params;
  return <AccountDetail accountId={accountId} />;
}
