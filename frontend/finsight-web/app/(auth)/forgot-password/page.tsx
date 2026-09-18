"use client";

import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowLeft, CheckCircle2, Loader2 } from "lucide-react";
import type { z } from "zod";

import { AuthPage } from "@/components/auth/auth-page";
import { FormMessage } from "@/components/auth/form-message";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getAuthErrorMessage, useAuth } from "@/lib/auth/auth-context";
import { forgotPasswordSchema } from "@/lib/validation/auth";

type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>;

export default function ForgotPasswordPage() {
  const { forgotPassword } = useAuth();
  const [serverError, setServerError] = useState<string>();
  const [submitted, setSubmitted] = useState(false);

  const form = useForm<ForgotPasswordForm>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: "" },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    setServerError(undefined);
    try {
      await forgotPassword(values);
      setSubmitted(true);
    } catch (error) {
      setServerError(getAuthErrorMessage(error));
    }
  });

  return (
    <AuthPage
      eyebrow="Account recovery"
      title="Reset your password"
      description="Enter your account email and FinSight will guide you through the password reset process."
      footer={
        <p className="text-center">
          <Link href="/login" className="inline-flex items-center gap-2 font-medium text-foreground underline underline-offset-4">
            <ArrowLeft className="size-3.5" aria-hidden="true" />
            Back to sign in
          </Link>
        </p>
      }
    >
      {submitted ? (
        <div className="space-y-5 rounded-2xl border bg-card p-6 text-center shadow-sm">
          <div className="mx-auto flex size-12 items-center justify-center rounded-full bg-muted">
            <CheckCircle2 className="size-6" aria-hidden="true" />
          </div>
          <div className="space-y-2">
            <h2 className="font-semibold">Check your email</h2>
            <p className="text-sm leading-6 text-muted-foreground">
              If an account matches that address, a password-reset message will be sent. For security, FinSight does not reveal whether the account exists.
            </p>
          </div>
          <Button asChild variant="outline" className="w-full">
            <Link href="/login">Return to sign in</Link>
          </Button>
        </div>
      ) : (
        <form className="space-y-5" onSubmit={onSubmit} noValidate>
          <FormMessage message={serverError} />
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
            {form.formState.errors.email ? <p className="text-xs text-destructive">{form.formState.errors.email.message}</p> : null}
          </div>
          <Button type="submit" className="w-full" size="lg" disabled={form.formState.isSubmitting}>
            {form.formState.isSubmitting ? <Loader2 className="animate-spin" aria-hidden="true" /> : null}
            {form.formState.isSubmitting ? "Sending…" : "Send reset instructions"}
          </Button>
        </form>
      )}
    </AuthPage>
  );
}
