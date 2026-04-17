import { Button } from "@/components/ui/button"
import LoginDialog from "@/features/auth/login/components/LoginDialog"
import { useAuth } from "@/features/auth/shared/auth-context"

export default function Header() {
    const { isLoggedIn, user, logout } = useAuth();

    return (
        <header className="bg-slate-950 text-white">
            <div className="mx-auto flex h-24 w-full max-w-7xl items-center justify-between px-6">
            {/* testing structure and content */}
                <div>Header</div>

                {isLoggedIn ? (
                    <div className="flex items-center gap-3">
                        <div>Hello {user?.firstName}</div>
                        <Button onClick={logout}>Logout</Button>
                    </div>
                ) : (
                    <div className="flex items-center gap-3">
                            <LoginDialog onSuccess={() => {}} /> {/* not needed, but keeping for now */}
                        <Button>Register</Button>
                    </div>
                )}
            </div>
        </header>
    );
}