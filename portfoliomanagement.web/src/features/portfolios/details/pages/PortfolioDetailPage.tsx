import { useEffect, useState } from 'react'
import { useParams } from 'react-router'
import EmptyPortfolio from '@/features/portfolios/details/components/EmptyPortfolio'
import { getPortfolio, type PortfolioResponse } from '@/features/portfolios/details/api/getPortfolio'
import PortfolioCard from '../components/PortfolioCard'

export default function PortfolioDetailPage() {
    const { portfolioId } = useParams<{ portfolioId: string }>()
    const [portfolio, setPortfolio] = useState<PortfolioResponse | null>(null)
    const [isLoading, setIsLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)
    const [refreshKey, setRefreshKey] = useState(0)

    useEffect(() => {

        /// DEBUG ///
        console.log("PortfolioDetailPage useEffect just ran", {
            portfolioId,
            refreshKey
        })
        /// DEBUG ///

        const id = Number(portfolioId)

        if (!Number.isInteger(id)) {
            setPortfolio(null)
            setError('Invalid portfolio id.')
            setIsLoading(false)
            return
        }

        let ignore = false

        async function loadPortfolio() {
            try {
                setIsLoading(true)
                setError(null)

                const result = await getPortfolio(id)

                console.log(JSON.stringify(result, null, 2))

                if (!ignore) {
                    setPortfolio(result)
                }
            } catch {
                if (!ignore) {
                    setPortfolio(null)
                    setError('Failed to fetch portfolio.')
                }
            } finally {
                if (!ignore) {
                    setIsLoading(false)
                }
            }
        }

        loadPortfolio()

        return () => {
            ignore = true
        }
    }, [portfolioId, refreshKey])

    if (isLoading) {
        return <p>Loading portfolio...</p>
    }

    if (error) {
        return <p>{error}</p>
    }

    if (!portfolio) {
        return <p>Portfolio not found.</p>
    }

    return (
        <>
            <PortfolioCard portfolio={portfolio} />

            {portfolio.positions.length === 0 ? (
                <EmptyPortfolio
                    portfolioId={portfolio.id}
                    onSuccess={() => setRefreshKey((current) => current + 1)}
                />
            ) : null}
        </>
    )
}