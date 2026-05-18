import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function StockProfileCardSkeleton() {
    return (
        <Card>
            <CardHeader className="flex space-y-2">
                <div className="space-y-2">
                    <Skeleton className="h-20 w-30" />
                </div>
                <div className="space-y-2">
                    <Skeleton className="h-10 w-60" />
                    <Skeleton className="h-8 w-50" />
                </div>
            </CardHeader>

            <CardContent className="space-y-18">                
                <Skeleton className="mr-50 h-80 w-full" />

                <div className="grid gap-4 md:grid-cols-3">
                    <Skeleton className="h-22 w-full rounded-md" />
                    <Skeleton className="h-22 w-full rounded-md" />
                    <Skeleton className="h-22 w-full rounded-md" />
                </div>

                <div className="space-y-3">
                    <div className="space-y-4">
                        <Skeleton className="h-50 w-full" />
                    </div>
                </div>
                
            </CardContent>
        </Card>
    );
}