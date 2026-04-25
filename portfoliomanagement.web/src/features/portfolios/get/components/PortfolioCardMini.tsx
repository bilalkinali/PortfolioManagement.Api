import { type PortfolioResponse } from '../api/getPortfolios'

type PortfolioCardMiniProps = {
    portfolio: PortfolioResponse
}

export default function PortfolioCardMini({ portfolio }: PortfolioCardMiniProps) {

    return (
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

            <div className="mt-4 border-t-1" /> {/* Horizontal Line*/}

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
    )
}