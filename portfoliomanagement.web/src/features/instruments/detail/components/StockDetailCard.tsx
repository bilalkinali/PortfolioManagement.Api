import { useEffect, useState } from "react";
import { Badge } from "@/components/ui/badge";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import StockProfileSection from "./StockProfileSection";
import StockDetailCardSkeleton from "./StockDetailCardSkeleton";
import StockHistoryChart from "./StockHistoryChart";
import {
    getStockProfile,
    type StockProfileResponse,
} from "../api/getStockProfile";
import { formatExchangeName } from "@/shared/helpers/formatters";

type StockDetailCardProps = {
    symbol: string;
};

export default function StockDetailCard({ symbol }: StockDetailCardProps) {
    const [profile, setProfile] = useState<StockProfileResponse | null>(null);
    const [isProfileLoading, setIsProfileLoading] = useState(true);
    const [profileError, setProfileError] = useState<string | null>(null);

    useEffect(() => {
        async function loadProfile() {
            try {
                setIsProfileLoading(true);
                setProfileError(null);

                const result = await getStockProfile(symbol);
                setProfile(result);
            } catch {
                setProfileError("Failed to load stock profile");
            } finally {
                setIsProfileLoading(false);
            }
        }

        loadProfile();
    }, [symbol]);
    

    if (isProfileLoading) return <StockDetailCardSkeleton />;
    if (profileError) return <p>{profileError}</p>;
    if (!profile) return <p>No stock profile found.</p>;

    const title = profile.name ?? profile.ticker ?? "Unknown instrument";
    const ticker = profile.ticker ?? "N/A";
    const exchange = profile.primaryExchange ?? profile.market ?? "Unknown market";

    return (
        <Card>
            <CardHeader className="mb-2 sm:grid-cols-[1fr_auto]">
                <div className="flex min-w-0 flex-col gap-4 sm:flex-row sm:items-start">
                    <StockLogo ticker={ticker} logoUrl={profile.branding?.logoUrl} />

                    <div className="flex min-w-0 flex-1 justify-between gap-4">
                        <div className="min-w-0">
                            <CardTitle className="text-2xl">{title}</CardTitle>

                            <CardDescription className="mt-1 flex flex-wrap items-center gap-2">
                                <span className="font-mono text-foreground">{ticker}</span>
                                <span>{formatExchangeName(exchange)}</span>
                                {profile.currencyName && (
                                    <span>{profile.currencyName.toUpperCase()}</span>
                                )}
                            </CardDescription>
                        </div>

                        <div className="mb-2 flex shrink-0 flex-wrap items-center justify-end gap-2">
                            <Badge variant={profile.active ? "secondary" : "outline"}>
                                {profile.active ? "Active" : "Inactive"}
                            </Badge>

                            {profile.type && <Badge variant="outline">{profile.type}</Badge>}

                            {profile.locale && (
                                <Badge variant="outline">{profile.locale.toUpperCase()}</Badge>
                            )}
                        </div>
                    </div>
                </div>
            </CardHeader>

            <CardContent className="space-y-6">
                <StockHistoryChart
                    symbol={symbol}
                    currency={profile.currencyName}
                />

                <StockProfileSection profile={profile} />
            </CardContent>
        </Card>
    );
}

type StockLogoProps = {
    ticker: string;
    logoUrl?: string | null;
};

function StockLogo({ ticker, logoUrl }: StockLogoProps) {
    const [failedLogoUrl, setFailedLogoUrl] = useState<string | null>(null);
    const shouldShowLogo = logoUrl && failedLogoUrl !== logoUrl;

    return (
        <div className="flex size-14 shrink-0 items-center justify-center overflow-hidden rounded-md border bg-muted font-mono text-sm font-semibold">
            {shouldShowLogo ? (
                <img
                    src={logoUrl}
                    alt=""
                    className="size-full object-contain p-2"
                    onError={() => setFailedLogoUrl(logoUrl)}
                />
            ) : (
                getTickerMark(ticker)
            )}
        </div>
    );
}

function getTickerMark(ticker: string) {
    return ticker.slice(0, 3).toUpperCase();
}