import { apiFetch } from "@/features/auth/shared/apiClient";

export type StockProfileResponse = {
    active: boolean;
    cik: string | null;
    compositeFigi: string | null;
    currencyName: string | null;
    description: string | null;
    homepageUrl: string | null;
    listDate: string | null;
    locale: string | null;
    market: string | null;
    marketCap: number | null;
    name: string | null;
    phoneNumber: string | null;
    primaryExchange: string | null;
    roundLot: number | null;
    shareClassFigi: string | null;
    shareClassSharesOutstanding: number | null;
    sicCode: string | null;
    sicDescription: string | null;
    ticker: string | null;
    tickerRoot: string | null;
    tickerSuffix: string | null;
    totalEmployees: number | null;
    type: string | null;
    weightedSharesOutstanding: number | null;
    address: AddressResponse | null;
    branding: BrandingResponse | null;
    delistedUtc: string | null;
    lastSyncedDate: string;
};

export type AddressResponse = {
    address1: string | null;
    city: string | null;
    state: string | null;
    postalCode: string | null;
};

export type BrandingResponse = {
    iconUrl: string | null;
    logoUrl: string | null;
};

export async function getStockProfile(ticker: string): Promise<StockProfileResponse> {
    const response = await apiFetch(`/api/instruments/${ticker}`, {
        method: "GET",
    });

    if (!response.ok) {
        throw new Error("Failed to fetch stock profile");
    }

    return await response.json() as StockProfileResponse;
}