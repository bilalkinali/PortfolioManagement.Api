import { useState } from "react"
import { Button } from "@/components/ui/button"

export default function Header() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);

    return (
        <header className="bg-slate-950 text-white">
            <div className="mx-auto flex h-24 w-full max-w-7xl items-center justify-between px-6">
            {/* testing structure and content */}
                <div>Header</div>

                {isLoggedIn ? (
                    <div className="flex items-center gap-3">
                        <div>Hello user</div>
                        <Button onClick={() => setIsLoggedIn(prev => !prev)}>Logout</Button>
                    </div>
                ) : (
                    <div className="flex items-center gap-3">
                        <Button onClick={() => setIsLoggedIn(prev => !prev)}>Login</Button>
                        <Button>Register</Button>
                    </div>
                )}
            </div>
        </header>
    );
}