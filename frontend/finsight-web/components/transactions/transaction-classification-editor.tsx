"use client";

import * as React from "react";
import { Check, Loader2, Pencil, Sparkles } from "lucide-react";
import { useAppToast } from "@/components/feedback/use-app-toast";

import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { useCategories } from "@/lib/categories/use-categories";
import { useCorrectTransactionClassification } from "@/lib/transactions/use-transactions";

interface Props {
    readonly transactionId: string;
    readonly categoryId: string | null;
    readonly subcategoryId: string | null;
}

/** Provides the auditable user correction workflow for transaction classification. */
export function TransactionClassificationEditor({
    transactionId,
    categoryId,
    subcategoryId,
}: Props) {
    const categories = useCategories();
    const correction = useCorrectTransactionClassification();
    const [open, setOpen] = React.useState(false);
    const [selectedCategoryId, setSelectedCategoryId] = React.useState(
        categoryId ?? "",
    );
    const [selectedSubcategoryId, setSelectedSubcategoryId] = React.useState(
        subcategoryId ?? "",
    );
    const toast = useAppToast();

    const handleOpenChange = (nextOpen: boolean) => {
        setOpen(nextOpen);
        if (nextOpen) {
            setSelectedCategoryId(categoryId ?? "");
            setSelectedSubcategoryId(subcategoryId ?? "");
            correction.reset();
        }
    };

    const selectedCategory = categories.data?.find(
        (category) => category.id === selectedCategoryId,
    );

    const save = async () => {
        if (!selectedCategoryId) return;
        try {
            await correction.mutateAsync({
                transactionId,
                categoryId: selectedCategoryId,
                subcategoryId: selectedSubcategoryId || null,
            });
            toast.success({
                title: "Classification updated",
                description:
                    "This transaction is now marked as user corrected.",
            });
            setOpen(false);
        } catch (error) {
            toast.error({
                title: "Classification could not be updated",
                description:
                    error instanceof Error
                        ? error.message
                        : "Please try again.",
            });
        }
    };

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogTrigger render={<Button variant="outline" />}>
                <Pencil aria-hidden="true" /> Edit classification
            </DialogTrigger>
            <DialogContent className="sm:max-w-lg">
                <DialogHeader>
                    <DialogTitle>
                        Correct transaction classification
                    </DialogTitle>
                    <DialogDescription>
                        Select the category FinSight should associate with this
                        transaction. The correction is saved as a user-provided
                        classification.
                    </DialogDescription>
                </DialogHeader>

                {categories.isLoading ? (
                    <div className="space-y-3" aria-busy="true">
                        <div className="h-10 animate-pulse rounded-lg bg-muted" />
                        <div className="h-10 animate-pulse rounded-lg bg-muted" />
                    </div>
                ) : categories.isError ? (
                    <p
                        role="alert"
                        className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive"
                    >
                        The classification taxonomy could not be loaded. Please
                        try again.
                    </p>
                ) : (
                    <div className="space-y-5">
                        <label className="space-y-1.5">
                            <span className="text-sm font-medium">
                                Category
                            </span>
                            <select
                                value={selectedCategoryId}
                                onChange={(event) => {
                                    setSelectedCategoryId(event.target.value);
                                    setSelectedSubcategoryId("");
                                }}
                                className="h-10 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30"
                            >
                                <option value="">Select a category</option>
                                {categories.data?.map((category) => (
                                    <option
                                        key={category.id}
                                        value={category.id}
                                    >
                                        {category.name}
                                    </option>
                                ))}
                            </select>
                        </label>
                        <label className="space-y-1.5">
                            <span className="text-sm font-medium">
                                Subcategory{" "}
                                <span className="font-normal text-muted-foreground">
                                    (optional)
                                </span>
                            </span>
                            <select
                                value={selectedSubcategoryId}
                                onChange={(event) =>
                                    setSelectedSubcategoryId(event.target.value)
                                }
                                disabled={
                                    !selectedCategory?.subcategories.length
                                }
                                className="h-10 w-full rounded-lg border border-input bg-background px-3 text-sm outline-none disabled:cursor-not-allowed disabled:opacity-60 focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/30"
                            >
                                <option value="">No subcategory</option>
                                {selectedCategory?.subcategories.map(
                                    (subcategory) => (
                                        <option
                                            key={subcategory.id}
                                            value={subcategory.id}
                                        >
                                            {subcategory.name}
                                        </option>
                                    ),
                                )}
                            </select>
                        </label>
                        {selectedCategory ? (
                            <div className="rounded-lg border bg-muted/20 p-3 text-sm">
                                <div className="flex items-start gap-2">
                                    <Sparkles
                                        className="mt-0.5 size-4 text-muted-foreground"
                                        aria-hidden="true"
                                    />
                                    <div>
                                        <p className="font-medium">
                                            This becomes the user classification
                                        </p>
                                        <p className="mt-1 text-muted-foreground">
                                            FinSight will mark the transaction
                                            as user corrected rather than
                                            machine classified.
                                        </p>
                                    </div>
                                </div>
                            </div>
                        ) : null}
                        {correction.isError ? (
                            <p
                                role="alert"
                                className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive"
                            >
                                {correction.error instanceof Error
                                    ? correction.error.message
                                    : "The classification could not be updated."}
                            </p>
                        ) : null}
                    </div>
                )}

                <DialogFooter>
                    <Button
                        variant="outline"
                        onClick={() => setOpen(false)}
                        disabled={correction.isPending}
                    >
                        Cancel
                    </Button>
                    <Button
                        onClick={() => void save()}
                        disabled={
                            !selectedCategoryId ||
                            correction.isPending ||
                            categories.isLoading
                        }
                    >
                        {correction.isPending ? (
                            <>
                                <Loader2
                                    className="animate-spin"
                                    aria-hidden="true"
                                />{" "}
                                Saving…
                            </>
                        ) : (
                            <>
                                <Check aria-hidden="true" /> Save correction
                            </>
                        )}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
