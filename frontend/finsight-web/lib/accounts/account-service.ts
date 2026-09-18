import type { ApiClient } from "@/lib/api/client";
import type {
  Account,
  AccountConnection,
  AccountSyncResponse,
  Institution,
  Transaction,
} from "@/lib/api/types";

/** Provides the financial account and banking connection API surface. */
export class AccountService {
  /** Creates an account service using the supplied authenticated API client. */
  public constructor(private readonly api: ApiClient) {}

  /** Gets financial accounts owned by the current user. */
  public getAccounts(): Promise<readonly Account[]> {
    return this.api.get<readonly Account[]>("/accounts");
  }

  /** Gets active bank connections owned by the current user. */
  public getConnections(): Promise<readonly AccountConnection[]> {
    return this.api.get<readonly AccountConnection[]>("/accounts/connections");
  }

  /** Gets the supported active financial institutions. */
  public getInstitutions(): Promise<readonly Institution[]> {
    return this.api.get<readonly Institution[]>("/institutions");
  }

  /** Gets one financial account owned by the current user. */
  public getAccount(accountId: string): Promise<Account> {
    return this.api.get<Account>(`/accounts/${accountId}`);
  }

  /** Gets recent transactions for one financial account. */
  public getAccountTransactions(accountId: string, limit = 100) {
    return this.api.get<readonly AccountTransaction[]>(`/accounts/${accountId}/transactions`, {
      query: { limit },
    });
  }

  /** Connects the current user to a supported institution. */
  public connect(institutionCode: string): Promise<string> {
    return this.api.post<string>("/accounts/connections", { institutionCode });
  }

  /** Runs an immediate synchronization for an account connection. */
  public sync(connectionId: string): Promise<AccountSyncResponse> {
    return this.api.post<AccountSyncResponse>(`/accounts/connections/${connectionId}/sync`);
  }

  /** Disconnects a financial institution connection. */
  public async disconnect(connectionId: string): Promise<void> {
    await this.api.delete<void>(`/accounts/connections/${connectionId}`);
  }
}

/** Represents a transaction returned by the account transaction endpoint. */
export type AccountTransaction = Omit<Transaction, "accountId">;
