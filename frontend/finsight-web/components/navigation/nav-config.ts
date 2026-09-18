import {
  Bell,
  CreditCard,
  LayoutDashboard,
  Lightbulb,
  Repeat2,
  Settings2,
  ShieldAlert,
  WalletCards,
  type LucideIcon,
} from "lucide-react";

export interface NavigationItem {
  readonly href: string;
  readonly label: string;
  readonly description: string;
  readonly icon: LucideIcon;
}

/**
 * Defines the authenticated FinSight navigation model.
 */
export const primaryNavigation: readonly NavigationItem[] = [
  {
    href: "/dashboard",
    label: "Overview",
    description: "Your financial picture at a glance",
    icon: LayoutDashboard,
  },
  {
    href: "/accounts",
    label: "Accounts",
    description: "Connected accounts and balances",
    icon: WalletCards,
  },
  {
    href: "/transactions",
    label: "Transactions",
    description: "Review and classify activity",
    icon: CreditCard,
  },
  {
    href: "/subscriptions",
    label: "Subscriptions",
    description: "Recurring payments and price changes",
    icon: Repeat2,
  },
  {
    href: "/anomalies",
    label: "Anomalies",
    description: "Unusual financial activity",
    icon: ShieldAlert,
  },
  {
    href: "/insights",
    label: "Insights",
    description: "Financial intelligence from your data",
    icon: Lightbulb,
  },
  {
    href: "/notifications",
    label: "Notifications",
    description: "Alerts and updates",
    icon: Bell,
  },
];

/**
 * Defines the secondary navigation model.
 */
export const secondaryNavigation: readonly NavigationItem[] = [
  {
    href: "/settings",
    label: "Settings",
    description: "Profile, security and preferences",
    icon: Settings2,
  },
];
