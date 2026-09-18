"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { ChevronDown, LogOut, Monitor, Moon, Palette, Settings2, Sun, UserRound } from "lucide-react";

import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuSeparator, DropdownMenuSub, DropdownMenuSubContent, DropdownMenuSubTrigger, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { useAuth } from "@/lib/auth/auth-context";
import { useTheme, type ThemePreference } from "@/lib/theme/theme-provider";

function getInitials(displayName: string): string {
  const parts = displayName.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "FS";
  return parts.slice(0, 2).map((part) => part[0]?.toUpperCase() ?? "").join("");
}

/** Renders the current user's identity and account actions in the application header. */
export function UserMenu() {
  const { user, logout } = useAuth();
  const { theme, setTheme } = useTheme();
  const router = useRouter();

  if (!user) return null;

  const initials = getInitials(user.displayName);

  const handleLogout = async () => {
    await logout();
    router.replace("/login");
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" className="gap-2 px-2" aria-label="Open account menu">
          <Avatar className="size-8">
            <AvatarFallback>{initials}</AvatarFallback>
          </Avatar>
          <span className="hidden max-w-36 truncate text-sm font-medium sm:block">{user.displayName}</span>
          <ChevronDown className="hidden size-4 text-muted-foreground sm:block" aria-hidden="true" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-60">
        <div className="px-2 py-2">
          <p className="truncate text-sm font-medium">{user.displayName}</p>
          <p className="truncate text-xs text-muted-foreground">{user.email}</p>
        </div>
        <DropdownMenuSeparator />
        <DropdownMenuItem asChild>
          <Link href="/settings"><UserRound />Profile & settings</Link>
        </DropdownMenuItem>
        <DropdownMenuItem asChild>
          <Link href="/settings?section=security"><Settings2 />Account security</Link>
        </DropdownMenuItem>
        <DropdownMenuSub>
          <DropdownMenuSubTrigger><Palette />Appearance</DropdownMenuSubTrigger>
          <DropdownMenuSubContent>
            <DropdownMenuLabel>Theme</DropdownMenuLabel>
            <DropdownMenuRadioGroup value={theme} onValueChange={(value) => setTheme(value as ThemePreference)}>
              <DropdownMenuRadioItem value="system"><Monitor />System</DropdownMenuRadioItem>
              <DropdownMenuRadioItem value="light"><Sun />Light</DropdownMenuRadioItem>
              <DropdownMenuRadioItem value="dark"><Moon />Dark</DropdownMenuRadioItem>
            </DropdownMenuRadioGroup>
          </DropdownMenuSubContent>
        </DropdownMenuSub>
        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={() => void handleLogout()} variant="destructive">
          <LogOut />Sign out
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
