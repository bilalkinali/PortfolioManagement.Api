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

export function toDateOnlyString(date: Date) {
    const year = date.getFullYear()
    const month = String(date.getMonth() + 1).padStart(2, "0")
    const day = String(date.getDate()).padStart(2, "0")

    return `${year}-${month}-${day}`
}

export function fromDateOnlyString(value?: string | null) {
    if (!value) {
        return undefined
    }

    const [year, month, day] = value.split("-").map(Number)

    if (!year || !month || !day) {
        return undefined
    }

    return new Date(year, month - 1, day)
}