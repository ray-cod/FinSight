"use client";

import Link from "next/link";
import { ArrowRight, Landmark, RefreshCw, WalletCards } from "lucide-react";

import { AccountCard } from "@/components/accounts/account-card";
import { ConnectionCard } from "@/components/accounts/connection-card";
import { ConnectAccountDialog } from "@/components/accounts/connect-account-dialog";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
    useAccounts,
    useAccountConnections,
    useInstitutions,
} from "@/lib/accounts/use-accounts";
import { DashboardErrorState } from "@/components/dashboard/dashboard-error-state";
import { formatCurrency } from "@/lib/formatting/currency";

function formatStatusCount(connections: readonly { status: string }[]): string {
    const active = connections.filter(
        (connection) => connection.status === "Connected",
    ).length;
    return `${active} connected`;
}

function LoadingAccounts() {
    return (
        <div className="space-y-8" aria-busy="true">
            <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
                <div className="space-y-2">
                    <Skeleton className="h-8 w-56" />
                    <Skeleton className="h-4 w-96 max-w-full" />
                </div>
                <Skeleton className="h-10 w-40" />
            </div>
            <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
                {Array.from({ length: 3 }, (_, index) => (
                    <Skeleton key={index} className="h-56 rounded-xl" />
                ))}
            </div>
            <Skeleton className="h-60 rounded-xl" />
        </div>
    );
}

/** Renders the complete financial account and banking workspace. */
export function AccountsOverview() {
    const accounts = useAccounts();
    const connections = useAccountConnections();
    const institutions = useInstitutions();

    if (accounts.isLoading || connections.isLoading || institutions.isLoading)
        return <LoadingAccounts />;

    if (accounts.isError || connections.isError || institutions.isError) {
        const retry = () => {
            void Promise.all([
                accounts.refetch(),
                connections.refetch(),
                institutions.refetch(),
            ]);
        };
        return (
            <DashboardErrorState
                title="Accounts unavailable"
                description="FinSight could not retrieve your connected accounts and institutions. Please try again."
                onRetry={retry}
            />
        );
    }

    const accountData = accounts.data ?? [];
    const connectionData = connections.data ?? [];
    const institutionData = institutions.data ?? [];
    const institutionNameById = new Map(
        institutionData.map((institution) => [
            institution.id,
            institution.name,
        ]),
    );
    const balancesByCurrency = [
        ...new Set(accountData.map((account) => account.currency)),
    ].map((currency) => ({
        currency,
        balance: accountData
            .filter((account) => account.currency === currency)
            .reduce((sum, account) => sum + account.currentBalance, 0),
        count: accountData.filter((account) => account.currency === currency)
            .length,
    }));

    return (
        <div className="space-y-8">
            <section className="flex flex-col justify-between gap-5 lg:flex-row lg:items-end">
                <div className="max-w-2xl space-y-2">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                        Financial accounts
                    </p>
                    <h2 className="text-2xl font-semibold tracking-tight sm:text-3xl">
                        Your connected money, in one place.
                    </h2>
                    <p className="text-sm leading-6 text-muted-foreground sm:text-base">
                        Manage connected institutions, review account balances,
                        and keep FinSight’s financial data synchronized.
                    </p>
                </div>
                <ConnectAccountDialog />
            </section>

            <section
                className="grid gap-4 md:grid-cols-3"
                aria-label="Account summary"
            >
                <Card>
                    <CardHeader className="pb-2">
                        <CardDescription>Total accounts</CardDescription>
                    </CardHeader>
                    <CardContent className="flex items-end justify-between gap-4">
                        <p className="text-2xl font-semibold tabular-nums">
                            {accountData.length}
                        </p>
                        <WalletCards
                            className="size-5 text-muted-foreground"
                            aria-hidden="true"
                        />
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="pb-2">
                        <CardDescription>Institutions</CardDescription>
                    </CardHeader>
                    <CardContent className="flex items-end justify-between gap-4">
                        <p className="text-2xl font-semibold tabular-nums">
                            {connectionData.length}
                        </p>
                        <Landmark
                            className="size-5 text-muted-foreground"
                            aria-hidden="true"
                        />
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="pb-2">
                        <CardDescription>Connection health</CardDescription>
                    </CardHeader>
                    <CardContent className="flex items-end justify-between gap-4">
                        <p className="text-lg font-semibold">
                            {connectionData.length
                                ? formatStatusCount(connectionData)
                                : "No connections"}
                        </p>
                        <RefreshCw
                            className="size-5 text-muted-foreground"
                            aria-hidden="true"
                        />
                    </CardContent>
                </Card>
            </section>

            {balancesByCurrency.length ? (
                <section
                    className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3"
                    aria-label="Balances by currency"
                >
                    {balancesByCurrency.map((item) => (
                        <Card key={item.currency} size="sm">
                            <CardContent className="flex items-center justify-between gap-4 py-3">
                                <div>
                                    <p className="text-xs font-medium text-muted-foreground">
                                        {item.currency}
                                    </p>
                                    <p className="mt-1 text-lg font-semibold tabular-nums">
                                        {formatCurrency(
                                            item.balance,
                                            item.currency,
                                        )}
                                    </p>
                                </div>
                                <p className="text-xs text-muted-foreground">
                                    {item.count}{" "}
                                    {item.count === 1 ? "account" : "accounts"}
                                </p>
                            </CardContent>
                        </Card>
                    ))}
                </section>
            ) : null}

            <section className="space-y-4" aria-labelledby="accounts-heading">
                <div className="flex items-end justify-between gap-4">
                    <div>
                        <h3
                            id="accounts-heading"
                            className="text-lg font-semibold tracking-tight"
                        >
                            Accounts
                        </h3>
                        <p className="mt-1 text-sm text-muted-foreground">
                            Balances returned by your connected institutions.
                        </p>
                    </div>
                </div>

                {accountData.length === 0 ? (
                    <Card className="border-dashed">
                        <CardContent className="flex min-h-56 flex-col items-center justify-center px-6 py-10 text-center">
                            <div className="flex size-12 items-center justify-center rounded-xl bg-muted">
                                <WalletCards
                                    className="size-5"
                                    aria-hidden="true"
                                />
                            </div>
                            <h4 className="mt-4 font-semibold">
                                No accounts connected yet
                            </h4>
                            <p className="mt-1 max-w-md text-sm leading-6 text-muted-foreground">
                                Connect the FinSight Demo Bank to import demo
                                accounts and transactions. The initial
                                synchronization runs automatically.
                            </p>
                            <div className="mt-5">
                                <ConnectAccountDialog />
                            </div>
                        </CardContent>
                    </Card>
                ) : (
                    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
                        {accountData.map((account) => (
                            <AccountCard
                                key={account.id}
                                account={account}
                                institutionName={
                                    institutionNameById.get(
                                        account.institutionId,
                                    ) ?? "Financial institution"
                                }
                            />
                        ))}
                    </div>
                )}
            </section>

            <section
                className="space-y-4"
                aria-labelledby="connections-heading"
            >
                <div>
                    <h3
                        id="connections-heading"
                        className="text-lg font-semibold tracking-tight"
                    >
                        Connected institutions
                    </h3>
                    <p className="mt-1 text-sm text-muted-foreground">
                        Control synchronization and connection health for every
                        institution.
                    </p>
                </div>
                {connectionData.length === 0 ? (
                    <Card className="border-dashed">
                        <CardContent className="flex min-h-40 flex-col items-center justify-center px-6 py-10 text-center">
                            <Landmark
                                className="size-5 text-muted-foreground"
                                aria-hidden="true"
                            />
                            <p className="mt-3 font-medium">
                                No active connections
                            </p>
                            <p className="mt-1 max-w-md text-sm text-muted-foreground">
                                Connect an institution to start importing
                                financial activity into FinSight.
                            </p>
                        </CardContent>
                    </Card>
                ) : (
                    <div className="space-y-4">
                        {connectionData.map((connection) => (
                            <ConnectionCard
                                key={connection.id}
                                connection={connection}
                            />
                        ))}
                    </div>
                )}
            </section>

            {accountData.length > 0 ? (
                <Card className="bg-muted/20">
                    <CardContent className="flex flex-col gap-3 p-5 sm:flex-row sm:items-center sm:justify-between">
                        <div>
                            <p className="font-medium">
                                Ready to review the underlying activity?
                            </p>
                            <p className="mt-1 text-sm text-muted-foreground">
                                Explore transactions for a specific account and
                                review the intelligence generated from them.
                            </p>
                        </div>
                        <Button asChild variant="outline">
                            <Link href="/transactions">
                                View transactions{" "}
                                <ArrowRight aria-hidden="true" />
                            </Link>
                        </Button>
                    </CardContent>
                </Card>
            ) : null}
        </div>
    );
}
