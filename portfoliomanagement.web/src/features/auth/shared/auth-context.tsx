import { createContext, useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { getToken, removeToken, saveToken } from './token-storage';
import { getMe } from '../login/api/me';

type AuthUser = {
    email: string;
    firstName: string;
    lastName: string;
};

type AuthContextValue = {
    isLoggedIn: boolean;
    user: AuthUser | null;
    token: string | null;
    login: (token: string, user: AuthUser) => void;
    logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

type AuthProviderProps = {
    children: ReactNode;
};

{// https://react.dev/learn/you-might-not-need-an-effect
}

export function AuthProvider({ children }: AuthProviderProps) {
    const [token, setToken] = useState<string | null>(() => getToken());
    const [user, setUser] = useState<AuthUser | null>(null);

    function isTokenExpired(token: string): boolean {
        const payload = JSON.parse(atob(token.split(".")[1]));
        const expiresAt = payload.exp * 1000;

        return Date.now() >= expiresAt;
    }

    useEffect(() => {
        if (!token || user) {
            return;
        }

        if (isTokenExpired(token)) {
            removeToken();
            return;
        }

        async function loadCurrentUser() {
            try {
                console.log("Loading current user info..."); // Debug log
                const me = await getMe();

                setUser({
                    email: me.email,
                    firstName: me.firstName,
                    lastName: me.lastName,
                });
            } catch {
                removeToken();
                setToken(null);
                setUser(null);
            }
        }

        loadCurrentUser();
    }, [token, user]);

    function login(token: string, user: AuthUser) {
        saveToken(token);
        setToken(token);
        setUser(user);
    }

    function logout() {
        removeToken();
        setToken(null);
        setUser(null);
    }

    const value: AuthContextValue = {
        isLoggedIn: token !== null,
        user,
        token,
        login,
        logout,
    };

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const context = useContext(AuthContext);

    if (context === null) {
        throw new Error("useAuth must be used within an AuthProvider");
    }

    return context;
}