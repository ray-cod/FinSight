const currencyFormatterCache = new Map<string, Intl.NumberFormat>();

/**
 * Formats a monetary amount according to the supplied ISO currency code.
 */
export function formatCurrency(
  amount: number,
  currency = "USD",
  locale = "en-US",
): string {
  const cacheKey = `${locale}:${currency}`;
  let formatter = currencyFormatterCache.get(cacheKey);

  if (!formatter) {
    formatter = new Intl.NumberFormat(locale, {
      style: "currency",
      currency,
      maximumFractionDigits: 2,
    });
    currencyFormatterCache.set(cacheKey, formatter);
  }

  return formatter.format(amount);
}
