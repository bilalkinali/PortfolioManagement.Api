import { Wallet } from 'lucide-react'
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import CreatePortfolioDialog from '@/features/portfolios/create/components/CreatePortfolioDialog'

export default function EmptyPortfolioCollection() {
    return (
        <Empty className="border border-gray-300 bg-white">
            <EmptyHeader>
                <EmptyMedia variant="icon">
                    <Wallet className="text-gray-400"/>
                </EmptyMedia>
                <EmptyTitle>No portfolios yet</EmptyTitle>
                <EmptyDescription>
                    Create your first portfolio to start tracking investments
                </EmptyDescription>
            </EmptyHeader>
            <EmptyContent>
                <CreatePortfolioDialog onSuccess={() => { }} />
            </EmptyContent>
        </Empty>
    )
}