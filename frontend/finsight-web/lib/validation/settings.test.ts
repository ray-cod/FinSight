import { describe, expect, it } from "vitest";

import { changePasswordSchema, profileSchema } from "@/lib/validation/auth";

describe("settings validation", () => {
  it("accepts a valid profile name", () => {
    expect(profileSchema.safeParse({ displayName: "Alex Morgan" }).success).toBe(true);
  });

  it("rejects profile names longer than the backend limit", () => {
    expect(profileSchema.safeParse({ displayName: "x".repeat(101) }).success).toBe(false);
  });

  it("accepts a compliant password change", () => {
    expect(
      changePasswordSchema.safeParse({
        currentPassword: "OldPassword!234",
        newPassword: "NewPassword!234",
        confirmPassword: "NewPassword!234",
      }).success,
    ).toBe(true);
  });

  it("rejects matching current and new passwords", () => {
    expect(
      changePasswordSchema.safeParse({
        currentPassword: "Password!234",
        newPassword: "Password!234",
        confirmPassword: "Password!234",
      }).success,
    ).toBe(false);
  });
});
