import { Skeleton } from "@/components/ui/skeleton"
import { Spinner } from "@/components/ui/spinner"

export default function StockHistoryChartSkeleton() {
    return (
        <section>
            <div className="relative">
                <Skeleton className="h-72 w-full" />

                <div className="absolute inset-0 flex items-center justify-center">
                    <Spinner />
                </div>
            </div>
        </section>
    )
}
