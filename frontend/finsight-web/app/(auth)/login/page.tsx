"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import type { z } from "zod";

import { AuthPage } from "@/components/auth/auth-page";
import { FormMessage } from "@/components/auth/form-message";
import { PasswordField } from "@/components/forms/password-field";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getAuthErrorMessage, useAuth } from "@/lib/auth/auth-context";
import { loginSchema } from "@/lib/validation/auth";

type LoginForm = z.infer<typeof loginSchema>;

function safeReturnUrl(value: string | null): string {
  if (!value || !value.startsWith("/") || value.startsWith("//")) return "/dashboard";
  return value;
}

function LoginForm() {
  const { login } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [serverError, setServerError] = useState<string>();

  const form = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    setServerError(undefined);
    try {
      await login(values);
      router.replace(safeReturnUrl(searchParams.get("returnUrl")));
    } catch (error) {
      setServerError(getAuthErrorMessage(error));
    }
  });

  const passwordChanged = searchParams.get("reason") === "password-changed";

  return (
    <form className="space-y-5" onSubmit={onSubmit} noValidate>
      {passwordChanged ? <p className="rounded-xl border bg-muted/40 p-3 text-sm text-muted-foreground">Your password was changed successfully. For your security, all previous sessions were signed out. Sign in again with your new password.</p> : null}
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

      <div className="space-y-2">
        <div className="flex items-center justify-between gap-3">
          <Label htmlFor="password">Password</Label>
          <Link href="/forgot-password" className="text-xs font-medium text-muted-foreground hover:text-foreground">
            Forgot password?
          </Link>
        </div>
        <PasswordField
          id="password"
          autoComplete="current-password"
          aria-invalid={Boolean(form.formState.errors.password)}
          {...form.register("password")}
        />
        {form.formState.errors.password ? <p className="text-xs text-destructive">{form.formState.errors.password.message}</p> : null}
      </div>

      <Button type="submit" className="w-full" size="lg" disabled={form.formState.isSubmitting}>
        {form.formState.isSubmitting ? <Loader2 className="animate-spin" aria-hidden="true" /> : null}
        {form.formState.isSubmitting ? "Signing in…" : "Sign in"}
      </Button>
    </form>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={null}>
      <AuthPage
        eyebrow="Welcome back"
        title="Sign in to FinSight"
        description="Access your financial workspace and keep an eye on subscriptions, anomalies and spending patterns."
        footer={
          <p className="text-center">
            New to FinSight?{" "}
            <Link href="/register" className="font-medium text-foreground underline underline-offset-4">
              Create an account
            </Link>
          </p>
        }
      >
        <LoginForm />
      </AuthPage>
    </Suspense>
  );
}
