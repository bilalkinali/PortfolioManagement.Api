export function formatCurrency(value: number, currency?: string | null) {
    return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: currency?.toUpperCase() ?? "USD",
    }).format(value)
}

export function formatExchangeName(exchangeCode?: string | null) {
    if (!exchangeCode) {
        return null
    }

    const exchangeNames: Record<string, string> = {
        XNAS: "Nasdaq",
        XNYS: "NYSE",
        ARCX: "NYSE Arca",
        XASE: "NYSE American",
        XCSE: "Nasdaq Copenhagen",
        XSTO: "Nasdaq Stockholm",
        XHEL: "Nasdaq Helsinki",
    }

    return exchangeNames[exchangeCode.toUpperCase()] ?? exchangeCode
}