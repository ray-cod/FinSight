"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Check, Loader2 } from "lucide-react";
import type { z } from "zod";

import { AuthPage } from "@/components/auth/auth-page";
import { FormMessage } from "@/components/auth/form-message";
import { PasswordField } from "@/components/forms/password-field";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getAuthErrorMessage, useAuth } from "@/lib/auth/auth-context";
import { registerSchema } from "@/lib/validation/auth";

type RegisterForm = z.infer<typeof registerSchema>;

export default function RegisterPage() {
    const { register } = useAuth();
    const router = useRouter();
    const [serverError, setServerError] = useState<string>();

    const form = useForm<RegisterForm>({
        resolver: zodResolver(registerSchema),
        defaultValues: {
            displayName: "",
            email: "",
            password: "",
            confirmPassword: "",
        },
    });

    const onSubmit = form.handleSubmit(async (values) => {
        const { confirmPassword, ...safeValues } = values;
        void confirmPassword;
        setServerError(undefined);
        try {
            await register(safeValues);
            router.replace("/dashboard");
        } catch (error) {
            setServerError(getAuthErrorMessage(error));
        }
    });

    return (
        <AuthPage
            eyebrow="Create your account"
            title="Start with a clearer financial picture"
            description="Create your FinSight workspace. You can connect the mock banking environment once you're signed in."
            footer={
                <p className="text-center">
                    Already have an account?{" "}
                    <Link
                        href="/login"
                        className="font-medium text-foreground underline underline-offset-4"
                    >
                        Sign in
                    </Link>
                </p>
            }
        >
            <form className="space-y-5" onSubmit={onSubmit} noValidate>
                <FormMessage message={serverError} />

                <div className="space-y-2">
                    <Label htmlFor="displayName">Name</Label>
                    <Input
                        id="displayName"
                        autoComplete="name"
                        placeholder="Alex Morgan"
                        aria-invalid={Boolean(
                            form.formState.errors.displayName,
                        )}
                        {...form.register("displayName")}
                    />
                    {form.formState.errors.displayName ? (
                        <p className="text-xs text-destructive">
                            {form.formState.errors.displayName.message}
                        </p>
                    ) : null}
                </div>

                <div className="space-y-2">
                    <Label htmlFor="email">Email</Label>
                    <Input
                        id="email"
                        type="email"
                        autoComplete="email"
                        placeholder="you@example.com"
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
                    <Label htmlFor="password">Password</Label>
                    <PasswordField
                        id="password"
                        autoComplete="new-password"
                        aria-invalid={Boolean(form.formState.errors.password)}
                        {...form.register("password")}
                    />
                    {form.formState.errors.password ? (
                        <p className="text-xs text-destructive">
                            {form.formState.errors.password.message}
                        </p>
                    ) : null}
                    <div className="rounded-lg border bg-muted/30 px-3 py-2 text-xs leading-5 text-muted-foreground">
                        <p className="mb-1 font-medium text-foreground">
                            Password requirements
                        </p>
                        <p className="flex items-start gap-2">
                            <Check
                                className="mt-0.5 size-3.5 shrink-0"
                                aria-hidden="true"
                            />
                            12 or more characters
                        </p>
                        <p className="flex items-start gap-2">
                            <Check
                                className="mt-0.5 size-3.5 shrink-0"
                                aria-hidden="true"
                            />
                            Uppercase, lowercase and a number
                        </p>
                        <p className="flex items-start gap-2">
                            <Check
                                className="mt-0.5 size-3.5 shrink-0"
                                aria-hidden="true"
                            />
                            At least one special character
                        </p>
                    </div>
                </div>

                <div className="space-y-2">
                    <Label htmlFor="confirmPassword">Confirm password</Label>
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
                    {form.formState.isSubmitting
                        ? "Creating account…"
                        : "Create account"}
                </Button>
            </form>
        </AuthPage>
    );
}
