import {
    Building2Icon,
    CalendarDaysIcon,
    CircleDollarSignIcon,
    ExternalLinkIcon,
    GlobeIcon,
    LandmarkIcon,
    MapPinIcon,
    UsersIcon,
} from "lucide-react";
import type { ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { CardAction } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import type { StockProfileResponse } from "@/features/instruments/detail/api/getStockProfile";

type StockProfileSectionProps = {
    profile: StockProfileResponse;
};

export default function StockProfileSection({ profile }: StockProfileSectionProps) {
    const homepageUrl = getExternalUrl(profile.homepageUrl);
    const address = formatAddress(profile.address);

    return (
        <div className="flex flex-col gap-6">

            <Separator />

            <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
                <ProfileMetric
                    icon={<CircleDollarSignIcon />}
                    label="Market cap"
                    value={formatMarketCap(profile.marketCap, profile.currencyName)}
                />

                <ProfileMetric
                    icon={<LandmarkIcon />}
                    label="Shares outstanding"
                    value={formatCompactNumber(profile.weightedSharesOutstanding)}
                />

                <ProfileMetric
                    icon={<UsersIcon />}
                    label="Employees"
                    value={formatCompactNumber(profile.totalEmployees)}
                />

                <ProfileMetric
                    icon={<CalendarDaysIcon />}
                    label="Listed"
                    value={formatDate(profile.listDate)}
                />
            </div>

            <Separator />

            <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_18rem]">
                <section className="flex flex-col gap-1">
                    <div className="flex items-center gap-1">
                        <h2 className="font-medium">Company profile</h2>
                        {homepageUrl && (
                            <CardAction className="mb-0.5">
                                <Button variant="ghost" size="sm" asChild>
                                    <a href={homepageUrl} target="_blank" rel="noreferrer">
                                        <ExternalLinkIcon />
                                    </a>
                                </Button>
                            </CardAction>
                        )}
                    </div>

                    <p className="text-sm leading-6 text-muted-foreground mr-2">
                        {profile.description ?? "No company description is available yet."}
                    </p>

                </section>

                <aside className="flex flex-col gap-4">
                    <DetailRow
                        icon={<Building2Icon />}
                        label="Industry"
                        value={profile.sicDescription}
                    />

                    <DetailRow
                        icon={<GlobeIcon />}
                        label="Market"
                        value={profile.market}
                    />

                    <DetailRow
                        icon={<MapPinIcon />}
                        label="Address"
                        value={address}
                    />

                    <DetailRow
                        icon={<CalendarDaysIcon />}
                        label="Last synced"
                        value={formatDate(profile.lastSyncedDate)}
                    />
                </aside>
            </div>
        </div>
    );
}

type ProfileMetricProps = {
    icon: ReactNode;
    label: string;
    value: string;
};

function ProfileMetric({ icon, label, value }: ProfileMetricProps) {
    return (
        <div className="rounded-md border bg-muted/30 p-4">
            <div className="mb-3 flex items-center gap-2 text-muted-foreground">
                {icon}
                <span className="text-xs font-medium uppercase tracking-normal">
                    {label}
                </span>
            </div>

            <div className="text-lg font-semibold">{value}</div>
        </div>
    );
}

type DetailRowProps = {
    icon: ReactNode;
    label: string;
    value?: string | null;
};

function DetailRow({ icon, label, value }: DetailRowProps) {
    return (
        <div className="flex gap-3">
            <div className="mt-0.5 text-muted-foreground">{icon}</div>

            <div className="min-w-0">
                <div className="text-xs font-medium uppercase tracking-normal text-muted-foreground">
                    {label}
                </div>

                <div className="text-sm">{value || "N/A"}</div>
            </div>
        </div>
    );
}

function formatMarketCap(value: number | null, currency?: string | null) {
    if (value === null) {
        return "N/A";
    }

    const formattedValue = formatCompactNumber(value);
    return currency ? `${formattedValue} ${currency.toUpperCase()}` : formattedValue;
}

function formatCompactNumber(value: number | null) {
    if (value === null) {
        return "N/A";
    }

    return new Intl.NumberFormat("en-US", {
        notation: "compact",
        maximumFractionDigits: 1,
    }).format(value);
}

function formatDate(value?: string | null) {
    if (!value) {
        return "N/A";
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    }).format(date);
}

function formatAddress(address: StockProfileResponse["address"]) {
    if (!address) {
        return null;
    }

    return [address.address1, address.city, address.state, address.postalCode]
        .filter(Boolean)
        .join(", ");
}

function getExternalUrl(value?: string | null) {
    if (!value) {
        return null;
    }

    return value.startsWith("http://") || value.startsWith("https://")
        ? value
        : `https://${value}`;
}