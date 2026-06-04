import { apiFetch } from "@/features/auth/shared/apiClient"

export type GetStockHistoryResponse = {
    adjusted: boolean
    next_url: string | null
    queryCount: number
    request_id: string | null
    resultsCount: number
    status: string
    ticker: string
    results: StockBar[] | null
}

export type StockBar = {
    c: number
    h: number
    l: number
    n: number | null
    o: number
    t: number
    v: number
    vw: number | null
}

type GetStockHistoryParams = {
    ticker: string
    from: string
    to: string
    timespan?: string
    range?: string
    signal?: AbortSignal
}

export async function getStockHistory({
    ticker,
    from,
    to,
    timespan = "day",
    range,
    signal,
}: GetStockHistoryParams): Promise<GetStockHistoryResponse> {
    const searchParams = new URLSearchParams({
        from,
        to,
        timespan,
    })

    if (range) {
        searchParams.set("range", range)
    }

    const response = await apiFetch(`/api/instruments/${ticker}/history?${searchParams.toString()}`, {
        method: "GET",
        signal,
    })

    if (!response.ok) {
        throw new Error("Failed to fetch stock history")
    }

    return await response.json() as GetStockHistoryResponse
}
