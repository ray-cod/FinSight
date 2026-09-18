import Link from "next/link";
import { FileQuestion } from "lucide-react";

import { EmptyState } from "@/components/feedback/empty-state";
import { Button } from "@/components/ui/button";

export default function NotFound() {
  return (
    <main className="flex min-h-screen items-center justify-center px-4 py-16 sm:px-6">
      <div className="w-full max-w-xl">
        <EmptyState icon={FileQuestion} title="Page not found" description="The page you requested does not exist or has moved to another area of the FinSight workspace." />
        <div className="mt-4 flex justify-center"><Button asChild><Link href="/dashboard">Back to overview</Link></Button></div>
      </div>
    </main>
  );
}
