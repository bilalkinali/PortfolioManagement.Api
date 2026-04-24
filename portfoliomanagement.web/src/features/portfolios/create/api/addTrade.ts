export type AddTradeRequest = {
    symbol: string;
    quantity: number;
    price: number;
    executedDate: string;
};

export async function addTrade(portfolioId: string, request: AddTradeRequest): Promise<void> {
    const response = await fetch(`/api/portfolios/${portfolioId}/trades`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.title || 'Trade addition failed');
    }

    return response.json() as Promise<void>;
}
