import { describe, expect, it } from "vitest";

import { loginSchema, registerSchema } from "@/lib/validation/auth";

describe("authentication validation", () => {
  it("accepts a backend-compatible strong registration password", () => {
    expect(
      registerSchema.safeParse({
        displayName: "Alex Morgan",
        email: "alex@example.com",
        password: "SecurePassword1!",
        confirmPassword: "SecurePassword1!",
      }).success,
    ).toBe(true);
  });

  it("rejects mismatched passwords", () => {
    const result = registerSchema.safeParse({
      displayName: "Alex Morgan",
      email: "alex@example.com",
      password: "SecurePassword1!",
      confirmPassword: "DifferentPassword1!",
    });

    expect(result.success).toBe(false);
  });

  it("does not impose registration password complexity on sign-in", () => {
    expect(loginSchema.safeParse({ email: "alex@example.com", password: "existing-secret" }).success).toBe(true);
  });
});
