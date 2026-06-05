import { apiFetch } from '@/features/auth/shared/apiClient';
import type { TradeType } from '@/features/portfolios/details/api/getPortfolio';

export type AddTradeRequest = {
    instrumentId: number;
    type: TradeType;
    shares: number;
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
}
