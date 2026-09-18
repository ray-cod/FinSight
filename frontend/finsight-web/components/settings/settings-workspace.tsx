"use client";

import * as React from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { KeyRound, Bell, UserRound } from "lucide-react";

import { NotificationPreferencesPanel } from "@/components/settings/notification-preferences";
import { ProfileSettingsPanel } from "@/components/settings/profile-settings";
import { SecuritySettingsPanel } from "@/components/settings/security-settings";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

const settingsSections = [
    { value: "profile", label: "Profile", icon: UserRound },
    { value: "security", label: "Security", icon: KeyRound },
    { value: "notifications", label: "Notifications", icon: Bell },
] as const;

type SettingsSection = (typeof settingsSections)[number]["value"];

/** Provides the consolidated settings workspace without unnecessary sub-routes. */
export function SettingsWorkspace() {
    const router = useRouter();
    const pathname = usePathname();
    const searchParams = useSearchParams();
    const requestedSection = searchParams.get("section");
    const section: SettingsSection = settingsSections.some(
        (item) => item.value === requestedSection,
    )
        ? (requestedSection as SettingsSection)
        : "profile";

    const changeSection = (value: string | null) => {
        if (!value || !settingsSections.some((item) => item.value === value))
            return;
        const next = new URLSearchParams(searchParams.toString());
        next.set("section", value);
        router.replace(`${pathname}?${next.toString()}`, { scroll: false });
    };

    return (
        <Tabs
            value={section}
            onValueChange={changeSection}
            orientation="vertical"
            className="gap-6 md:flex-row"
        >
            <TabsList
                variant="line"
                className="w-full shrink-0 flex-col items-stretch md:w-48"
            >
                {settingsSections.map((item) => {
                    const Icon = item.icon;
                    return (
                        <TabsTrigger
                            key={item.value}
                            value={item.value}
                            className="justify-start px-3 py-2 text-left"
                        >
                            <Icon aria-hidden="true" />
                            {item.label}
                        </TabsTrigger>
                    );
                })}
            </TabsList>
            <div className="min-w-0 flex-1">
                <TabsContent value="profile">
                    <ProfileSettingsPanel />
                </TabsContent>
                <TabsContent value="security">
                    <SecuritySettingsPanel />
                </TabsContent>
                <TabsContent value="notifications">
                    <NotificationPreferencesPanel />
                </TabsContent>
            </div>
        </Tabs>
    );
}
