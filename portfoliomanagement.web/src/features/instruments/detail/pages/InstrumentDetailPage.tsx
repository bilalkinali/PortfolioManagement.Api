import { useEffect, useState } from "react";
import { useParams } from "react-router";
import { getStockProfile, type StockProfileResponse } from "@/features/instruments/detail/api/getStockProfile";

export default function InstrumentDetailPage() {
    const { symbol } = useParams<{ symbol: string }>();

    const [profile, setProfile] = useState<StockProfileResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!symbol) {
            setError("Missing symbol");
            setIsLoading(false);
            return;
        }

        async function loadProfile() {
            try {
                setIsLoading(true);
                setError(null);

                const result = await getStockProfile(symbol!);
                setProfile(result);
            } catch {
                setError("Failed to load stock profile");
            } finally {
                setIsLoading(false);
            }
        }

        loadProfile();
    }, [symbol]);

    if (isLoading) return <p>Loading stock profile...</p>;
    if (error) return <p>{error}</p>;
    if (!profile) return <p>No stock profile found.</p>;

    return (
        <div className="space-y-4">
            <div>
                <h1 className="text-2xl font-semibold">{profile.name}</h1>
                <p className="text-muted-foreground">{profile.ticker}</p>
            </div>

            <div className="space-y-1 text-sm">
                <p><strong>Active:</strong> {profile.active ? "Yes" : "No"}</p>
                <p><strong>Market:</strong> {profile.market}</p>
                <p><strong>Exchange:</strong> {profile.primaryExchange}</p>
                <p><strong>Currency:</strong> {profile.currencyName}</p>
                <p><strong>Market cap:</strong> {profile.marketCap}</p>
                <p><strong>Employees:</strong> {profile.totalEmployees}</p>
                <p><strong>Homepage:</strong> {profile.homepageUrl}</p>
                <p><strong>Last synced:</strong> {profile.lastSyncedDate}</p>
            </div>

            <div>
                <h2 className="font-semibold">Description</h2>
                <p className="text-sm text-muted-foreground">{profile.description}</p>
            </div>
        </div>
    );
}