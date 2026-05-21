import * as React from "react"
import { ChevronDownIcon, ChevronRightIcon } from "lucide-react"
import { type PortfolioResponse } from '@/features/portfolios/details/api/getPortfolio';
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import AddTradeDialog from "@/features/trades/add/components/AddTradeDialog"
import PositionTradesDataTable from "@/features/portfolios/details/components/PositionTradesDataTable"
import { formatCurrency } from "@/shared/helpers/formatters"
import { Wallet, LineChart, TrendingUp, TrendingDown } from 'lucide-react';

type PortfolioCardProps = {
    portfolio: PortfolioResponse;
    onSuccess: () => void;
}

const numberFormatter = new Intl.NumberFormat("en-US", {
    maximumFractionDigits: 4,
})

const dateFormatter = new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
})

export default function PortfolioCard({ portfolio, onSuccess }: PortfolioCardProps) {
    const [expandedPositionIds, setExpandedPositionIds] = React.useState<Set<number>>(() => new Set())

    function togglePosition(positionId: number) {
        setExpandedPositionIds((current) => {
            const next = new Set(current)

            if (next.has(positionId)) {
                next.delete(positionId)
            } else {
                next.add(positionId)
            }

            return next
        })
    }

    const costBasis = portfolio.positions.reduce(
        (sum, position) =>
            sum + Math.abs(position.quantity) * position.averageCostBasis,
        0
    );

    const marketValue = portfolio.positions.reduce(
        (sum, position) => sum + position.quantity * (position.latestPrice ?? 0),
        0
    );

    const profitLoss = marketValue - costBasis;

    return (
        <Card className="gap-2">
            <CardHeader>
                <div>
                    <CardTitle>{portfolio.name}</CardTitle>
                    <CardDescription>{portfolio.description}</CardDescription>
                </div>
                <CardAction>
                    <Badge variant="secondary">
                        {dateFormatter.format(new Date(portfolio.createdAt))}
                    </Badge>                    
                </CardAction>
            </CardHeader>
            
            <CardContent>
                <div className="grid grid-cols-1 gap-6 pt-4 sm:grid-cols-3 mb-6 text-center">
                    <div className="relative grid min-h-36 place-items-center rounded-md bg-muted p-4">
                        <Wallet className="text-blue-600 absolute h-5 w-5 right-4 top-4" />
                        <div>
                            <h1 className="text-sm font-semibold">Invested</h1>
                            <p className="mt-1 text-xl font-semibold tabular-nums">
                                {formatCurrency(costBasis)}
                            </p>
                        </div>
                    </div>

                    <div className="relative grid min-h-36 place-items-center rounded-md bg-muted p-4">
                        <LineChart className="text-blue-600 h-5 w-5 absolute right-4 top-4" />
                        <div>
                            <h1 className="text-sm font-semibold">Market Value</h1>
                            <p className="mt-1 text-xl font-semibold tabular-nums">
                                {formatCurrency(marketValue)}
                            </p>
                        </div>
                    </div>

                    <div className="relative grid min-h-36 place-items-center rounded-md bg-muted p-4">
                        {profitLoss >= 0 ? (
                            <TrendingUp className="h-5 w-5 absolute right-4 top-4 text-green-600" />
                        ) : (
                            <TrendingDown className="h-5 w-5 absolute right-4 top-4 text-red-600" />
                        )}                        
                        <div>
                            <h1 className="text-sm font-semibold">Profit / Loss</h1>
                            <p className="mt-1 text-xl font-semibold tabular-nums">
                                <span className={profitLoss >= 0 ? "text-green-600" : "text-red-600"}>
                                    {formatCurrency(profitLoss)}
                                </span>
                            </p>
                        </div>
                    </div>

                </div>
                <div className="mb-2 text-right">
                    <AddTradeDialog
                        portfolioId={portfolio.id}
                        onSuccess={onSuccess}
                        buttonVariant="default"
                        buttonSize="sm"
                        buttonText="Add Trade" />
                </div>
                <Table className="px-0 min-w-[760px]">
                    <TableHeader className="bg-muted/50">
                        <TableRow className="hover:bg-muted/50">
                            <TableHead className="w-12 px-6">
                                <span className="sr-only">Expand</span>
                            </TableHead>
                            <TableHead className="w-[220px] px-6 font-semibold">Position</TableHead>
                            <TableHead className="text-right font-semibold">Quantity</TableHead>
                            <TableHead className="text-right font-semibold">Average Cost Basis</TableHead>
                            <TableHead className="text-right font-semibold">Invested</TableHead>
                            <TableHead className="text-right font-semibold">Market Value</TableHead>
                            <TableHead className="text-right font-semibold">Realized P/L</TableHead>
                            <TableHead className="px-6 text-right font-semibold">Current Price</TableHead>                           
                        </TableRow>
                    </TableHeader>

                    <TableBody>
                        {portfolio.positions.map((position) => {
                            const costBasis = Math.abs(position.quantity) * position.averageCostBasis;
                            const marketValue = position.quantity * (position.latestPrice ?? 0)
                            const isProfit = position.realizedPnL >= 0
                            const isExpanded = expandedPositionIds.has(position.id)

                            return (
                                <React.Fragment key={position.id}>
                                    <TableRow>
                                        <TableCell className="px-6">
                                            <Button
                                                variant="ghost"
                                                size="icon-sm"
                                                aria-label={`${isExpanded ? "Collapse" : "Expand"} ${position.symbol} trades`}
                                                aria-expanded={isExpanded}
                                                onClick={() => togglePosition(position.id)}
                                            >
                                                {isExpanded ? (
                                                    <ChevronDownIcon />
                                                ) : (
                                                    <ChevronRightIcon />
                                                )}
                                            </Button>
                                        </TableCell>

                                        <TableCell className="px-6">
                                            <div className="flex flex-col">
                                                <span className="font-semibold">{position.symbol}</span>
                                                <span className="text-xs text-muted-foreground">
                                                    {position.name}
                                                </span>
                                            </div>
                                        </TableCell>

                                        <TableCell className="text-right tabular-nums">
                                            {numberFormatter.format(position.quantity)}
                                        </TableCell>

                                        <TableCell className="text-right tabular-nums">
                                            {formatCurrency(position.averageCostBasis, position.currency)}
                                        </TableCell>

                                        <TableCell className="text-right tabular-nums">
                                            {formatCurrency(costBasis, position.currency)}
                                        </TableCell>

                                        <TableCell className="text-right tabular-nums">
                                            {formatCurrency(marketValue, position.currency)}
                                        </TableCell>

                                        <TableCell className="text-right font-medium tabular-nums">
                                            <span className={isProfit ? "text-green-600" : "text-red-600"}>
                                                {formatCurrency(position.realizedPnL, position.currency)}
                                            </span>
                                        </TableCell>

                                        <TableCell className="px-6 text-right tabular-nums">
                                            <div className="flex flex-col">
                                                <span>
                                                    {position.latestPrice !== null
                                                        ? formatCurrency(position.latestPrice, position.currency)
                                                        : "—"}
                                                </span>
                                                {position.latestPriceDate ? (
                                                    <span className="text-xs text-muted-foreground">
                                                        {dateFormatter.format(new Date(position.latestPriceDate))}
                                                    </span>
                                                ) : null}
                                            </div>
                                        </TableCell>
                                        
                                    </TableRow>

                                    {isExpanded ? (
                                        <TableRow className="bg-muted/10 hover:bg-muted/10">
                                            <TableCell colSpan={8} className="px-6 pb-5 pt-0">
                                                <PositionTradesDataTable
                                                    portfolioId={portfolio.id}
                                                    position={position}
                                                    onSuccess={onSuccess}
                                                />
                                            </TableCell>
                                        </TableRow>
                                    ) : null}
                                </React.Fragment>
                            )
                        })}
                    </TableBody>
                </Table>
            </CardContent>
        </Card>
    );
}
