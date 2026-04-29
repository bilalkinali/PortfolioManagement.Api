import { type PortfolioResponse } from '@/features/portfolios/details/api/getPortfolio';
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"

type PortfolioCardProps = {
    portfolio: PortfolioResponse
}

const currencyFormatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
})

const numberFormatter = new Intl.NumberFormat("en-US", {
    maximumFractionDigits: 4,
})

export default function PortfolioCard({ portfolio }: PortfolioCardProps) {
    return (
        <Card>
            <CardHeader className="border-b">
                <CardTitle>{portfolio.name}</CardTitle>
                {portfolio.description ? (
                    <CardDescription>{portfolio.description}</CardDescription>
                ) : null}
                <CardAction className="rounded-md bg-muted px-3 py-1 text-sm font-medium">
                    {portfolio.positions.length} positions
                </CardAction>
            </CardHeader>

            <CardContent className="px-0">
                <Table className="min-w-[760px]">
                    <TableHeader className="bg-muted/50">
                        <TableRow className="hover:bg-muted/50">
                            <TableHead className="w-[220px] px-6 font-semibold">Position</TableHead>
                            <TableHead className="text-right font-semibold">Quantity</TableHead>
                            <TableHead className="text-right font-semibold">Avg. Cost</TableHead>
                            <TableHead className="text-right font-semibold">Current Price</TableHead>
                            <TableHead className="text-right font-semibold">Market Value</TableHead>
                            <TableHead className="px-6 text-right font-semibold">Realized P/L</TableHead>
                        </TableRow>
                    </TableHeader>

                    <TableBody>
                        {portfolio.positions.map((position) => {
                            const currentPrice = 100 // Need to get price from API
                            const marketValue = position.quantity * currentPrice
                            const isProfit = position.realizedPnL >= 0

                            return (
                                <TableRow key={position.id}>
                                    <TableCell className="px-6">
                                        <div className="flex flex-col gap-1">
                                            <span className="font-semibold">{position.symbol}</span>
                                            <span className="text-xs text-muted-foreground">
                                                {position.status}
                                            </span>
                                        </div>
                                    </TableCell>

                                    <TableCell className="text-right tabular-nums">
                                        {numberFormatter.format(position.quantity)}
                                    </TableCell>

                                    <TableCell className="text-right tabular-nums">
                                        {currencyFormatter.format(position.avgCost)}
                                    </TableCell>

                                    <TableCell className="text-right tabular-nums">
                                        {currencyFormatter.format(currentPrice)}
                                    </TableCell>

                                    <TableCell className="text-right tabular-nums">
                                        {currencyFormatter.format(marketValue)}
                                    </TableCell>

                                    <TableCell className="px-6 text-right font-medium tabular-nums">
                                        <span className={isProfit ? "text-green-600" : "text-red-600"}>
                                            {currencyFormatter.format(position.realizedPnL)}
                                        </span>
                                    </TableCell>
                                </TableRow>
                            )
                        })}
                    </TableBody>
                </Table>
            </CardContent>
        </Card>
    );
}
