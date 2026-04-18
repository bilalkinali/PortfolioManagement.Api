export type RegisterRequest = {    
    firstname: string;
    lastname: string;
    email: string;
    username: string;
    password: string;
}

export async function register(request: RegisterRequest): Promise<void> {
    const response = await fetch("/auth/register", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        throw new Error("Registration failed");
    }

    return response.json() as Promise<void>;
}