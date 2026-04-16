export type LoginRequest = {
    email: string;
    password: string;
}

export type LoginResponse = {
    token: string;
    email: string;
    firstName: string;
    lastName: string;
}


export async function login(request: LoginRequest): Promise<LoginResponse> {
    const response = await fetch("/auth/login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        throw new Error("Login failed");
    }

    return response.json() as Promise<LoginResponse>;
}