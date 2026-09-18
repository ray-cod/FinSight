import Script from "next/script";

const script = `(() => { try { const stored = localStorage.getItem("finsight-theme"); const theme = stored === "dark" || stored === "light" ? stored : (window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light"); document.documentElement.classList.toggle("dark", theme === "dark"); document.documentElement.style.colorScheme = theme; } catch {} })();`;

/** Applies the persisted appearance preference before hydration to minimize visual theme flashing. */
export function ThemeInitScript() {
  return <Script id="finsight-theme-init" strategy="beforeInteractive">{script}</Script>;
}
