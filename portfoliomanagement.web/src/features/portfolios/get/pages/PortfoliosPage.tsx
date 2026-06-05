import { useState, useEffect, useCallback } from "react";
import EmptyPortfolioCollection from "../components/EmptyPortfolioCollection";
import { getPortfoliosOverview, type PortfoliosOverviewResponse } from "../api/getPortfoliosOverview";
import PortfolioCardMiniSkeleton from "../components/PortfolioCardMiniSkeleton";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbSeparator
} from "@/components/ui/breadcrumb"
import PortfolioCardOverview from "../components/PortfolioCardOverview";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatCurrency } from "@/shared/helpers/formatters";
import { cn } from "@/lib/utils";

function getPnLClassName(value: number) {
    if (value === 0) {
        return "text-muted-foreground";
    }

    return value > 0 ? "text-chart-1" : "text-destructive";
}

function formatPercentage(value: number) {
    return `${value.toFixed(2)}%`;
}

export default function PortfoliosPage() {
    const [portfolios, setPortfolios] = useState<PortfoliosOverviewResponse[]>([])
    const [isLoading, setIsLoading] = useState(true)

    const loadPortfolios = useCallback(async () => {
        try {
            setIsLoading(true)
            const result = await getPortfoliosOverview()

            setPortfolios(result)
        } catch {
            setPortfolios([])
        } finally {
            setIsLoading(false)
        }
    }, [])

    useEffect(() => {
        loadPortfolios()
    }, [loadPortfolios])

    if (isLoading) {
        return <PortfolioCardMiniSkeleton />
    }

    if (!portfolios) {
        return <p>Portfolios not found.</p>
    }

    if (portfolios.length === 0) {
        return <EmptyPortfolioCollection onSuccess={loadPortfolios} />
    }

    const summary = portfolios.reduce(
        (totals, portfolio) => ({
            totalCostBasis: totals.totalCostBasis + portfolio.totalCostBasis,
            totalMarketValue: totals.totalMarketValue + portfolio.totalMarketValue,
            totalRealizedPnL: totals.totalRealizedPnL + portfolio.totalRealizedPnL,
            totalUnrealizedPnL: totals.totalUnrealizedPnL + portfolio.totalUnrealizedPnL,
            totalPnL: totals.totalPnL + portfolio.totalPnL,
            openPositionCount: totals.openPositionCount + portfolio.openPositionCount,
            missingPricePositionCount: totals.missingPricePositionCount + portfolio.missingPricePositionCount,
        }),
        {
            totalCostBasis: 0,
            totalMarketValue: 0,
            totalRealizedPnL: 0,
            totalUnrealizedPnL: 0,
            totalPnL: 0,
            openPositionCount: 0,
            missingPricePositionCount: 0,
        }
    )
    const totalPnLPercentage = summary.totalCostBasis > 0
        ? summary.totalPnL / summary.totalCostBasis * 100
        : 0
    const hasMissingPrices = summary.missingPricePositionCount > 0

    return (
        <>
            <Breadcrumb className="mb-4">
                <BreadcrumbList>
                    <BreadcrumbItem>
                        <BreadcrumbLink href="/">Home</BreadcrumbLink>
                    </BreadcrumbItem>
                    <BreadcrumbSeparator />
                    <BreadcrumbItem>
                        Portfolios
                    </BreadcrumbItem>
                </BreadcrumbList>
            </Breadcrumb>

            <div className="mb-6 grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4">
                <SummaryCard
                    label="Market Value"
                    value={hasMissingPrices ? "Partial" : formatCurrency(summary.totalMarketValue)}
                    detail={hasMissingPrices ? `${summary.missingPricePositionCount} prices missing` : undefined}
                />
                <SummaryCard label="Cost Basis" value={formatCurrency(summary.totalCostBasis)} />
                <SummaryCard
                    label="Total P/L"
                    value={hasMissingPrices ? "Partial" : formatCurrency(summary.totalPnL)}
                    valueClassName={hasMissingPrices ? "text-muted-foreground" : getPnLClassName(summary.totalPnL)}
                />
                <SummaryCard
                    label="Total P/L %"
                    value={hasMissingPrices ? "Partial" : formatPercentage(totalPnLPercentage)}
                    valueClassName={hasMissingPrices ? "text-muted-foreground" : getPnLClassName(totalPnLPercentage)}
                />
                <SummaryCard label="Portfolios" value={portfolios.length.toString()} />
                <SummaryCard label="Open Positions" value={summary.openPositionCount.toString()} />
                <SummaryCard
                    label="Realized P/L"
                    value={formatCurrency(summary.totalRealizedPnL)}
                    valueClassName={getPnLClassName(summary.totalRealizedPnL)}
                />
                <SummaryCard
                    label="Unrealized P/L"
                    value={hasMissingPrices ? "Partial" : formatCurrency(summary.totalUnrealizedPnL)}
                    valueClassName={hasMissingPrices ? "text-muted-foreground" : getPnLClassName(summary.totalUnrealizedPnL)}
                />
            </div>

            <div className="grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-2">
                {portfolios.length > 0 && portfolios.map((portfolio) => (
                    <PortfolioCardOverview key={portfolio.id} portfolio={portfolio} />
                ))}
            </div >
        </>
    );
}

type SummaryCardProps = {
    label: string;
    value: string;
    detail?: string;
    valueClassName?: string;
    detailClassName?: string;
}

function SummaryCard({ label, value, detail, valueClassName, detailClassName }: SummaryCardProps) {
    return (
        <Card>
            <CardHeader>
                <CardTitle className="text-sm font-medium text-muted-foreground">{label}</CardTitle>
            </CardHeader>
            <CardContent>
                <p className={cn("text-2xl font-semibold", valueClassName)}>{value}</p>
                {detail ? (
                    <p className={cn("text-sm text-muted-foreground", detailClassName)}>{detail}</p>
                ) : null}
            </CardContent>
        </Card>
    )
}
