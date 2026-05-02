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
    quantity: number;
    avgCost: number;
    realizedPnL: number;
    status: string;
    openDate: string;
    closeDate: string | null;
    instrumentId: number;
    symbol: string;
    name: string;
    currency: string | null;
    exchange: string | null;
    latestPrice: number | null;
    latestPriceDate: string | null;
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
            currency: position.currency ?? null,
            exchange: position.exchange ?? null,
            trades: position.trades,
        })),
    };
}
