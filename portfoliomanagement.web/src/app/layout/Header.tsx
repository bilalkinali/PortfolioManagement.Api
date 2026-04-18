import { Button } from "@/components/ui/button"
import LoginDialog from "@/features/auth/login/components/LoginDialog"
import RegisterDialog from "@/features/auth/register/components/RegisterDialog"
import { useAuth } from "@/features/auth/shared/auth-context"
import { Spinner } from "@/components/ui/spinner"

export default function Header() {
    const { isLoggedIn, user, logout } = useAuth();

    return (
        <header className="bg-slate-950 text-white">
            <div className="mx-auto flex h-24 w-full max-w-7xl items-center justify-between px-6">
            {/* testing structure and content */}
                <div>Header</div>

                {isLoggedIn ? (
                    <div className="flex items-center gap-3">
                        {user ? (
                            <span>Hello, {user.firstName} {user.lastName}</span>
                        ) : (
                            <div className="flex items-center gap-2">
                                <Spinner className="size-3" data-icon="inline-start"/>
                                <span className="text-sm text-slate-300">Loading user...</span>
                            </div>
                        )}
                        <Button onClick={logout}>Logout</Button>
                    </div>
                ) : (
                    <div className="flex items-center gap-3">
                        <LoginDialog onSuccess={() => { }} />
                        <RegisterDialog onSuccess={() => { }} />
                    </div>
                )}
            </div>
        </header>
    );
}