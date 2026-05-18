import { Skeleton } from "@/components/ui/skeleton"

export default function StockHistoryChartSkeleton() {
    return (
        <section className="flex flex-col gap-3">
            <div className="flex flex-col gap-1">
                <Skeleton className="h-5 w-40" />
                <Skeleton className="h-4 w-64" />
            </div>
            <Skeleton className="h-72 w-full" />
        </section>
    )
}
