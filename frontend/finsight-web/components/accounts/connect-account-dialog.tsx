"use client";

import * as React from "react";
import {
    Building2,
    CheckCircle2,
    Landmark,
    Loader2,
    ShieldCheck,
    Sparkles,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { useAppToast } from "@/components/feedback/use-app-toast";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import {
    useConnectAccount,
    useInstitutions,
} from "@/lib/accounts/use-accounts";

/** Provides the financial institution connection workflow for the mock banking environment. */
export function ConnectAccountDialog() {
    const institutions = useInstitutions();
    const connect = useConnectAccount();
    const [open, setOpen] = React.useState(false);
    const [selectedCode, setSelectedCode] = React.useState("");
    const toast = useAppToast();

    const handleOpenChange = (nextOpen: boolean) => {
        setOpen(nextOpen);
        if (!nextOpen) {
            setSelectedCode("");
            connect.reset();
        }
    };

    const selectedInstitution = institutions.data?.find(
        (institution) => institution.providerCode === selectedCode,
    );

    const handleConnect = async () => {
        if (!selectedCode) return;
        try {
            const result = await connect.mutateAsync(selectedCode);
            toast.success({
                title: "Account connected",
                description: `${result.importedTransactions} transaction${result.importedTransactions === 1 ? "" : "s"} imported during initial sync.`,
            });
            setOpen(false);
        } catch (error) {
            toast.error({
                title: "Account could not be connected",
                description:
                    error instanceof Error
                        ? error.message
                        : "Please try again.",
            });
        }
    };

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogTrigger render={<Button size="lg" />}>
                <Landmark aria-hidden="true" />
                Connect an account
            </DialogTrigger>
            <DialogContent className="sm:max-w-lg">
                <DialogHeader>
                    <DialogTitle>Connect a financial institution</DialogTitle>
                    <DialogDescription>
                        Choose a supported institution to connect its demo
                        accounts. FinSight will run an initial synchronization
                        immediately after the connection is created.
                    </DialogDescription>
                </DialogHeader>

                {connect.isSuccess ? (
                    <div className="rounded-xl border border-emerald-500/20 bg-emerald-500/5 p-5">
                        <div className="flex items-start gap-3">
                            <CheckCircle2
                                className="mt-0.5 size-5 text-emerald-700 dark:text-emerald-400"
                                aria-hidden="true"
                            />
                            <div>
                                <p className="font-medium">Account connected</p>
                                <p className="mt-1 text-sm text-muted-foreground">
                                    FinSight imported{" "}
                                    {connect.data.importedTransactions} new
                                    transaction
                                    {connect.data.importedTransactions === 1
                                        ? ""
                                        : "s"}{" "}
                                    during the initial sync.
                                </p>
                            </div>
                        </div>
                    </div>
                ) : (
                    <div className="space-y-5">
                        <div className="grid gap-2 sm:grid-cols-3">
                            {[
                                {
                                    icon: ShieldCheck,
                                    title: "Secure",
                                    description:
                                        "Protected by your FinSight session.",
                                },
                                {
                                    icon: Sparkles,
                                    title: "Automatic",
                                    description:
                                        "Transactions sync for intelligence.",
                                },
                                {
                                    icon: Building2,
                                    title: "Demo data",
                                    description:
                                        "Uses the built-in mock provider.",
                                },
                            ].map(({ icon: Icon, title, description }) => (
                                <div
                                    key={title}
                                    className="rounded-lg border bg-muted/20 p-3"
                                >
                                    <Icon
                                        className="size-4 text-muted-foreground"
                                        aria-hidden="true"
                                    />
                                    <p className="mt-2 text-sm font-medium">
                                        {title}
                                    </p>
                                    <p className="mt-1 text-xs leading-5 text-muted-foreground">
                                        {description}
                                    </p>
                                </div>
                            ))}
                        </div>

                        {institutions.isLoading ? (
                            <div className="space-y-2" aria-busy="true">
                                <div className="h-10 animate-pulse rounded-lg bg-muted" />
                                <div className="h-10 animate-pulse rounded-lg bg-muted" />
                            </div>
                        ) : institutions.isError ? (
                            <p className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
                                FinSight could not load supported institutions.
                                Please close this dialog and try again.
                            </p>
                        ) : institutions.data?.length ? (
                            <div className="space-y-2">
                                <label
                                    htmlFor="institution"
                                    className="text-sm font-medium"
                                >
                                    Institution
                                </label>
                                <select
                                    id="institution"
                                    value={selectedCode}
                                    onChange={(event) =>
                                        setSelectedCode(event.target.value)
                                    }
                                    className="h-10 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30"
                                    disabled={connect.isPending}
                                >
                                    <option value="">
                                        Select an institution
                                    </option>
                                    {institutions.data.map((institution) => (
                                        <option
                                            key={institution.id}
                                            value={institution.providerCode}
                                        >
                                            {institution.name}
                                        </option>
                                    ))}
                                </select>
                                {selectedInstitution ? (
                                    <p className="text-xs text-muted-foreground">
                                        Provider:{" "}
                                        {selectedInstitution.providerCode}
                                    </p>
                                ) : null}
                            </div>
                        ) : (
                            <p className="rounded-lg border border-dashed p-4 text-sm text-muted-foreground">
                                There are no active institutions available to
                                connect right now.
                            </p>
                        )}

                        {connect.isError ? (
                            <p
                                role="alert"
                                className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive"
                            >
                                {connect.error instanceof Error
                                    ? connect.error.message
                                    : "The account could not be connected. Please try again."}
                            </p>
                        ) : null}
                    </div>
                )}

                <DialogFooter>
                    <Button
                        variant="outline"
                        onClick={() => setOpen(false)}
                        disabled={connect.isPending}
                    >
                        {connect.isSuccess ? "Done" : "Cancel"}
                    </Button>
                    {!connect.isSuccess ? (
                        <Button
                            onClick={() => void handleConnect()}
                            disabled={
                                !selectedCode ||
                                connect.isPending ||
                                !institutions.data?.length
                            }
                        >
                            {connect.isPending ? (
                                <>
                                    <Loader2
                                        className="animate-spin"
                                        aria-hidden="true"
                                    />{" "}
                                    Connecting…
                                </>
                            ) : (
                                "Connect & sync"
                            )}
                        </Button>
                    ) : null}
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
