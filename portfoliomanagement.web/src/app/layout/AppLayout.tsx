import { Outlet } from 'react-router';
import Header from './Header';
import Footer from './Footer';

type AppLayoutProps = {
    children?: React.ReactNode;
};

export default function AppLayout({ children }: AppLayoutProps) {
    return (
        <div className="flex min-h-screen flex-col bg-background text-foreground">
            <Header />
            <main className="mx-auto w-full max-w-7xl flex-1 px-6 py-10">
                {children ?? <Outlet />}
            </main>
            <Footer />
        </div>
    );
}
