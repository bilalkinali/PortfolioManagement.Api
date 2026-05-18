import { useEffect, useMemo, useState, type ComponentProps } from "react"
import { Area, Bar, CartesianGrid, ComposedChart, XAxis, YAxis } from "recharts"
import { ChartContainer, ChartTooltip, ChartTooltipContent, type ChartConfig } from "@/components/ui/chart"
import { getStockHistory, type StockBar } from "@/features/instruments/detail/api/getStockHistory"
import StockHistoryChartSkeleton from "@/features/instruments/detail/components/StockHistoryChartSkeleton"
import { toDateOnlyString } from "@/shared/helpers/formatters"

type StockHistoryChartProps = {
    ticker: string
    currency?: string | null
}

type StockHistoryPoint = {
    date: string
    close: number
    volume: number
}

const chartConfig = {
    close: {
        label: "Close",
        color: "var(--chart-1)",
    },
    volume: {
        label: "Volume",
        color: "var(--chart-3)",
    },
} satisfies ChartConfig

export default function StockHistoryChart({ ticker, currency }: StockHistoryChartProps) {
    const [bars, setBars] = useState<StockBar[]>([])
    const [isLoading, setIsLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)

    const dateRange = useMemo(() => {
        const toDate = new Date()
        const fromDate = new Date()

        fromDate.setFullYear(fromDate.getFullYear() - 1)

        return {
            from: toDateOnlyString(fromDate),
            to: toDateOnlyString(toDate),
        }
    }, [])

    const chartData = useMemo(
        () => bars
            .map(toChartPoint)
            .sort((first, second) => first.date.localeCompare(second.date)),
        [bars]
    )

    useEffect(() => {
        const controller = new AbortController()

        async function loadHistory() {
            try {
                setIsLoading(true)
                setError(null)

                const result = await getStockHistory({
                    ticker,
                    from: dateRange.from,
                    to: dateRange.to,
                    timespan: "day",
                    signal: controller.signal,
                })

                setBars(result.results ?? [])
            } catch (error) {
                if (error instanceof DOMException && error.name === "AbortError") {
                    return
                }

                setBars([])
                setError("Failed to load stock history.")
            } finally {
                if (!controller.signal.aborted) {
                    setIsLoading(false)
                }
            }
        }

        loadHistory()

        return () => controller.abort()
    }, [dateRange.from, dateRange.to, ticker])

    if (isLoading) {
        return <StockHistoryChartSkeleton />
    }

    if (error) {
        return (
            <section className="flex flex-col gap-2">
                <h2 className="font-medium">Price history</h2>
                <p className="text-sm text-destructive">{error}</p>
            </section>
        )
    }

    if (chartData.length === 0) {
        return (
            <section className="flex flex-col gap-2">
                <h2 className="font-medium">Price history</h2>
                <p className="text-sm text-muted-foreground">No historical price bars found for {ticker}.</p>
            </section>
        )
    }

    return (
        <section className="flex flex-col gap-3">
            <div>
                <h2 className="font-medium">Price history</h2>
                <p className="text-sm text-muted-foreground">
                    Daily close and volume from {formatShortDate(dateRange.from)} to {formatShortDate(dateRange.to)}.
                </p>
            </div>

            <ChartContainer config={chartConfig} className="h-72 w-full">
                <ComposedChart accessibilityLayer data={chartData} margin={{ left: 0, right: 0, top: 8 }}>
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
                        width={56}
                        domain={["dataMin", "dataMax"]}
                        tickFormatter={(value) => formatPriceTick(value, currency)}
                    />
                    <YAxis yAxisId="volume" orientation="right" hide />
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
                        radius={[2, 2, 0, 0]}
                        opacity={0.22}
                    />
                </ComposedChart>
            </ChartContainer>
        </section>
    )
}

function toChartPoint(bar: StockBar): StockHistoryPoint {
    return {
        date: toDateOnlyString(new Date(bar.t)),
        close: bar.c,
        volume: Math.round(bar.v),
    }
}

function formatAxisDate(value: string) {
    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        year: "numeric"
    }).format(new Date(value))
}

function formatShortDate(value: string) {
    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    }).format(new Date(value))
}

function formatLongDate(value: string) {
    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    }).format(new Date(value))
}

function formatPriceTick(value: number, currency?: string | null) {
    return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: currency?.toUpperCase() ?? "USD",
        notation: "compact",
        maximumFractionDigits: 1,
    }).format(value)
}

type ChartTooltipContentPayload = ComponentProps<typeof ChartTooltipContent>["payload"]

function formatTooltipPayload(payload: ChartTooltipContentPayload, currency?: string | null): ChartTooltipContentPayload {
    return payload?.map((item) => {
        if ((item.dataKey !== "close" && item.name !== "close") || typeof item.value !== "number") {
            return item
        }

        return {
            ...item,
            value: formatCurrencyTooltipValue(item.value, currency),
        }
    }) as ChartTooltipContentPayload
}

function formatCurrencyTooltipValue(value: number, currency?: string | null) {
    return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: currency?.toUpperCase() ?? "USD",
        maximumFractionDigits: 2,
    }).format(value)
}
