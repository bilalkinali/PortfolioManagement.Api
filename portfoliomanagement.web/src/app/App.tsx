import SearchInstrumentsCard from '@/features/instruments/searchInstruments/components/SearchInstrumentsCard'
import PortfolioSection from '@/features/portfolios/get/components/PortfolioSection'
import { Button } from '@/components/ui/button'
import { Link } from 'react-router'


function App() {
    return (
        <>
            <Button asChild>
                <Link to="/instruments/AAPL">
                    Test Instrument: Apple Inc. (AAPL)
                </Link>
            </Button>

            <Button asChild>
                <Link to="/portfolios">
                    Portfolios
                </Link>
            </Button>

            <SearchInstrumentsCard />
            <PortfolioSection />            
        </>
    );
}

export default App
