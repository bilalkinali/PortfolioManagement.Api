import { SearchIcon } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import AppLayout from '@/app/layout/AppLayout'
import { Button } from '@/components/ui/button'
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card'
import { Field, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'
import { Spinner } from '@/components/ui/spinner'
import {
    searchInstruments,
    type SearchInstrumentResult,
} from '@/features/instruments/searchInstruments/api/searchInstruments'
import PortfolioSection from '@/features/portfolios/get/components/PortfolioSection'

function App() {
    const [query, setQuery] = useState('')
    const [results, setResults] = useState<SearchInstrumentResult[]>([])
    const [hasSearched, setHasSearched] = useState(false)
    const [isSearching, setIsSearching] = useState(false)
    const [error, setError] = useState<string | null>(null)

    async function handleInstrumentSearch(event: FormEvent<HTMLFormElement>) {
        event.preventDefault()

        const trimmedQuery = query.trim()

        if (!trimmedQuery) {
            setResults([])
            setHasSearched(false)
            setError(null)
            return
        }

        try {
            setIsSearching(true)
            setError(null)
            setHasSearched(true)

            const instruments = await searchInstruments(trimmedQuery, 8)
            setResults(instruments)
        } catch {
            setResults([])
            setError('Search failed. Try another symbol or name.')
        } finally {
            setIsSearching(false)
        }
    }

    return (
        <AppLayout>
            <section className="mt-6">
                <Card>
                    <CardHeader>
                        <CardTitle>Find instruments</CardTitle>
                        <CardDescription>
                            Search by ticker or company name before adding trades.
                        </CardDescription>
                    </CardHeader>

                    <CardContent className="flex flex-col gap-4">
                        <form onSubmit={handleInstrumentSearch}>
                            <FieldGroup>
                                <Field>
                                    <FieldLabel htmlFor="instrument-search">Instrument search</FieldLabel>
                                    <div className="flex flex-col gap-2 sm:flex-row">
                                        <Input
                                            id="instrument-search"
                                            value={query}
                                            onChange={(event) => setQuery(event.target.value)}
                                            placeholder="Search Apple, MSFT, Novo..."
                                            disabled={isSearching}
                                        />
                                        <Button type="submit" disabled={isSearching || !query.trim()}>
                                            {isSearching ? (
                                                <Spinner data-icon="inline-start" />
                                            ) : (
                                                <SearchIcon data-icon="inline-start" />
                                            )}
                                            Search
                                        </Button>
                                    </div>
                                </Field>
                            </FieldGroup>
                        </form>

                        {renderInstrumentSearchContent()}
                    </CardContent>
                </Card>
            </section>

            <PortfolioSection />
        </AppLayout>
    );

    function renderInstrumentSearchContent() {
        if (isSearching) {
            return (
                <div className="flex flex-col gap-3">
                    {Array.from({ length: 3 }).map((_, index) => (
                        <Skeleton key={index} className="h-16 w-full" />
                    ))}
                </div>
            )
        }

        if (error) {
            return <p className="text-sm text-destructive">{error}</p>
        }

        if (!hasSearched) {
            return null
        }

        if (results.length === 0) {
            return <p className="text-sm text-muted-foreground">No instruments found.</p>
        }

        return (
            <div className="flex flex-col gap-0 rounded-lg border">
                {results.map((instrument, index) => (
                    <div key={`${instrument.symbol}-${instrument.exchange ?? index}`}>
                        <div className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between">
                            <div className="min-w-0">
                                <div className="flex items-center gap-2">
                                    <span className="font-mono text-sm font-semibold">
                                        {instrument.symbol}
                                    </span>
                                    <span className="text-sm text-muted-foreground">
                                        {instrument.exchange ?? instrument.market ?? 'Unknown market'}
                                    </span>
                                </div>
                                <p className="truncate font-medium">{instrument.name}</p>
                            </div>

                            <div className="flex flex-wrap gap-2 text-xs text-muted-foreground">
                                {instrument.currency && <span>{instrument.currency}</span>}
                                {instrument.type && <span>{instrument.type}</span>}
                                <span>{instrument.active ? 'Active' : 'Inactive'}</span>
                            </div>
                        </div>

                        {index < results.length - 1 && <Separator />}
                    </div>
                ))}
            </div>
        )
    }
}

export default App
