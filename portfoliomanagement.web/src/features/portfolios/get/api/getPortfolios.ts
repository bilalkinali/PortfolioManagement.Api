export type PortfolioResponse = {
    id: number;
    name: string;
    description?: string;
    createdAt: string;
}

export async function getPortfolios(): Promise<PortfolioResponse[]> {
    const response = await fetch("/api/portfolios", {
        method: "GET",
        headers: {
            "Accept": "application/json",
        }
    });

    if (!response.ok) {
        throw new Error("Failed to fetch portfolios");
    }

    return response.json() as Promise<PortfolioResponse[]>;
}