import { Button } from '@/components/ui/button'
import { IconFolderCode } from "@tabler/icons-react"
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'

export default function EmptyPortfolio() {
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
              <Button variant="outline" size="sm">
                  Add a trade
              </Button>
          </EmptyContent>
      </Empty>
  )
}