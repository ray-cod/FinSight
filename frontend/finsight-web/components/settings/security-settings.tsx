"use client";

import * as React from "react";
import Link from "next/link";
import { CheckCircle2, KeyRound, ShieldCheck } from "lucide-react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { z } from "zod";

import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { PasswordField } from "@/components/forms/password-field";
import { getAuthErrorMessage, useAuth } from "@/lib/auth/auth-context";
import { changePasswordSchema } from "@/lib/validation/auth";
import { SessionManagementPanel } from "@/components/settings/session-management";
import { SecurityActivityPanel } from "@/components/settings/security-activity";

type ChangePasswordForm = z.infer<typeof changePasswordSchema>;

/** Renders password, session and security-activity controls for the authenticated user. */
export function SecuritySettingsPanel() {
  const { changePassword } = useAuth();
  const router = useRouter();
  const [serverError, setServerError] = React.useState<string>();
  const [success, setSuccess] = React.useState(false);

  const form = useForm<ChangePasswordForm>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: { currentPassword: "", newPassword: "", confirmPassword: "" },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    setServerError(undefined);
    setSuccess(false);
    try {
      await changePassword({ currentPassword: values.currentPassword, newPassword: values.newPassword });
      setSuccess(true);
      form.reset();
      router.replace("/login?reason=password-changed");
    } catch (error) {
      setServerError(getAuthErrorMessage(error));
    }
  });

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2"><KeyRound className="size-5" aria-hidden="true" /> Password</CardTitle>
          <CardDescription>Change your password using the same security policy enforced by the FinSight Identity backend.</CardDescription>
        </CardHeader>
        <CardContent>
          {success ? <Alert className="mb-5"><CheckCircle2 aria-hidden="true" /><AlertDescription>Password changed. Your previous sessions were revoked and you will need to sign in again.</AlertDescription></Alert> : null}
          {serverError ? <Alert variant="destructive" className="mb-5"><AlertDescription>{serverError}</AlertDescription></Alert> : null}

          <form className="space-y-5" onSubmit={onSubmit} noValidate>
            <div className="space-y-2">
              <Label htmlFor="current-password">Current password</Label>
              <PasswordField id="current-password" autoComplete="current-password" aria-invalid={Boolean(form.formState.errors.currentPassword)} {...form.register("currentPassword")} />
              {form.formState.errors.currentPassword ? <p className="text-xs text-destructive">{form.formState.errors.currentPassword.message}</p> : null}
            </div>
            <div className="grid gap-5 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="new-password">New password</Label>
                <PasswordField id="new-password" autoComplete="new-password" aria-invalid={Boolean(form.formState.errors.newPassword)} {...form.register("newPassword")} />
                {form.formState.errors.newPassword ? <p className="text-xs text-destructive">{form.formState.errors.newPassword.message}</p> : null}
              </div>
              <div className="space-y-2">
                <Label htmlFor="confirm-password">Confirm new password</Label>
                <PasswordField id="confirm-password" autoComplete="new-password" aria-invalid={Boolean(form.formState.errors.confirmPassword)} {...form.register("confirmPassword")} />
                {form.formState.errors.confirmPassword ? <p className="text-xs text-destructive">{form.formState.errors.confirmPassword.message}</p> : null}
              </div>
            </div>

            <div className="rounded-xl border border-dashed bg-muted/20 p-4 text-sm text-muted-foreground">
              <div className="flex gap-3">
                <ShieldCheck className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
                <div>
                  <p className="font-medium text-foreground">Password requirements</p>
                  <p className="mt-1 leading-5">Use at least 12 characters with uppercase, lowercase, a number and a special character. Your new password must differ from your current password.</p>
                </div>
              </div>
            </div>

            <div className="flex flex-col gap-3 border-t pt-5 sm:flex-row sm:items-center sm:justify-between">
              <Link href="/forgot-password" className="text-sm font-medium text-muted-foreground underline-offset-4 hover:text-foreground hover:underline">Forgot your password?</Link>
              <Button type="submit" disabled={form.formState.isSubmitting}>
                <KeyRound aria-hidden="true" />
                {form.formState.isSubmitting ? "Changing password…" : "Change password"}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      <SessionManagementPanel />
      <SecurityActivityPanel />
    </div>
  );
}
