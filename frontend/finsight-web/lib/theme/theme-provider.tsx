"use client";

import * as React from "react";

export type ThemePreference = "system" | "light" | "dark";
export type ResolvedTheme = "light" | "dark";

interface ThemeContextValue {
    /** Stored appearance preference. */
    readonly theme: ThemePreference;
    /** Effective appearance after applying a system preference when configured. */
    readonly resolvedTheme: ResolvedTheme;
    /** Changes and persists the appearance preference. */
    readonly setTheme: (theme: ThemePreference) => void;
}

const ThemeContext = React.createContext<ThemeContextValue | null>(null);
const STORAGE_KEY = "finsight-theme";

function resolveTheme(theme: ThemePreference): ResolvedTheme {
    if (theme !== "system") return theme;
    return window.matchMedia("(prefers-color-scheme: dark)").matches
        ? "dark"
        : "light";
}

function applyTheme(theme: ThemePreference): ResolvedTheme {
    const resolved = resolveTheme(theme);
    const root = document.documentElement;
    root.classList.toggle("dark", resolved === "dark");
    root.style.colorScheme = resolved;
    return resolved;
}

/** Provides a persistent light/dark/system appearance preference. */
export function ThemeProvider({
    children,
}: {
    readonly children: React.ReactNode;
}) {
    const [theme, setThemeState] = React.useState<ThemePreference>(() => {
        if (typeof window === "undefined") return "system";
        const stored = window.localStorage.getItem(STORAGE_KEY);
        return stored === "light" || stored === "dark" || stored === "system"
            ? stored
            : "system";
    });
    const [resolvedTheme, setResolvedTheme] = React.useState<ResolvedTheme>(
        () => {
            if (typeof window === "undefined") return "light";
            const stored = window.localStorage.getItem(STORAGE_KEY);
            const initial: ThemePreference =
                stored === "light" || stored === "dark" || stored === "system"
                    ? stored
                    : "system";
            return applyTheme(initial);
        },
    );

    React.useEffect(() => {
        if (theme !== "system") return;
        const media = window.matchMedia("(prefers-color-scheme: dark)");
        const update = () => setResolvedTheme(applyTheme("system"));
        media.addEventListener("change", update);
        return () => media.removeEventListener("change", update);
    }, [theme]);

    const setTheme = React.useCallback((next: ThemePreference) => {
        window.localStorage.setItem(STORAGE_KEY, next);
        setThemeState(next);
        setResolvedTheme(applyTheme(next));
    }, []);

    const value = React.useMemo(
        () => ({ theme, resolvedTheme, setTheme }),
        [resolvedTheme, setTheme, theme],
    );
    return (
        <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
    );
}

/** Returns the current FinSight appearance preference. */
export function useTheme(): ThemeContextValue {
    const context = React.useContext(ThemeContext);
    if (!context)
        throw new Error("useTheme must be used within ThemeProvider.");
    return context;
}
