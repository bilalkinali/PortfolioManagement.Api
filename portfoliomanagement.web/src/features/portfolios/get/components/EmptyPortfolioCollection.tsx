import { Wallet } from 'lucide-react'
import { Button } from '@/components/ui/button'

export default function EmptyPortfolioCollection() {
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
    )
}