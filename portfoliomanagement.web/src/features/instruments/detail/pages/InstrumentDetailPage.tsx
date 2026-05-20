import { useEffect, useState } from "react";
import { useParams } from "react-router";
import StockDetailCard from "@/features/instruments/detail/components/StockDetailCard";
import StockProfileCardSkeleton from "@/features/instruments/detail/components/StockProfileCardSkeleton";
import { type StockRange } from "../types/StockRange";
import {
    getStockProfile,
    type StockProfileResponse
} from "@/features/instruments/detail/api/getStockProfile";

import {
    getStockHistory,
    type GetStockHistoryResponse
} from "@/features/instruments/detail/api/getStockHistory";

import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator
} from "@/components/ui/breadcrumb";

function getRangeQuery(range: StockRange) {
    const toDate = new Date();
    const fromDate = new Date(toDate);

    switch (range) {
        case "5D":
            fromDate.setDate(fromDate.getDate() - 5);
            break;
        case "1M":
            fromDate.setMonth(fromDate.getMonth() - 1);
            break;
        case "3M":
            fromDate.setMonth(fromDate.getMonth() - 3);
            break;
        case "1Y":
            fromDate.setFullYear(fromDate.getFullYear() - 1);
            break;
        case "5Y":
            fromDate.setFullYear(fromDate.getFullYear() - 5);
            break;
        case "ALL":
            fromDate.setFullYear(1990, 0, 1);
            break;
    }

    return {
        from: toDateString(fromDate),
        to: toDateString(toDate),
        timespan: "day"
    };
}

function toDateString(date: Date) {
    return date.toISOString().slice(0, 10);
}

export default function InstrumentDetailPage() {
    const { symbol } = useParams<{ symbol: string }>();

    const [profile, setProfile] = useState<StockProfileResponse | null>(null);
    const [history, setHistory] = useState<GetStockHistoryResponse | null>(null);

    const [selectedRange, setSelectedRange] = useState<StockRange>("1Y");

    const [isProfileLoading, setIsProfileLoading] = useState(true);
    const [isHistoryLoading, setIsHistoryLoading] = useState(true);

    const [profileError, setProfileError] = useState<string | null>(null);
    const [historyError, setHistoryError] = useState<string | null>(null);

    const rangeQuery = getRangeQuery(selectedRange);

    useEffect(() => {
        if (!symbol) {
            setProfileError("Missing symbol");
            setIsProfileLoading(false);
            return;
        }

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
        if (!symbol) {
            setHistoryError("Missing symbol");
            setIsHistoryLoading(false);
            return;
        }

        async function loadHistory() {
            try {
                setIsHistoryLoading(true);
                setHistoryError(null);

                const result = await getStockHistory({
                    ticker: symbol,
                    from: rangeQuery.from,
                    to: rangeQuery.to,
                    timespan: rangeQuery.timespan,
                });

                setHistory(result);
            } catch {
                setHistoryError("Failed to load stock history");
            } finally {
                setIsHistoryLoading(false);
            }
        }

        loadHistory();
    }, [symbol, rangeQuery.from, rangeQuery.to, rangeQuery.timespan]);

    if (isProfileLoading) return <StockProfileCardSkeleton />;
    if (profileError) return <p>{profileError}</p>;
    if (!profile) return <p>No stock profile found.</p>;

    return (
        <>
            <div className="mb-2">
                <Breadcrumb>
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/">Home</BreadcrumbLink>
                        </BreadcrumbItem>

                        <BreadcrumbSeparator />

                        <BreadcrumbItem>
                            Instruments
                        </BreadcrumbItem>

                        <BreadcrumbSeparator />

                        <BreadcrumbItem>
                            <BreadcrumbPage>{profile.ticker}</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>
            </div>

            <StockDetailCard
                profile={profile}
                history={history}
                selectedRange={selectedRange}
                onRangeChange={setSelectedRange}
                isHistoryLoading={isHistoryLoading}
                historyError={historyError}
                historyFrom={rangeQuery.from}
                historyTo={rangeQuery.to}
            />
        </>
    );
}