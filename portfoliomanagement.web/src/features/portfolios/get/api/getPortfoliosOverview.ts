import { apiFetch } from '@/features/auth/shared/apiClient';

export type PortfoliosOverviewResponse = {
    id: number;
    name: string;
    description: string | null;
    createdAt: string;
    positionCount: number;
    openPositionCount: number;
    totalCostBasis: number;
    totalMarketValue: number;
    totalUnrealizedPnL: number;
    totalRealizedPnL: number;
    totalPnL: number;
    totalPnLPercentage: number;
    missingPricePositionCount: number;
    positions: PortfolioPositionSummaryResponse[];
}

export type PortfolioPositionSummaryResponse = {
    positionId: number;
    instrumentId: number,
    symbol: string;
    name: string;
    currency: string | null;
    quantity: number;
    averageCostBasis: number;
    realizedPnL: number;
    latestPrice: number | null;
    costBasis: number;
    marketValue: number | null;
    unrealizedPnL: number | null;
    unrealizedPnLPercentage: number | null;
    allocationPercentage: number | null;
    status: string;
}

export async function getPortfoliosOverview(): Promise<PortfoliosOverviewResponse[]> {
    const response = await apiFetch("/api/portfolios/overview", {
        method: "GET",
    });

    if (!response.ok) {
        throw new Error("Failed to fetch portfolios");
    }

    return response.json() as Promise<PortfoliosOverviewResponse[]>;
}
