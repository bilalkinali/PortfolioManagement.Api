import { apiFetch } from '@/features/auth/shared/apiClient';

export async function deleteTrade(portfolioId: number, positionId: number, tradeId: number): Promise<void> {
    const response = await apiFetch(`/api/portfolios/${portfolioId}/positions/${positionId}/trades/${tradeId}`, {
        method: 'DELETE',
    });

    if (!response.ok) {
        throw new Error('Failed to delete trade');
    }
}