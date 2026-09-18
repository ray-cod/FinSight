"use client";

import * as React from "react";
import { CheckCircle2, Mail, Save, UserRound } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { z } from "zod";

import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { getAuthErrorMessage, useAuth } from "@/lib/auth/auth-context";
import { formatDate } from "@/lib/formatting/date";
import { useAppToast } from "@/components/feedback/use-app-toast";
import { profileSchema } from "@/lib/validation/auth";

type ProfileForm = z.infer<typeof profileSchema>;

/** Renders the authenticated user's profile and editable display name. */
export function ProfileSettingsPanel() {
    const { user, updateProfile } = useAuth();
    const [serverError, setServerError] = React.useState<string>();
    const [saved, setSaved] = React.useState(false);
    const toast = useAppToast();

    const form = useForm<ProfileForm>({
        resolver: zodResolver(profileSchema),
        defaultValues: { displayName: user?.displayName ?? "" },
    });

    React.useEffect(() => {
        if (user) form.reset({ displayName: user.displayName });
    }, [form, user]);

    React.useEffect(() => {
        if (!form.formState.isDirty) return;
        const handleBeforeUnload = (event: BeforeUnloadEvent) => {
            event.preventDefault();
            event.returnValue = "";
        };
        window.addEventListener("beforeunload", handleBeforeUnload);
        return () =>
            window.removeEventListener("beforeunload", handleBeforeUnload);
    }, [form.formState.isDirty]);

    if (!user) {
        return (
            <Card>
                <CardHeader>
                    <Skeleton className="h-6 w-40" />
                    <Skeleton className="h-4 w-64" />
                </CardHeader>
                <CardContent>
                    <Skeleton className="h-24 w-full" />
                </CardContent>
            </Card>
        );
    }

    const onSubmit = form.handleSubmit(async (values) => {
        setServerError(undefined);
        setSaved(false);
        try {
            await updateProfile({ displayName: values.displayName.trim() });
            setSaved(true);
            toast.success({
                title: "Profile saved",
                description: "Your display name has been updated.",
            });
        } catch (error) {
            setServerError(getAuthErrorMessage(error));
            toast.error({
                title: "Profile could not be saved",
                description: getAuthErrorMessage(error),
            });
        }
    });

    return (
        <div className="space-y-6">
            <Card>
                <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                        <UserRound className="size-5" aria-hidden="true" />{" "}
                        Profile
                    </CardTitle>
                    <CardDescription>
                        Keep the identity information FinSight uses across your
                        financial workspace up to date.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-6">
                    {serverError ? (
                        <Alert variant="destructive">
                            <AlertDescription>{serverError}</AlertDescription>
                        </Alert>
                    ) : null}
                    {saved ? (
                        <Alert>
                            <CheckCircle2 aria-hidden="true" />
                            <AlertDescription>
                                Profile updated successfully.
                            </AlertDescription>
                        </Alert>
                    ) : null}

                    <form onSubmit={onSubmit} className="space-y-5" noValidate>
                        <div className="grid gap-5 md:grid-cols-2">
                            <div className="space-y-2">
                                <Label htmlFor="profile-display-name">
                                    Display name
                                </Label>
                                <Input
                                    id="profile-display-name"
                                    autoComplete="name"
                                    maxLength={100}
                                    aria-invalid={Boolean(
                                        form.formState.errors.displayName,
                                    )}
                                    {...form.register("displayName")}
                                />
                                {form.formState.errors.displayName ? (
                                    <p className="text-xs text-destructive">
                                        {
                                            form.formState.errors.displayName
                                                .message
                                        }
                                    </p>
                                ) : null}
                                <p className="text-xs text-muted-foreground">
                                    Shown in your FinSight workspace and account
                                    menu.
                                </p>
                            </div>

                            <div className="space-y-2">
                                <Label htmlFor="profile-email">
                                    Email address
                                </Label>
                                <div className="relative">
                                    <Mail
                                        className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
                                        aria-hidden="true"
                                    />
                                    <Input
                                        id="profile-email"
                                        value={user.email}
                                        readOnly
                                        className="pl-9"
                                    />
                                </div>
                                <div className="flex items-center gap-2 text-xs text-muted-foreground">
                                    <Badge
                                        variant={
                                            user.emailConfirmed
                                                ? "secondary"
                                                : "outline"
                                        }
                                    >
                                        {user.emailConfirmed
                                            ? "Verified"
                                            : "Not verified"}
                                    </Badge>
                                    <span>
                                        Email changes require a separate
                                        account-verification flow.
                                    </span>
                                </div>
                            </div>
                        </div>

                        <div className="grid gap-5 border-t pt-5 sm:grid-cols-2">
                            <div>
                                <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                                    Account created
                                </p>
                                <p className="mt-1 text-sm font-medium">
                                    {formatDate(user.createdAt)}
                                </p>
                            </div>
                            <div>
                                <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                                    Email verification
                                </p>
                                <p className="mt-1 text-sm font-medium">
                                    {user.emailConfirmed
                                        ? "Verified"
                                        : "Not verified"}
                                </p>
                            </div>
                        </div>

                        <div className="flex justify-end border-t pt-5">
                            <Button
                                type="submit"
                                disabled={
                                    form.formState.isSubmitting ||
                                    !form.formState.isDirty
                                }
                            >
                                <Save aria-hidden="true" />
                                {form.formState.isSubmitting
                                    ? "Saving…"
                                    : "Save profile"}
                            </Button>
                        </div>
                    </form>
                </CardContent>
            </Card>
        </div>
    );
}
