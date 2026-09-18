import { TransactionDetail } from "@/components/transactions/transaction-detail";

interface TransactionPageProps {
  readonly params: Promise<{ transactionId: string }>;
}

/** Renders the authenticated details for one owned transaction. */
export default async function TransactionPage({ params }: TransactionPageProps) {
  const { transactionId } = await params;
  return <TransactionDetail transactionId={transactionId} />;
}
