"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { Brand } from "@/components/layout/brand";
import {
  primaryNavigation,
  secondaryNavigation,
} from "@/components/navigation/nav-config";
import { Button } from "@/components/ui/button";
import { Sheet, SheetClose, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { Menu } from "lucide-react";

/**
 * Renders the mobile navigation drawer for authenticated FinSight pages.
 */
export function MobileNav() {
  const pathname = usePathname();

  return (
    <Sheet>
      <SheetTrigger asChild>
        <Button variant="outline" size="icon" className="md:hidden" aria-label="Open navigation">
          <Menu className="size-4" aria-hidden="true" />
        </Button>
      </SheetTrigger>
      <SheetContent side="left" className="w-80 max-w-[calc(100vw-2rem)] p-0">
        <SheetHeader className="border-b px-5 py-4 text-left">
          <SheetTitle>
            <Brand />
          </SheetTitle>
        </SheetHeader>
        <nav className="space-y-1 p-3" aria-label="Mobile navigation">
          {[...primaryNavigation, ...secondaryNavigation].map((item) => {
            const Icon = item.icon;
            const active = pathname === item.href || pathname.startsWith(`${item.href}/`);

            return (
              <SheetClose asChild key={item.href}>
                <Link
                  href={item.href}
                  className={`flex min-h-11 items-center gap-3 rounded-lg px-3 text-sm font-medium ${
                  active
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-accent hover:text-foreground"
                }`}
                  aria-current={active ? "page" : undefined}
                >
                  <Icon className="size-4" aria-hidden="true" />
                  {item.label}
                </Link>
              </SheetClose>
            );
          })}
        </nav>
      </SheetContent>
    </Sheet>
  );
}
