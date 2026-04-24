import { getToken } from "@/features/auth/shared/token-storage";

export async function apiFetch(input: RequestInfo, init: RequestInit = {}) {
    const token = getToken();

    const headers = new Headers(init.headers);

    if (!headers.has("Content-Type") && init.body) {
        headers.set("Content-Type", "application/json");
    }

    if (token) {
        headers.set("Authorization", `Bearer ${token}`);
    }

    return fetch(input, {
        ...init,
        headers,
    });
}