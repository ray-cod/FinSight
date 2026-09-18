"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { Suspense, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { CheckCircle2, Loader2 } from "lucide-react";
import type { z } from "zod";

import { AuthPage } from "@/components/auth/auth-page";
import { FormMessage } from "@/components/auth/form-message";
import { PasswordField } from "@/components/forms/password-field";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getAuthErrorMessage, useAuth } from "@/lib/auth/auth-context";
import { resetPasswordSchema } from "@/lib/validation/auth";

type ResetPasswordForm = z.infer<typeof resetPasswordSchema>;

function ResetPasswordForm() {
    const { resetPassword } = useAuth();
    const searchParams = useSearchParams();
    const [serverError, setServerError] = useState<string>();
    const [completed, setCompleted] = useState(false);

    const form = useForm<ResetPasswordForm>({
        resolver: zodResolver(resetPasswordSchema),
        defaultValues: {
            email: searchParams.get("email") ?? "",
            token: searchParams.get("token") ?? "",
            newPassword: "",
            confirmPassword: "",
        },
    });

    const onSubmit = form.handleSubmit(async (values) => {
        const { confirmPassword, ...safeValues } = values;
        void confirmPassword;
        setServerError(undefined);
        try {
            await resetPassword(safeValues);
            setCompleted(true);
        } catch (error) {
            setServerError(getAuthErrorMessage(error));
        }
    });

    if (completed) {
        return (
            <div className="space-y-5 rounded-2xl border bg-card p-6 text-center shadow-sm">
                <div className="mx-auto flex size-12 items-center justify-center rounded-full bg-muted">
                    <CheckCircle2 className="size-6" aria-hidden="true" />
                </div>
                <div className="space-y-2">
                    <h2 className="font-semibold">Password updated</h2>
                    <p className="text-sm leading-6 text-muted-foreground">
                        Your password has been reset. You can now sign in with
                        your new password.
                    </p>
                </div>
                <Button asChild className="w-full">
                    <Link href="/login">Sign in</Link>
                </Button>
            </div>
        );
    }

    return (
        <form className="space-y-5" onSubmit={onSubmit} noValidate>
            <FormMessage message={serverError} />
            <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input
                    id="email"
                    type="email"
                    autoComplete="email"
                    aria-invalid={Boolean(form.formState.errors.email)}
                    {...form.register("email")}
                />
                {form.formState.errors.email ? (
                    <p className="text-xs text-destructive">
                        {form.formState.errors.email.message}
                    </p>
                ) : null}
            </div>
            <div className="space-y-2">
                <Label htmlFor="token">Reset token</Label>
                <Input
                    id="token"
                    autoComplete="off"
                    aria-invalid={Boolean(form.formState.errors.token)}
                    {...form.register("token")}
                />
                {form.formState.errors.token ? (
                    <p className="text-xs text-destructive">
                        {form.formState.errors.token.message}
                    </p>
                ) : null}
                <p className="text-xs leading-5 text-muted-foreground">
                    Paste the reset token from your FinSight password-reset
                    message.
                </p>
            </div>
            <div className="space-y-2">
                <Label htmlFor="newPassword">New password</Label>
                <PasswordField
                    id="newPassword"
                    autoComplete="new-password"
                    aria-invalid={Boolean(form.formState.errors.newPassword)}
                    {...form.register("newPassword")}
                />
                {form.formState.errors.newPassword ? (
                    <p className="text-xs text-destructive">
                        {form.formState.errors.newPassword.message}
                    </p>
                ) : null}
            </div>
            <div className="space-y-2">
                <Label htmlFor="confirmPassword">Confirm new password</Label>
                <PasswordField
                    id="confirmPassword"
                    autoComplete="new-password"
                    aria-invalid={Boolean(
                        form.formState.errors.confirmPassword,
                    )}
                    {...form.register("confirmPassword")}
                />
                {form.formState.errors.confirmPassword ? (
                    <p className="text-xs text-destructive">
                        {form.formState.errors.confirmPassword.message}
                    </p>
                ) : null}
            </div>
            <Button
                type="submit"
                className="w-full"
                size="lg"
                disabled={form.formState.isSubmitting}
            >
                {form.formState.isSubmitting ? (
                    <Loader2 className="animate-spin" aria-hidden="true" />
                ) : null}
                {form.formState.isSubmitting ? "Resetting…" : "Reset password"}
            </Button>
        </form>
    );
}

export default function ResetPasswordPage() {
    return (
        <Suspense fallback={null}>
            <AuthPage
                eyebrow="Account recovery"
                title="Choose a new password"
                description="Use the reset token from your FinSight message to set a new password."
                footer={
                    <p className="text-center">
                        <Link
                            href="/login"
                            className="font-medium text-foreground underline underline-offset-4"
                        >
                            Back to sign in
                        </Link>
                    </p>
                }
            >
                <ResetPasswordForm />
            </AuthPage>
        </Suspense>
    );
}
