import { Wallet } from 'lucide-react'
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import CreatePortfolioDialog from '@/features/portfolios/create/components/CreatePortfolioDialog'

type EmptyPortfolioCollectionProps = {
    onSuccess?: () => void;
}

export default function EmptyPortfolioCollection({ onSuccess }: EmptyPortfolioCollectionProps) {
    return (
        <Empty className="border border-border bg-card">
            <EmptyHeader>
                <EmptyMedia variant="icon">
                    <Wallet className="text-muted-foreground"/>
                </EmptyMedia>
                <EmptyTitle>No portfolios yet</EmptyTitle>
                <EmptyDescription>
                    Create your first portfolio to start tracking investments
                </EmptyDescription>
            </EmptyHeader>
            <EmptyContent>
                <CreatePortfolioDialog onSuccess={onSuccess ?? (() => { })} />
            </EmptyContent>
        </Empty>
    )
}
