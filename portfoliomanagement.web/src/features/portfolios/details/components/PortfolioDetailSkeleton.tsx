import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function PortfolioDetailSkeleton() {
    return (
        <Card>
            <CardHeader className="space-y-2">
                <div className="space-y-2">
                    <Skeleton className="h-5 w-36" />
                    <Skeleton className="h-4 w-52" />
                </div>
            </CardHeader>

            <CardContent className="space-y-18">
                <div className="grid gap-4 md:grid-cols-3">
                    <Skeleton className="h-22 w-full rounded-md" />
                    <Skeleton className="h-22 w-full rounded-md" />
                    <Skeleton className="h-22 w-full rounded-md" />
                </div>

                <div className="space-y-3">
                    <div className="grid grid-cols-6 gap-4 border-b pb-3">
                        <Skeleton className="h-4 w-24" />
                        <Skeleton className="h-4 w-20" />
                        <Skeleton className="h-4 w-20" />
                        <Skeleton className="h-4 w-20" />
                        <Skeleton className="h-4 w-24" />
                        <Skeleton className="h-4 w-24" />
                    </div>

                    <div className="space-y-4">
                        <Skeleton className="h-10 w-full" />
                        <Skeleton className="h-10 w-full" />
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}