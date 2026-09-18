"use client";

import * as React from "react";
import { RefreshCw, Unplug } from "lucide-react";

import { AccountConnectionStatus } from "@/components/accounts/account-status";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useDisconnectAccount, useSyncAccount } from "@/lib/accounts/use-accounts";
import type { AccountConnection } from "@/lib/api/types";
import { formatDate } from "@/lib/formatting/date";
import { useAppToast } from "@/components/feedback/use-app-toast";

/** Renders one institution connection with synchronization and disconnect controls. */
export function ConnectionCard({ connection }: { readonly connection: AccountConnection }) {
  const sync = useSyncAccount();
  const disconnect = useDisconnectAccount();
  const [confirmOpen, setConfirmOpen] = React.useState(false);
  const toast = useAppToast();

  const isSyncing = sync.isPending || connection.status === "Syncing";
  const accountLabel = connection.accountCount === 1 ? "1 account" : `${connection.accountCount} accounts`;
  const lastSync = connection.lastSuccessfulSyncAt ? formatDate(connection.lastSuccessfulSyncAt) : "Never synced";

  const handleDisconnect = async () => {
    try {
      await disconnect.mutateAsync(connection.id);
      toast.success({ title: "Connection disconnected", description: `${connection.institutionName} will no longer sync new activity.` });
      setConfirmOpen(false);
    } catch (error) {
      toast.error({ title: "Connection could not be disconnected", description: error instanceof Error ? error.message : "Please try again." });
    }
  };

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex items-start justify-between gap-4">
            <div className="min-w-0">
              <div className="flex flex-wrap items-center gap-2">
                <CardTitle className="truncate">{connection.institutionName}</CardTitle>
                <AccountConnectionStatus status={connection.status} />
              </div>
              <CardDescription className="mt-1">{connection.providerCode} · {accountLabel}</CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-2">
            <div className="rounded-lg border bg-muted/20 p-3">
              <p className="text-xs text-muted-foreground">Last successful sync</p>
              <p className="mt-1 text-sm font-medium">{lastSync}</p>
            </div>
            <div className="rounded-lg border bg-muted/20 p-3">
              <p className="text-xs text-muted-foreground">Accounts discovered</p>
              <p className="mt-1 text-sm font-medium">{accountLabel}</p>
            </div>
          </div>

          {connection.status === "Failed" && connection.lastSyncError ? (
            <div role="alert" className="rounded-lg border border-destructive/20 bg-destructive/5 p-3 text-sm text-destructive">
              {connection.lastSyncError}
            </div>
          ) : null}

          {connection.status === "ReauthorizationRequired" ? (
            <div className="rounded-lg border border-amber-500/20 bg-amber-500/5 p-3 text-sm text-muted-foreground">
              This connection requires reauthorization before FinSight can synchronize it again.
            </div>
          ) : null}

          <div className="flex flex-wrap items-center justify-between gap-3 border-t pt-4">
            <Badge variant="outline">Connection ID {connection.id.slice(0, 8)}…</Badge>
            <div className="flex flex-wrap gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={isSyncing || connection.status === "ReauthorizationRequired"}
                onClick={() => sync.mutate(connection.id, {
                  onSuccess: (result) => toast.success({ title: "Account synchronized", description: `${result.importedTransactions} transaction${result.importedTransactions === 1 ? "" : "s"} imported.` }),
                  onError: (error) => toast.error({ title: "Synchronization failed", description: error instanceof Error ? error.message : "Please try again." }),
                })}
              >
                <RefreshCw className={isSyncing ? "animate-spin" : ""} aria-hidden="true" />
                {isSyncing ? "Syncing…" : "Sync now"}
              </Button>
              <Button variant="destructive" size="sm" onClick={() => setConfirmOpen(true)} disabled={isSyncing || disconnect.isPending}>
                <Unplug aria-hidden="true" />
                Disconnect
              </Button>
            </div>
          </div>
          {sync.isError ? <p role="alert" className="text-sm text-destructive">{sync.error instanceof Error ? sync.error.message : "Synchronization failed."}</p> : null}
          {disconnect.isError ? <p role="alert" className="text-sm text-destructive">{disconnect.error instanceof Error ? disconnect.error.message : "The connection could not be disconnected."}</p> : null}
        </CardContent>
      </Card>

      <Dialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Disconnect {connection.institutionName}?</DialogTitle>
            <DialogDescription>
              FinSight will stop synchronizing this institution. The existing financial records remain in your FinSight history, but new transactions will no longer be imported from this connection.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmOpen(false)} disabled={disconnect.isPending}>Cancel</Button>
            <Button variant="destructive" onClick={() => void handleDisconnect()} disabled={disconnect.isPending}>
              {disconnect.isPending ? "Disconnecting…" : "Disconnect"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
