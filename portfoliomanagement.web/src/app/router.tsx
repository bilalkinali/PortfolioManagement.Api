import { Navigate, Outlet, createBrowserRouter } from 'react-router'
import App from '@/app/App'
import AppLayout from '@/app/layout/AppLayout'
import { useAuth } from '@/features/auth/shared/auth-context'
import PortfolioDetailPage from '@/features/portfolios/details/pages/PortfolioDetailPage'
import InstrumentDetailPage from '../features/instruments/detail/pages/InstrumentDetailPage'

function ProtectedRoute() {
    const { isLoggedIn } = useAuth()

    if (!isLoggedIn) {
        return <Navigate to="/" replace />
    }

    return <Outlet />
}

export const router = createBrowserRouter([
    {
        element: <AppLayout />,
        children: [
            {
                path: '/',
                element: <App />,
            },
            {
                path: '/instruments/:symbol',
                element: <InstrumentDetailPage />,
            },
            {
                element: <ProtectedRoute />,
                children: [
                    {
                        path: '/portfolios/:portfolioId',
                        element: <PortfolioDetailPage />,
                    },
                ],
            },
            {
                path: '*',
                element: <Navigate to="/" replace />,
            },
        ],
    },
])
