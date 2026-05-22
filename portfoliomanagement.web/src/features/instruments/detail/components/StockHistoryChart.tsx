import { useEffect, useMemo, useState, type ComponentProps } from "react";
import { Area, Bar, CartesianGrid, ComposedChart, XAxis, YAxis } from "recharts";
import {
    ChartContainer,
    ChartTooltip,
    ChartTooltipContent,
    type ChartConfig,
} from "@/components/ui/chart";
import {
    getStockHistory,
    type StockBar,
    type GetStockHistoryResponse,
} from "@/features/instruments/detail/api/getStockHistory";
import StockHistoryChartSkeleton from "@/features/instruments/detail/components/StockHistoryChartSkeleton";
import StockRangeSelector from "@/features/instruments/detail/components/StockRangeSelector";
import { type StockRange } from "@/features/instruments/detail/types/StockRange";
import { toDateOnlyString } from "@/shared/helpers/formatters";

type StockHistoryChartProps = {
    symbol: string;
    currency?: string | null;
};

type StockHistoryPoint = {
    date: string;
    close: number;
    volume: number;
};

const chartConfig = {
    close: {
        label: "Close",
        color: "var(--chart-1)",
    },
    volume: {
        label: "Volume",
        color: "var(--chart-3)",
    },
} satisfies ChartConfig;

export default function StockHistoryChart({
    symbol,
    currency,
}: StockHistoryChartProps) {
    const [history, setHistory] = useState<GetStockHistoryResponse | null>(null);
    const [selectedRange, setSelectedRange] = useState<StockRange>("1Y");
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const rangeQuery = useMemo(
        () => getRangeQuery(selectedRange),
        [selectedRange]
    );

    useEffect(() => {
        async function loadHistory() {
            try {
                setIsLoading(true);
                setError(null);

                const result = await getStockHistory({
                    ticker: symbol,
                    from: rangeQuery.from,
                    to: rangeQuery.to,
                    timespan: rangeQuery.timespan,
                });

                setHistory(result);
            } catch {
                setError("Failed to load stock history");
                setHistory(null);
            } finally {
                setIsLoading(false);
            }
        }

        loadHistory();
    }, [symbol, rangeQuery.from, rangeQuery.to, rangeQuery.timespan]);

    const chartData = useMemo(() => {
        const bars = history?.results ?? [];

        return bars
            .map(toChartPoint)
            .sort((first, second) => first.date.localeCompare(second.date));
    }, [history?.results]);

    return (
        <section className="flex flex-col gap-3">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                <div>
                    <h3 className="text-base font-semibold">Price history</h3>
                    <p className="text-sm text-muted-foreground">
                        Daily close and volume from {formatLongDate(rangeQuery.from)} to{" "}
                        {formatLongDate(rangeQuery.to)}.
                    </p>
                </div>

                <StockRangeSelector
                    selectedRange={selectedRange}
                    onRangeChange={setSelectedRange}
                />
            </div>

            {isLoading && <StockHistoryChartSkeleton />}

            {!isLoading && error && (
                <p className="text-sm text-destructive">{error}</p>
            )}

            {!isLoading && !error && chartData.length === 0 && (
                <p className="text-sm text-muted-foreground">
                    No historical price bars found.
                </p>
            )}

            {!isLoading && !error && chartData.length > 0 && (
                <HistoryChartContent chartData={chartData} currency={currency} />
            )}
        </section>
    );
}

type HistoryChartContentProps = {
    chartData: StockHistoryPoint[];
    currency?: string | null;
};

function HistoryChartContent({
    chartData,
    currency,
}: HistoryChartContentProps) {
    const maxVolume = Math.max(...chartData.map((x) => x.volume));

    return (
        <ChartContainer config={chartConfig} className="h-72 w-full">
            <ComposedChart
                accessibilityLayer
                data={chartData}
                margin={{ left: 0, right: 0, top: 8 }}
            >
                <CartesianGrid vertical={false} />

                <XAxis
                    dataKey="date"
                    tickLine={false}
                    axisLine={false}
                    tickMargin={8}
                    minTickGap={32}
                    tickFormatter={formatAxisDate}
                />

                <YAxis
                    yAxisId="price"
                    tickLine={false}
                    axisLine={false}
                    tickMargin={8}
                    width={55}
                    orientation="right"
                    domain={["dataMin", "dataMax"]}
                    tickFormatter={(value) => formatPriceTick(value, currency)}
                />

                <YAxis
                    yAxisId="volume"
                    orientation="right"
                    hide
                    domain={[0, maxVolume * 2 || 1]}
                />

                <ChartTooltip
                    content={(props) => (
                        <ChartTooltipContent
                            active={props.active}
                            indicator="line"
                            label={props.label}
                            labelFormatter={(value) => formatLongDate(String(value))}
                            payload={formatTooltipPayload(props.payload, currency)}
                        />
                    )}
                />

                <Area
                    yAxisId="price"
                    type="monotone"
                    dataKey="close"
                    fill="var(--color-close)"
                    fillOpacity={0.18}
                    stroke="var(--color-close)"
                    strokeWidth={2}
                />

                <Bar
                    yAxisId="volume"
                    dataKey="volume"
                    fill="var(--color-volume)"
                    radius={[5, 5, 0, 0]}
                    opacity={0.2}
                />
            </ComposedChart>
        </ChartContainer>
    );
}

function toChartPoint(bar: StockBar): StockHistoryPoint {
    return {
        date: toDateOnlyString(new Date(bar.t)),
        close: bar.c,
        volume: Math.round(bar.v),
    };
}

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
        from: toDateOnlyString(fromDate),
        to: toDateOnlyString(toDate),
        timespan: "day",
    };
}

function formatAxisDate(value: string) {
    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        year: "numeric",
    }).format(new Date(value));
}

function formatLongDate(value: string) {
    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    }).format(new Date(value));
}

function formatPriceTick(value: number, currency?: string | null) {
    return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: currency?.toUpperCase() ?? "USD",
        notation: "compact",
        maximumFractionDigits: 1,
    }).format(value);
}

type ChartTooltipContentPayload =
    ComponentProps<typeof ChartTooltipContent>["payload"];

function formatTooltipPayload(
    payload: ChartTooltipContentPayload,
    currency?: string | null
): ChartTooltipContentPayload {
    return payload?.map((item) => {
        if (
            (item.dataKey !== "close" && item.name !== "close") ||
            typeof item.value !== "number"
        ) {
            return item;
        }

        return {
            ...item,
            value: formatCurrencyTooltipValue(item.value, currency),
        };
    }) as ChartTooltipContentPayload;
}

function formatCurrencyTooltipValue(value: number, currency?: string | null) {
    return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: currency?.toUpperCase() ?? "USD",
        maximumFractionDigits: 2,
    }).format(value);
}