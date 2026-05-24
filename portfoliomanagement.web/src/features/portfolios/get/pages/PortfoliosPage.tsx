//import { Card, CardHeader, CardDescription, CardContent } from "@/components/ui/card";
import { useState, useEffect } from "react";
import EmptyPortfolioCollection from "../components/EmptyPortfolioCollection";
import { getPortfoliosOverview, type PortfoliosOverviewResponse } from "../api/getPortfoliosOverview";
import PortfolioCardMiniSkeleton from "../components/PortfolioCardMiniSkeleton";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbSeparator
} from "@/components/ui/breadcrumb"
import PortfolioCardOverview from "../components/PortfolioCardOverview";

export default function PortfoliosPage() {
    const [portfolios, setPortfolios] = useState<PortfoliosOverviewResponse[]>([])
    const [isLoading, setIsLoading] = useState(true)

    useEffect(() => {

        async function loadPortfolios() {
            try {
                setIsLoading(true)
                const result = await getPortfoliosOverview()

                console.log(JSON.stringify(result, null, 2))

                setPortfolios(result)
            } catch {
                setPortfolios([])
            } finally {
                setIsLoading(false)
            }
        }

        loadPortfolios()

    }, [])

    if (isLoading) {
        return <PortfolioCardMiniSkeleton />
    }

    if (!portfolios) {
        return <p>Portfolios not found.</p>
    }

    if (portfolios.length === 0) {
        return <EmptyPortfolioCollection />
    }

    return (
        <>
            <Breadcrumb className="mb-4">
                <BreadcrumbList>
                    <BreadcrumbItem>
                        <BreadcrumbLink href="/">Home</BreadcrumbLink>
                    </BreadcrumbItem>
                    <BreadcrumbSeparator />
                    <BreadcrumbItem>
                        Portfolios
                    </BreadcrumbItem>
                </BreadcrumbList>
            </Breadcrumb>

            <div className="grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-2">
                {portfolios.length > 0 && portfolios.map((portfolio) => (
                    <PortfolioCardOverview key={portfolio.id} portfolio={portfolio} />
                ))}
            </div >
        </>
    );
}