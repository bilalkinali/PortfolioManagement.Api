import { apiFetch } from '@/features/auth/shared/apiClient';

export type AddTradeRequest = {
    instrumentId: number;
    quantity: number;
    price: number;
    executedDate: string;
};

export async function addTrade(request: AddTradeRequest, portfolioId: number): Promise<void> {
    const response = await apiFetch(`/api/portfolios/${portfolioId}/trades`, {
        method: 'POST',
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.title || 'Trade addition failed');
    }

    return response.json() as Promise<void>;
}
