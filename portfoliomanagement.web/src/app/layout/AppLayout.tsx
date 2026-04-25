import Header from './Header';
import Footer from './Footer';

type AppLayoutProps = {
    children: React.ReactNode;
};

export default function AppLayout({ children }: AppLayoutProps) {
    return (
        <div className="min-h-screen bg-gray-50 flex flex-col">       
            <Header />
            <main className="mx-auto w-full max-w-7xl flex-1 px-6 py-8">
                {children}
            </main>
            <Footer />
        </div>
    );
}