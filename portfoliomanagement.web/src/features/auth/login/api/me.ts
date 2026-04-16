export type MeResponse = {
    email: string,
    firstName: string,
    lastName: string
};

export async function getMe(token: string): Promise<MeResponse> {
    const response = await fetch("/auth/me", {
        method: "GET",
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });

    if (!response.ok) {
        throw new Error("Failed to fetch user info");
    }

    return response.json() as Promise<MeResponse>;
}