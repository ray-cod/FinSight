import { z } from "zod";

const clientEnvSchema = z.object({
  NEXT_PUBLIC_API_BASE_URL: z
    .string()
    .url()
    .default("http://localhost:5059"),
  NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT: z
    .union([z.string().url(), z.literal("")])
    .optional(),
});

/**
 * Resolved client-side environment configuration.
 */
export const clientEnv = clientEnvSchema.parse({
  NEXT_PUBLIC_API_BASE_URL:
    process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5059",
  NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT: process.env.NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT ?? "",
});
