import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Link } from 'react-router'
import { type PortfoliosOverviewResponse } from '../api/getPortfoliosOverview'
import { EllipsisVertical } from 'lucide-react';
import { formatCurrency } from '@/shared/helpers/formatters';

type PortfolioCardOverviewProps = {
    portfolio: PortfoliosOverviewResponse
}

export default function PortfolioCardOverview({ portfolio }: PortfolioCardOverviewProps) {

    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex justify-between">
                    {portfolio.name}
                    <EllipsisVertical />
                </CardTitle>
                <CardDescription>{portfolio.description}</CardDescription>
            </CardHeader>

            <CardContent>
                <div className="flex items-center justify-between">
                    <div>
                        <p className="text-muted-foreground">Market Value</p>
                        <h2 className="font-semibold text-2xl">{formatCurrency(portfolio.totalMarketValue)}</h2>
                    </div>
                    <div>
                        <p className="text-right text-muted-foreground">Day P/L</p>
                        <h2 className="font-semibold text-2xl text-green-600">+$253.89</h2>
                    </div>
                </div>
                <p className="text-right text-green-600">5.17%</p>

                <div className="mt-4 mb-4 border-t" /> {/* Horizontal Line*/}

                <div className="flex items-center justify-between">
                    <div>
                        <p className="text-muted-foreground">Total Cost Basis</p>
                        <h2 className="font-semibold text-lg">{formatCurrency(portfolio.totalCostBasis)}</h2>
                    </div>
                    <div>
                        <p className="text-right text-muted-foreground">Return</p>
                        <h2 className={portfolio.totalPnL >= 0
                            ? "font-semibold text-lg text-green-600"
                            : "font-semibold text-lg text-red-600"}>
                            {formatCurrency(portfolio.totalPnL)}
                        </h2>
                    </div>
                </div>
                <p className={portfolio.totalPnLPercentage >= 0
                    ? "text-right text-green-600"
                    : "text-right text-red-600"}>
                    {portfolio.totalPnLPercentage.toFixed(2)}%
                </p>
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
