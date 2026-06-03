import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Link } from 'react-router'
import { type PortfoliosOverviewResponse } from '../api/getPortfoliosOverview'
import { EllipsisVertical } from 'lucide-react';
import { formatCurrency } from '@/shared/helpers/formatters';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { cn } from '@/lib/utils';

type PortfolioCardOverviewProps = {
    portfolio: PortfoliosOverviewResponse
}

function getPnLClassName(value: number | null | undefined) {
    if (value == null || value === 0) {
        return "text-muted-foreground";
    }

    return value > 0 ? "text-green-600" : "text-red-600";
}

function formatPercentage(value: number | null | undefined) {
    if (value == null) {
        return "N/A";
    }

    return `${value.toFixed(2)}%`;
}

function formatDate(value: string) {
    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    }).format(new Date(value));
}

export default function PortfolioCardOverview({ portfolio }: PortfolioCardOverviewProps) {
    const hasMissingPrices = portfolio.missingPricePositionCount > 0;

    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-start justify-between gap-3">
                    <span>{portfolio.name}</span>
                    <EllipsisVertical className="text-muted-foreground" />
                </CardTitle>
                <CardDescription>{portfolio.description ?? "No description"}</CardDescription>
                <div className="flex flex-wrap gap-2 pt-2">
                    <Badge variant="secondary">{portfolio.positionCount} positions</Badge>
                    <Badge variant="outline">{portfolio.openPositionCount} open</Badge>
                    <Badge variant="outline">Created {formatDate(portfolio.createdAt)}</Badge>
                    {hasMissingPrices ? (
                        <Badge variant="destructive">{portfolio.missingPricePositionCount} prices missing</Badge>
                    ) : null}
                </div>
            </CardHeader>

            <CardContent className="flex flex-col gap-4">
                <div className="flex items-center justify-between">
                    <div>
                        <p className="text-muted-foreground">Market Value</p>
                        <h2 className="font-semibold text-2xl">
                            {hasMissingPrices ? "Partial" : formatCurrency(portfolio.totalMarketValue)}
                        </h2>
                    </div>
                    <div>
                        <p className="text-right text-muted-foreground">Total P/L</p>
                        <h2 className={cn("font-semibold text-2xl", hasMissingPrices ? "text-muted-foreground" : getPnLClassName(portfolio.totalPnL))}>
                            {hasMissingPrices ? "Partial" : formatCurrency(portfolio.totalPnL)}
                        </h2>
                    </div>
                </div>
                <p className={cn("text-right", hasMissingPrices ? "text-muted-foreground" : getPnLClassName(portfolio.totalPnLPercentage))}>
                    {hasMissingPrices ? "Partial" : formatPercentage(portfolio.totalPnLPercentage)}
                </p>

                <Separator />

                <div className="flex items-center justify-between">
                    <div>
                        <p className="text-muted-foreground">Total Cost Basis</p>
                        <h2 className="font-semibold text-lg">{formatCurrency(portfolio.totalCostBasis)}</h2>
                    </div>
                    <div>
                        <p className="text-right text-muted-foreground">Unrealized P/L</p>
                        <h2 className={cn("font-semibold text-lg", hasMissingPrices ? "text-muted-foreground" : getPnLClassName(portfolio.totalUnrealizedPnL))}>
                            {hasMissingPrices ? "Partial" : formatCurrency(portfolio.totalUnrealizedPnL)}
                        </h2>
                    </div>
                </div>

                <div className="flex items-center justify-between">
                    <p className="text-muted-foreground">Realized P/L</p>
                    <p className={cn("font-medium", getPnLClassName(portfolio.totalRealizedPnL))}>
                        {formatCurrency(portfolio.totalRealizedPnL)}
                    </p>
                </div>

            </CardContent>
            <CardFooter className="flex justify-center">
                <Button variant="secondary" asChild>
                    <Link to={`/portfolios/${portfolio.id}`}>
                        View Details
                    </Link>
                </Button>
            </CardFooter>
        </Card>
    )
}
