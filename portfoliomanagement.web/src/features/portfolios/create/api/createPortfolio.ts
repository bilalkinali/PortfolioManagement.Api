import { apiFetch } from '@/features/auth/shared/apiClient';

export type CreatePortfolioRequest = {
    name: string;
    description: string;
};

export async function createPortfolio(request: CreatePortfolioRequest): Promise<void> {
    const response = await apiFetch('/api/portfolios', {
        method: 'POST',
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.title || 'Creation failed');
    }

    return response.json() as Promise<void>;
}
