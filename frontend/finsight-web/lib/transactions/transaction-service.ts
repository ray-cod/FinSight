import type { ApiClient } from "@/lib/api/client";
import type {
  TransactionDetail,
  TransactionListQuery,
  TransactionListResponse,
  UpdateTransactionClassificationRequest,
} from "@/lib/api/types";

/** Provides the user-owned transaction API surface. */
export class TransactionService {
  /** Creates a transaction service using the supplied API client. */
  public constructor(private readonly api: ApiClient) {}

  /** Gets a filtered and paginated transaction page. */
  public getTransactions(query: TransactionListQuery = {}): Promise<TransactionListResponse> {
    return this.api.get<TransactionListResponse>("/transactions", {
      query,
    });
  }

  /** Gets one transaction with contextual account, merchant and taxonomy details. */
  public getTransaction(transactionId: string): Promise<TransactionDetail> {
    return this.api.get<TransactionDetail>(`/transactions/${transactionId}`);
  }

  /** Applies a user-selected category correction to a transaction. */
  public async correctClassification(
    transactionId: string,
    request: UpdateTransactionClassificationRequest,
  ): Promise<void> {
    await this.api.put<void>(`/transactions/${transactionId}/classification`, request);
  }
}
