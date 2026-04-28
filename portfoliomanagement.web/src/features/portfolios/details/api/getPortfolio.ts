import { apiFetch } from '@/features/auth/shared/apiClient';

export type PortfolioResponse = {
    id: number;
    name: string;
    description?: string;
    createdAt: string;
}

export async function getPortfolio(portfolioId: number): Promise<PortfolioResponse> {
    const response = await apiFetch(`/api/portfolios/${portfolioId}`, {
        method: "GET",
    });

    if (!response.ok) {
        throw new Error("Failed to fetch portfolio");
    }

    return response.json() as Promise<PortfolioResponse>;
}