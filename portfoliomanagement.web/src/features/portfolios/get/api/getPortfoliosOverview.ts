import { apiFetch } from '@/features/auth/shared/apiClient';

export type PortfoliosOverviewResponse = {
    id: number;
    name: string;
    description: string;
    createdAt: string;
    positionCount: number;
    openPositionsCount: number;
    totalCostBasis: number;
    totalMarketValue: number;
    totalUnrealizedPnL: number;
    totalRealizedPnL: number;
    totalPnL: number;
    totalPnLPercentage: number;
    positions: PortfolioPositionSummaryResponse[];
}

export type PortfolioPositionSummaryResponse = {
    positionId: number;
    instrumentId: number,
    symbol: string;
    name: string;
    currency: string;
    quantity: number;
    averageCostBasis: number;
    realizedPnL: number;
    latestPrice: number;
    costBasis: number;
    marketValue: number;
    unrealizedPnL: number;
    unrealizedPnLPercentage: number;
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