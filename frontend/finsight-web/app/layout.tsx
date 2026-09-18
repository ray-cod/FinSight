import type { Metadata, Viewport } from "next";
import { Geist, Geist_Mono } from "next/font/google";

import { AppProviders } from "@/components/providers/app-providers";
import { ThemeInitScript } from "@/components/theme/theme-init-script";

import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: {
    default: "FinSight",
    template: "%s | FinSight",
  },
  description:
    "The smart subscription and financial anomaly engine for clearer everyday financial decisions.",
};

export const viewport: Viewport = {
  colorScheme: "light dark",
  themeColor: "#17191f",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${geistSans.variable} ${geistMono.variable} min-h-screen font-sans antialiased`}>
        <ThemeInitScript />
        <AppProviders>{children}</AppProviders>
      </body>
    </html>
  );
}
