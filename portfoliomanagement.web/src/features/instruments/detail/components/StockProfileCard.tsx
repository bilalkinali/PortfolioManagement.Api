import {
    Building2Icon,
    CalendarDaysIcon,
    CircleDollarSignIcon,
    ExternalLinkIcon,
    GlobeIcon,
    LandmarkIcon,
    MapPinIcon,
    UsersIcon,
} from "lucide-react"
import { useState, type ReactNode } from "react"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"
import type { StockProfileResponse } from "@/features/instruments/detail/api/getStockProfile"
import StockHistoryChart from "@/features/instruments/detail/components/StockHistoryChart"
import { formatExchangeName } from "@/shared/helpers/formatters"

type StockProfileCardProps = {
    profile: StockProfileResponse
}

export default function StockProfileCard({ profile }: StockProfileCardProps) {
    const title = profile.name ?? profile.ticker ?? "Unknown instrument"
    const ticker = profile.ticker ?? "N/A"
    const exchange = profile.primaryExchange ?? profile.market ?? "Unknown market"
    const homepageUrl = getExternalUrl(profile.homepageUrl)
    const address = formatAddress(profile.address)

    return (
        <Card>
            <CardHeader className="mb-2 sm:grid-cols-[1fr_auto]">
                <div className="flex min-w-0 flex-col gap-4 sm:flex-row sm:items-start">
                    <StockLogo ticker={ticker} logoUrl={profile.branding?.logoUrl} />

                    <div className="flex min-w-0 flex-1 justify-between">
                        <div className="min-w-0">
                            <CardTitle className="text-2xl">{title}</CardTitle>
                            <CardDescription className="mt-1 flex flex-wrap items-center gap-2">
                                <span className="font-mono text-foreground">{ticker}</span>
                                <span>{formatExchangeName(exchange)}</span>
                                {profile.currencyName && <span>{profile.currencyName.toUpperCase()}</span>}
                            </CardDescription>
                        </div>

                        <div className="mb-2 flex shrink-0 flex-wrap items-center justify-end gap-2">
                            <Badge variant={profile.active ? "secondary" : "outline"}>
                                {profile.active ? "Active" : "Inactive"}
                            </Badge>
                            {profile.type && <Badge variant="outline">{profile.type}</Badge>}
                            {profile.locale && <Badge variant="outline">{profile.locale.toUpperCase()}</Badge>}
                        </div>
                    </div>
                </div>

                
            </CardHeader>

            <CardContent className="flex flex-col gap-6">
                {profile.ticker && (
                    <>
                        <StockHistoryChart ticker={profile.ticker} currency={profile.currencyName} />
                        <Separator />
                    </>
                )}

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
                    <section className="flex flex-col gap-2">
                        <h2 className="font-medium">Company profile</h2>
                        <p className="text-sm leading-6 text-muted-foreground">
                            {profile.description ?? "No company description is available yet."}
                        </p>

                        {homepageUrl && (
                            <CardAction className="mt-4">
                                <Button variant="outline" size="sm" asChild>
                                    <a href={homepageUrl} target="_blank" rel="noreferrer">
                                        <ExternalLinkIcon data-icon="inline-start" />
                                        Website
                                    </a>
                                </Button>
                            </CardAction>
                        )}
                    </section>
                    
                    <aside className="flex flex-col gap-4">
                        <DetailRow icon={<Building2Icon />} label="Industry" value={profile.sicDescription} />
                        <DetailRow icon={<GlobeIcon />} label="Market" value={profile.market} />
                        <DetailRow icon={<MapPinIcon />} label="Address" value={address} />
                        <DetailRow
                            icon={<CalendarDaysIcon />}
                            label="Last synced"
                            value={formatDate(profile.lastSyncedDate)}
                        />
                    </aside>
                </div>
            </CardContent>
        </Card>
    )
}

type StockLogoProps = {
    ticker: string
    logoUrl?: string | null
}

function StockLogo({ ticker, logoUrl }: StockLogoProps) {
    const [failedLogoUrl, setFailedLogoUrl] = useState<string | null>(null)
    const shouldShowLogo = logoUrl && failedLogoUrl !== logoUrl

    return (
        <div className="flex size-14 shrink-0 items-center justify-center overflow-hidden rounded-md border bg-muted font-mono text-sm font-semibold">
            {shouldShowLogo ? (
                <img
                    src={logoUrl}
                    alt=""
                    className="size-full object-contain p-2"
                    onError={() => {setFailedLogoUrl(logoUrl)}}
                />
            ) : (
                getTickerMark(ticker)
            )}
        </div>
    )
}

type ProfileMetricProps = {
    icon: ReactNode
    label: string
    value: string
}

function ProfileMetric({ icon, label, value }: ProfileMetricProps) {
    return (
        <div className="rounded-md border bg-muted/30 p-4">
            <div className="mb-3 flex items-center gap-2 text-muted-foreground">
                {icon}
                <span className="text-xs font-medium uppercase tracking-normal">{label}</span>
            </div>
            <div className="text-lg font-semibold">{value}</div>
        </div>
    )
}

type DetailRowProps = {
    icon: ReactNode
    label: string
    value?: string | null
}

function DetailRow({ icon, label, value }: DetailRowProps) {
    return (
        <div className="flex gap-3">
            <div className="mt-0.5 text-muted-foreground">{icon}</div>
            <div className="min-w-0">
                <div className="text-xs font-medium uppercase tracking-normal text-muted-foreground">{label}</div>
                <div className="text-sm">{value || "N/A"}</div>
            </div>
        </div>
    )
}

function formatMarketCap(value: number | null, currency?: string | null) {
    if (value === null) {
        return "N/A"
    }

    const formattedValue = formatCompactNumber(value)
    return currency ? `${formattedValue} ${currency.toUpperCase()}` : formattedValue
}

function formatCompactNumber(value: number | null) {
    if (value === null) {
        return "N/A"
    }

    return new Intl.NumberFormat("en-US", {
        notation: "compact",
        maximumFractionDigits: 1,
    }).format(value)
}

function formatDate(value?: string | null) {
    if (!value) {
        return "N/A"
    }

    const date = new Date(value)

    if (Number.isNaN(date.getTime())) {
        return value
    }

    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    }).format(date)
}

function formatAddress(address: StockProfileResponse["address"]) {
    if (!address) {
        return null
    }

    return [address.address1, address.city, address.state, address.postalCode]
        .filter(Boolean)
        .join(", ")
}

function getTickerMark(ticker: string) {
    return ticker.slice(0, 3).toUpperCase()
}

function getExternalUrl(value?: string | null) {
    if (!value) {
        return null
    }

    return value.startsWith("http://") || value.startsWith("https://")
        ? value
        : `https://${value}`
}
