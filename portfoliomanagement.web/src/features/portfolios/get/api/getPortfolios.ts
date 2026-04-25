import { apiFetch } from '@/features/auth/shared/apiClient';

export type PortfolioResponse = {
    id: number;
    name: string;
    description?: string;
    createdAt: string;
}

export async function getPortfolios(): Promise<PortfolioResponse[]> {
    const response = await apiFetch("/api/portfolios", {
        method: "GET",
    });

    if (!response.ok) {
        throw new Error("Failed to fetch portfolios");
    }

    return response.json() as Promise<PortfolioResponse[]>;
}