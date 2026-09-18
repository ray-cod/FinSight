import { z } from "zod";

export const passwordRequirements = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{12,}$/;

export const loginSchema = z.object({
  email: z.string().trim().email("Enter a valid email address."),
  password: z.string().min(1, "Enter your password."),
});

export const registerSchema = z
  .object({
    displayName: z.string().trim().min(2, "Use at least 2 characters for your name.").max(120),
    email: z.string().trim().email("Enter a valid email address."),
    password: z
      .string()
      .regex(
        passwordRequirements,
        "Use 12+ characters with uppercase, lowercase, a number and a special character.",
      ),
    confirmPassword: z.string().min(1, "Confirm your password."),
  })
  .refine((values) => values.password === values.confirmPassword, {
    path: ["confirmPassword"],
    message: "Passwords do not match.",
  });

export const forgotPasswordSchema = z.object({
  email: z.string().trim().email("Enter a valid email address."),
});

export const resetPasswordSchema = z
  .object({
    email: z.string().trim().email("Enter a valid email address."),
    token: z.string().min(1, "Enter the reset token from your email."),
    newPassword: z
      .string()
      .regex(
        passwordRequirements,
        "Use 12+ characters with uppercase, lowercase, a number and a special character.",
      ),
    confirmPassword: z.string().min(1, "Confirm your password."),
  })
  .refine((values) => values.newPassword === values.confirmPassword, {
    path: ["confirmPassword"],
    message: "Passwords do not match.",
  });


export const profileSchema = z.object({
  displayName: z.string().trim().min(2, "Use at least 2 characters for your name.").max(100, "Use 100 characters or fewer."),
});

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Enter your current password."),
    newPassword: z
      .string()
      .regex(
        passwordRequirements,
        "Use 12+ characters with uppercase, lowercase, a number and a special character.",
      ),
    confirmPassword: z.string().min(1, "Confirm your new password."),
  })
  .refine((values) => values.newPassword === values.confirmPassword, {
    path: ["confirmPassword"],
    message: "Passwords do not match.",
  })
  .refine((values) => values.currentPassword !== values.newPassword, {
    path: ["newPassword"],
    message: "Your new password must be different from your current password.",
  });
