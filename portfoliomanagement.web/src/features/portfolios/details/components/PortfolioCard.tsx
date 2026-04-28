import { type PortfolioResponse } from '@/features/portfolios/details/api/getPortfolio'

type PortfolioCardProps = {
    portfolio: PortfolioResponse
}

export default function PortfolioCard({ portfolio }: PortfolioCardProps) {
  return (
    <section>
        <h1>{portfolio.name}</h1>
        {portfolio.description ? <p>{portfolio.description}</p> : null}
        <p>{portfolio.positions.length} positions</p>
    </section>
  );
}
