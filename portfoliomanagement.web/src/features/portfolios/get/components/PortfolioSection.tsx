import { useEffect, useState } from "react";
import { getPortfolios, type PortfolioResponse } from '@/features/portfolios/get/api/getPortfolios'
import { useAuth } from '@/features/auth/shared/auth-context'
import { Button } from '@/components/ui/button'
import EmptyPortfolioCollection from '@/features/portfolios/get/components/EmptyPortfolioCollection'
import PortfolioCardMini from '@/features/portfolios/get/components/PortfolioCardMini'

export default function PortfolioSection() {
    const [portfolios, setPortfolios] = useState<PortfolioResponse[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    const { user } = useAuth();

    async function loadPortfolios() {
        try {
            setIsLoading(true);

            const result = await getPortfolios();
            setPortfolios(result);
        } catch {
            setPortfolios([]);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        if (!user) {
            setPortfolios([]);
            return;
        }

        loadPortfolios();
    }, [user])

    function renderContent() {
        if (!user || isLoading) {
            return null;
        }

        {/* Empty portfolios */ }
        if (portfolios.length === 0) {
            return <EmptyPortfolioCollection />
        }

        {/* Show portfolio cards */ }
        return (
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-2">
                {portfolios.map((portfolio) => (
                    <PortfolioCardMini key={portfolio.id} portfolio={portfolio} />
                ))}
            </div>
        );
    }

    return (
        <section className="mt-6">
            <div className="mb-6 flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-semibold">Your Portfolios</h1>
                    <p className="text-1xl">Manage and track your portfolios here</p>
                </div>

                <Button>New Portfolio</Button>
            </div>

            {renderContent()}
        </section>
    )
}