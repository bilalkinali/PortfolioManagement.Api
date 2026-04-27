import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { type PortfolioResponse } from '../api/getPortfolios'
import { EllipsisVertical } from 'lucide-react';

type PortfolioCardMiniProps = {
    portfolio: PortfolioResponse
}

export default function PortfolioCardMini({ portfolio }: PortfolioCardMiniProps) {

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
                        <p className="text-muted-foreground">Current value</p>
                        <h2 className="font-semibold text-2xl">$5,157.23</h2>
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
                        <p className="text-muted-foreground">Invested</p>
                        <h2 className="font-semibold text-lg">$1,000.00</h2>
                    </div>
                    <div>
                        <p className="text-right text-muted-foreground">Return</p>
                        <h2 className="font-semibold text-lg text-green-600">+$4,157.23</h2>
                    </div>
                </div>
                <p className="text-right text-green-600">415.72%</p>
           </CardContent>
        </Card>
    )
}