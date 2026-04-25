import { useEffect, useState } from 'react'
import AppLayout from '@/app/layout/AppLayout'
import { Button } from '@/components/ui/button'
import { Wallet } from 'lucide-react'
import CreatePortfolioDialog from '@/features/portfolios/create/components/CreatePortfolioDialog'
import { getPortfolios, type PortfolioResponse } from '../features/portfolios/get/api/getPortfolios'
import { useAuth } from '@/features/auth/shared/auth-context'


function App() {
    const [portfolios, setPortfolios] = useState<PortfolioResponse[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    //const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const { user } = useAuth();

    async function loadPortfolios() {
        try {
            setIsLoading(true);
            //setErrorMessage(null);

            const result = await getPortfolios();

            setPortfolios(result);
        } catch {
            //setErrorMessage("Failed to load portfolios");
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


    return (
        <AppLayout>
            <h1 className="text-3xl font-semibold mb-6">Portfolio Management</h1>

            <Button>Test</Button>

            <CreatePortfolioDialog onSuccess={() => console.log("Portfolio created")} />

            {/* Portfolio section */}
            <section className="mt-6">
                <div className="mb-6 flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-semibold">Your Portfolios</h1>
                        <p className="text-1xl">Manage and track your portfolios here</p>
                    </div>

                    <Button>New Portfolio</Button>
                </div>
                {renderPortfolioContent()}
            </section>

        </AppLayout>
    );

    function renderPortfolioContent() {
        if (!user || isLoading) {
            return null;
        }

        {/* Empty portfolios */ }
        if (portfolios.length === 0) {
            return (
                <div className="rounded-xl border border-gray-200 bg-white p-12 text-center">
                    <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
                        <Wallet className="h-8 w-8 text-gray-400" />
                    </div>

                    <h3 className="font-semibold text-gray-800">No portfolios yet</h3>

                    <p className="text-gray-800 mb-4">
                        Create your first portfolio to start tracking investments
                    </p>

                    <Button>Create Portfolio</Button>
                </div>
            );
        }

        {/* Show portfolio cards */ }
        return (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-6">
                {portfolios.map((portfolio) => (
                    <div
                        className="rounded-xl border bg-white p-6"
                        key={portfolio.id}
                    >
                        <h3 className="font-semibold">{portfolio.name}</h3>
                        <p>{portfolio.description}</p>

                        <div className="mt-4 flex items-center justify-between">
                            <div>
                                <p>Current value</p>
                                <h2 className="font-semibold text-2xl">$5,157.23</h2>                                
                            </div>
                            <div>
                                <p className="text-right">Day P/L</p>
                                <h2 className="font-semibold text-2xl text-green-600">+$253.89</h2>
                            </div>
                        </div>
                        <p className="text-right text-green-600">5.17%</p>

                        <div className="mt-4 border-t-1" /> {/* Horizontal Line*/ }

                        <div className="mt-4 flex items-center justify-between">
                            <div>
                                <p>Invested</p>
                                <h2 className="font-semibold text-1xl">$1,000.00</h2>
                            </div>
                            <div>
                                <p className="text-right">Return</p>
                                <h2 className="font-semibold text-1xl text-green-600">+$4,157.23</h2>
                            </div>
                        </div>
                        <p className="text-right text-green-600">415.72%</p>

                    </div>
                ))}
            </div>
        );
    }
}

export default App
