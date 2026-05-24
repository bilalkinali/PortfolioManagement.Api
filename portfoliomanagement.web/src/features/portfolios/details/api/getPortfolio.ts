import { apiFetch } from '@/features/auth/shared/apiClient';

export type PortfolioResponse = {
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
    positions: PortfolioPositionResponse[];
}

export type PortfolioPositionResponse = {
    id: number;
    instrumentId: number;
    symbol: string;
    name: string;
    currency: string | null;
    quantity: number;
    averageCostBasis: number;
    realizedPnL: number;
    latestPrice: number | null;
    latestPriceDate: string | null;
    costBasis: number;
    marketValue: number | null;
    unrealizedPnL: number | null;
    unrealizedPnLPercentage: number | null;
    status: string;
    trades: PortfolioTradeResponse[];
}

export type PortfolioTradeResponse = {
    id: number;
    isBuy: boolean;
    quantity: number;
    price: number;
    executedDate: string;
}

export async function getPortfolio(portfolioId: number): Promise<PortfolioResponse> {
    const response = await apiFetch(`/api/portfolios/${portfolioId}`, {
        method: "GET",
    });

    if (!response.ok) {
        throw new Error("Failed to fetch portfolio");
    }

    return await response.json() as PortfolioResponse;
}