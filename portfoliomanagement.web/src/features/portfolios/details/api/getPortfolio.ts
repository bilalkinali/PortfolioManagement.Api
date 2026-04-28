import { apiFetch } from '@/features/auth/shared/apiClient';

export type PortfolioResponse = {
    id: number;
    name: string;
    description: string | null;
    createdAt: string;
    positions: PortfolioPositionResponse[];
}

export type PortfolioPositionResponse = {
    id: number;
    symbol: string;
    quantity: number;
    avgCost: number;
    realizedPnL: number;
    status: string;
    openDate: string;
    closeDate: string | null;
    instrumentId: number | null;
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

    const portfolio = await response.json() as PortfolioResponse;

    return {
        ...portfolio,
        description: portfolio.description ?? null,
        positions: portfolio.positions.map((position) => ({
            ...position,
            closeDate: position.closeDate ?? null,
            instrumentId: position.instrumentId ?? null,
            trades: position.trades,
        })),
    };
}
