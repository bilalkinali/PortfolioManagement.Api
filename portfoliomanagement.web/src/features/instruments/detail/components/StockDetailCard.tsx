import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import StockProfileSection from "./StockProfileSection";
import type { StockProfileResponse } from "../api/getStockProfile";
import type { GetStockHistoryResponse } from "../api/getStockHistory";
import StockHistoryChart from "./StockHistoryChart";
import { formatExchangeName } from "@/shared/helpers/formatters";
import StockRangeSelector from "./StockRangeSelector";
import { type StockRange } from "../types/StockRange";

type StockDetailCardProps = {
    profile: StockProfileResponse;
    history: GetStockHistoryResponse | null;
    selectedRange: StockRange;
    onRangeChange: (range: StockRange) => void;
    isHistoryLoading: boolean;
    historyError: string | null;
    historyFrom: string;
    historyTo: string;
};

export default function StockDetailCard({
    profile,
    history,
    selectedRange,
    onRangeChange,
    isHistoryLoading,
    historyError,
    historyFrom,
    historyTo,
}: StockDetailCardProps) {
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

            <CardContent className="flex flex-col gap-6">
                <section className="flex flex-col gap-3">

                    <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                        <div>
                            <h2 className="font-medium">Price history</h2>
                            <p className="text-sm text-muted-foreground">
                                Daily close and volume from {formatShortDate(historyFrom)} to {formatShortDate(historyTo)}.
                            </p>
                        </div>

                        <StockRangeSelector
                            selectedRange={selectedRange}
                            onRangeChange={onRangeChange}
                        />
                    </div>

                    <StockHistoryChart
                        history={history}
                        isLoading={isHistoryLoading}
                        error={historyError}
                        currency={profile.currencyName}
                    />
                </section>

                <Separator />

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

function formatShortDate(value: string) {
    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    }).format(new Date(value))
}