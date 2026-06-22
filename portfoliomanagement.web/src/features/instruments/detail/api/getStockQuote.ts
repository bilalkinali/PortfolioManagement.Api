import { apiFetch } from "@/features/auth/shared/apiClient"

export type StockQuoteResponse = {
    symbol: string
    currentPrice: number
    previousClose: number | null
    open: number | null
    high: number | null
    low: number | null
    volume: number | null
    timestampUtc: string | null
    priceDate: string | null
    currency: string | null
    source: "Live" | "LatestLoaded"
    cachedAtUtc: string
}

export async function getStockQuote(
    ticker: string,
    signal?: AbortSignal
): Promise<StockQuoteResponse> {
    const response = await apiFetch(`/api/instruments/${ticker}/quote`, {
        method: "GET",
        signal,
    })

    if (!response.ok) {
        throw new Error("Failed to fetch stock quote")
    }

    return await response.json() as StockQuoteResponse
}
