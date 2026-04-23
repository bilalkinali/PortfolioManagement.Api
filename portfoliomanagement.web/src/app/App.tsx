import AppLayout from '@/app/layout/AppLayout'
import { Button } from '@/components/ui/button'
import CreatePortfolioDialog from '@/features/portfolios/components/CreatePortfolioDialog'

function App() {
  return (
      <AppLayout>
          <h1 className="text-3xl font-semibold mb-6">Portfolio Management</h1>
          <Button>Test</Button>
          <CreatePortfolioDialog onSuccess={() => console.log("Portfolio created")} />
      </AppLayout>
  );
}

export default App
