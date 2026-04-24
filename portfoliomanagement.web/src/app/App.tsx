import AppLayout from '@/app/layout/AppLayout'
import { Button } from '@/components/ui/button'
import { Wallet } from 'lucide-react'
import CreatePortfolioDialog from '@/features/portfolios/create/components/CreatePortfolioDialog'

function App() {
    return (
        <AppLayout>
            <h1 className="text-3xl font-semibold mb-6">Portfolio Management</h1>
            <Button>Test</Button>
            <CreatePortfolioDialog onSuccess={() => console.log("Portfolio created")} />
    
            <section className="mt-6">
                {/* Get portfolios, if empty */}
                <div className="mb-6 flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-semibold">Your Portfolios</h1>
                        <p className="text-1xl">Manage and track your portfolios here</p>
                    </div>
                    <Button>New Portfolio</Button>
                </div>
    
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
                {/* Else show portfolio cards */}
            </section>
        </AppLayout>
    );
}

export default App
