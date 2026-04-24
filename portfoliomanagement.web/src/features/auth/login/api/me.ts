import { apiFetch } from '@/features/auth/shared/apiClient';


export type MeResponse = {
    email: string,
    firstName: string,
    lastName: string
};

export async function getMe(): Promise<MeResponse> {
    const response = await apiFetch("/auth/me", {
        method: "GET",
    });

    if (!response.ok) {
        throw new Error("Failed to fetch user info");
    }

    return response.json() as Promise<MeResponse>;
}