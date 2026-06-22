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
import { getStockProfile, type StockProfileResponse } from "../api/getStockProfile";
import { getStockQuote, type StockQuoteResponse } from "../api/getStockQuote";
import { formatExchangeName } from "@/shared/helpers/formatters";

type StockDetailCardProps = {
    symbol: string;
};

export default function StockDetailCard({ symbol }: StockDetailCardProps) {
    const [profile, setProfile] = useState<StockProfileResponse | null>(null);
    const [quote, setQuote] = useState<StockQuoteResponse | null>(null);
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

    useEffect(() => {
        async function loadQuote() {
            try {
                const result = await getStockQuote(symbol);
                setQuote(result);
            } catch {
                setQuote(null);
            }
        }

        loadQuote();
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

                        <QuoteSummary
                            currency={quote?.currency ?? profile.currencyName}
                            quote={quote}
                        />

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

function formatQuoteCurrency(value: number, currency?: string | null) {
    return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: currency?.toUpperCase() ?? "USD",
        maximumFractionDigits: 2,
    }).format(value);
}

function formatSignedCurrency(value: number, currency?: string | null) {
    const formatted = formatQuoteCurrency(Math.abs(value), currency);
    return `${value >= 0 ? "+" : "-"}${formatted}`;
}

function formatQuoteTimestamp(value: string) {
    return new Intl.DateTimeFormat("en-US", {
        dateStyle: "medium",
        timeStyle: "short",
    }).format(new Date(value));
}

function formatQuoteDate(value: string) {
    const [year, month, day] = value.split("-").map(Number);

    if (!year || !month || !day) {
        return value;
    }

    return new Intl.DateTimeFormat("en-US", {
        dateStyle: "medium",
    }).format(new Date(year, month - 1, day));
}

type QuoteSummaryProps = {
    quote: StockQuoteResponse | null;
    currency?: string | null;
};

function QuoteSummary({ quote, currency }: QuoteSummaryProps) {
    if (!quote) {
        return (
            <div className="hidden shrink-0 text-right sm:block">
                <div className="text-sm text-muted-foreground">Quote unavailable</div>
            </div>
        );
    }

    const change = quote.previousClose
        ? quote.currentPrice - quote.previousClose
        : null;
    const quoteMeta = quote.source === "Live"
        ? quote.timestampUtc
            ? `Live quote - ${formatQuoteTimestamp(quote.timestampUtc)}`
            : "Live quote"
        : quote.priceDate
            ? `Latest loaded price - ${formatQuoteDate(quote.priceDate)}`
            : "Latest loaded price";

    return (
        <div className="hidden shrink-0 text-right sm:block">
            <div className="text-xl font-semibold">
                {formatQuoteCurrency(quote.currentPrice, currency)}
            </div>
            {change !== null && (
                <div className="text-sm text-muted-foreground">
                    {formatSignedCurrency(change, currency)}
                </div>
            )}
            <div className="text-xs text-muted-foreground">
                {quoteMeta}
            </div>
        </div>
    );
}
