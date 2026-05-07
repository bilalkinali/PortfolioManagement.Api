import { apiFetch } from '@/features/auth/shared/apiClient';

export type EditTradeRequest = {
    quantity: number;
    price: number;
    executedDate: string;
}

export async function editTrade(
    request: EditTradeRequest,
    portfolioId: number,
    positionId: number,
    tradeId: number): Promise<void> {
    const response = await apiFetch(`/api/portfolios/${portfolioId}/positions/${positionId}/trades/${tradeId}`, {
        method: 'PUT',
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        throw new Error('Failed to update trade');
    }
}