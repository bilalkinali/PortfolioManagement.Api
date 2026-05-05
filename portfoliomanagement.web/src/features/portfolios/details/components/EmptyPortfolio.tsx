import { IconFolderCode } from "@tabler/icons-react"
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import AddTradeDialog from "@/features/trades/add/components/AddTradeDialog"

type EmptyPortfolioProps = {
    portfolioId: number;
    onSuccess: () => void;
}

export default function EmptyPortfolio({ onSuccess, portfolioId }: EmptyPortfolioProps) {
  return (
      <Empty className="border border-gray-300 bg-white">
          <EmptyHeader>
              <EmptyMedia variant="icon">
                  <IconFolderCode />
              </EmptyMedia>
              <EmptyTitle>No positions yet</EmptyTitle>
              <EmptyDescription>
                  Register your first trade to add a position
              </EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
              <AddTradeDialog
                  onSuccess={onSuccess}
                  portfolioId={portfolioId}
                  buttonVariant="outline"
                  buttonSize="sm" />
          </EmptyContent>
      </Empty>
  )
}