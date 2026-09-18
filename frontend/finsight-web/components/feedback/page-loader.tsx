import { Skeleton } from "@/components/ui/skeleton";

/** Provides the shared accessible loading treatment used by route transitions and guarded pages. */
export function PageLoader() {
  return (
    <div className="space-y-6" aria-label="Loading FinSight" aria-live="polite" aria-busy="true" role="status">
      <span className="sr-only">Loading FinSight…</span>
      <div className="space-y-2"><Skeleton className="h-8 w-48" /><Skeleton className="h-4 w-80 max-w-full" /></div>
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">{["one", "two", "three", "four"].map((key) => <Skeleton className="h-28 rounded-xl" key={key} />)}</div>
      <Skeleton className="h-80 rounded-xl" />
    </div>
  );
}

/** Provides the shared skeleton treatment for detail routes. */
export function DetailPageLoader() {
  return (
    <div className="space-y-6" aria-label="Loading details" aria-live="polite" aria-busy="true" role="status">
      <span className="sr-only">Loading details…</span>
      <Skeleton className="h-8 w-56" />
      <Skeleton className="h-4 w-96 max-w-full" />
      <div className="grid gap-4 md:grid-cols-2">{Array.from({ length: 4 }, (_, index) => <Skeleton key={index} className="h-32 rounded-xl" />)}</div>
      <Skeleton className="h-64 rounded-xl" />
    </div>
  );
}
