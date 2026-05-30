import { useRef, useState } from "react"
import * as PopoverPrimitive from "@radix-ui/react-popover"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import LoginDialog from "@/features/auth/login/components/LoginDialog"
import RegisterDialog from "@/features/auth/register/components/RegisterDialog"
import { useAuth } from "@/features/auth/shared/auth-context"
import { Spinner } from "@/components/ui/spinner"
import {
    Popover,
    PopoverContent,
} from "@/components/ui/popover"

export default function Header() {
    const { isLoggedIn, user, logout } = useAuth()

    const [isSearching, setIsSearching] = useState(false)
    const [searchOpen, setSearchOpen] = useState(false)
    const [query, setQuery] = useState("")

    const searchRef = useRef<HTMLDivElement>(null)

    return (
        <header className="border bg-white">
            <div className="mx-auto flex h-24 w-full max-w-7xl items-center justify-between px-6">
                <div>Header</div>

                <Popover open={searchOpen}>
                    <PopoverPrimitive.Anchor asChild>
                        <div ref={searchRef} className="w-96">
                            <Input
                                id="search-instrument"
                                type="text"
                                value={query}
                                onChange={(e) => {
                                    setQuery(e.target.value)
                                    setSearchOpen(true)
                                }}
                                onFocus={() => setSearchOpen(true)}
                                className="w-full"
                                placeholder="Search Apple, MSFT, Novo..."
                                disabled={isSearching}
                            />
                        </div>
                    </PopoverPrimitive.Anchor>

                    <PopoverContent
                        align="start"
                        sideOffset={8}
                        className="w-96 p-0"
                        onOpenAutoFocus={(e) => e.preventDefault()}
                        onInteractOutside={(e) => {
                            const target = e.target as Node

                            if (searchRef.current?.contains(target)) {
                                e.preventDefault()
                                return
                            }

                            setSearchOpen(false)
                        }}
                        onEscapeKeyDown={() => setSearchOpen(false)}
                    >
                        <div className="p-3">
                            <div className="mb-3 flex gap-2">
                                <Button size="sm" variant="secondary">
                                    All
                                </Button>
                                <Button size="sm" variant="outline">
                                    Stock
                                </Button>
                                <Button size="sm" variant="outline">
                                    Index
                                </Button>
                            </div>

                            <div className="space-y-2">
                                <div className="cursor-pointer rounded-md p-2 hover:bg-muted">
                                    <div className="font-medium">AAPL</div>
                                    <div className="text-sm text-muted-foreground">
                                        Apple Inc.
                                    </div>
                                </div>

                                <div className="cursor-pointer rounded-md p-2 hover:bg-muted">
                                    <div className="font-medium">MSFT</div>
                                    <div className="text-sm text-muted-foreground">
                                        Microsoft Corporation
                                    </div>
                                </div>

                                <div className="cursor-pointer rounded-md p-2 hover:bg-muted">
                                    <div className="font-medium">NVO</div>
                                    <div className="text-sm text-muted-foreground">
                                        Novo Nordisk
                                    </div>
                                </div>
                            </div>
                        </div>
                    </PopoverContent>
                </Popover>

                {isLoggedIn ? (
                    <div className="flex items-center gap-3">
                        {user ? (
                            <span>
                                Hello, {user.firstName} {user.lastName}
                            </span>
                        ) : (
                            <div className="flex items-center gap-2">
                                <Spinner className="size-3" data-icon="inline-start" />
                                <span className="text-sm text-slate-300">
                                    Loading user...
                                </span>
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
    )
}