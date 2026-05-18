import { useEffect, useState } from "react";
import { useParams } from "react-router";
import { getStockProfile, type StockProfileResponse } from "@/features/instruments/detail/api/getStockProfile";
import StockProfileCard from "@/features/instruments/detail/components/StockProfileCard";
import StockProfileCardSkeleton from "@/features/instruments/detail/components/StockProfileCardSkeleton";

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

    if (isLoading) return <StockProfileCardSkeleton />;
    if (error) return <p>{error}</p>;
    if (!profile) return <p>No stock profile found.</p>;

    return <StockProfileCard profile={profile} />;
    
}
