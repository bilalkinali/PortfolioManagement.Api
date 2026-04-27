import { apiFetch } from '@/features/auth/shared/apiClient';

export type SearchInstrumentsRequest = {
    query: string;
    limit?: number;
    type?: string;
}

export type SearchInstrumentResult = {
    symbol: string;
    name: string;
    exchange?: string;
    currency?: string;
    market?: string;
    type?: string;
    active: boolean;
}

export type SearchInstrumentsResponse = {
    results: SearchInstrumentResult[];
}

export async function searchInstruments(
    query: string,
    limit?: number,
    type?: string): Promise<SearchInstrumentResult[]> {
    const searchParams = new URLSearchParams({
        query,
    });

    if (limit) {
        searchParams.set('limit', limit.toString());
    }

    if (type) {
        searchParams.set('type', type);
    }

    const response = await apiFetch(`/api/instruments/search?${searchParams.toString()}`, {
        method: 'GET',
    });

    if (!response.ok) {
        throw new Error('Failed to search instruments');
    }

    const data = await response.json() as SearchInstrumentsResponse;
    return data.results;
}
