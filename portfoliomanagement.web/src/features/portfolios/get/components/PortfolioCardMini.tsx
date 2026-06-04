import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Link } from 'react-router'
import { type PortfolioResponse } from '../api/getPortfolios'
import { EllipsisVertical } from 'lucide-react';

type PortfolioCardMiniProps = {
    portfolio: PortfolioResponse
}

function formatDate(value: string) {
    return new Intl.DateTimeFormat("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    }).format(new Date(value));
}

export default function PortfolioCardMini({ portfolio }: PortfolioCardMiniProps) {

    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-start justify-between gap-3">
                    <span>{portfolio.name}</span>
                    <EllipsisVertical className="text-muted-foreground" />
                </CardTitle>
                <CardDescription>{portfolio.description ?? "No description"}</CardDescription>
            </CardHeader>

            <CardContent>
                <p className="text-sm text-muted-foreground">Created {formatDate(portfolio.createdAt)}</p>
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
