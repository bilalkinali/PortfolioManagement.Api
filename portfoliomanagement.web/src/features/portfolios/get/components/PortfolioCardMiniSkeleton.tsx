import { Card, CardHeader, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'

export default function PortfolioCardMiniSkeleton() {
    return (
        <Card>
            <CardHeader>
                <Skeleton className="h-5 w-full" />
                <Skeleton className="h-4 w-full" />
            </CardHeader>

            <CardContent>
                <div className="flex items-center justify-between">
                    <Skeleton className="h-12 w-1/3" />
                    <Skeleton className="h-12 w-1/3" />
                </div>

                <Skeleton className="ml-auto mt-2 h-4 w-16" />

                <div className="my-4 border-t" />

                <div className="flex items-center justify-between">
                    <Skeleton className="h-10 w-1/3" />
                    <Skeleton className="h-10 w-1/3" />
                </div>

                <Skeleton className="ml-auto mt-2 h-4 w-16" />
            </CardContent>
        </Card>
    )
}