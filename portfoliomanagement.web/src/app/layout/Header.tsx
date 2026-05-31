import { useRef, useState, useEffect } from "react"
import { useNavigate } from 'react-router'
import * as PopoverPrimitive from "@radix-ui/react-popover"
import { searchInstruments, type SearchInstrumentResult } from "@/features/instruments/searchInstruments/api/searchInstruments"
import { formatCurrency, formatExchangeName } from "@/shared/helpers/formatters"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import LoginDialog from "@/features/auth/login/components/LoginDialog"
import RegisterDialog from "@/features/auth/register/components/RegisterDialog"
import { useAuth } from "@/features/auth/shared/auth-context"
import { Spinner } from "@/components/ui/spinner"
import { SearchIcon } from "lucide-react";
import {
    Popover,
    PopoverContent,
} from "@/components/ui/popover"
import {
    Command,
    CommandEmpty,
    CommandGroup,
    CommandItem,
    CommandList
} from "@/components/ui/command"

export default function Header() {
    const { isLoggedIn, user, logout } = useAuth()

    const [searchOpen, setSearchOpen] = useState(false)
    const [instrumentSearch, setInstrumentSearch] = useState("")
    const [instruments, setInstruments] = useState<SearchInstrumentResult[]>([])
    const [isLoadingInstruments, setIsLoadingInstruments] = useState(false);

    const searchRef = useRef<HTMLDivElement>(null)
    const navigate = useNavigate()

    useEffect(() => {
        const query = instrumentSearch.trim();

        if (!searchOpen) {
            return;
        }

        if (query.length < 3) {
            setInstruments([]);
            setIsLoadingInstruments(false);
            return;
        }

        setIsLoadingInstruments(true);

        const controller = new AbortController();
        const searchDebounceMs = 600;

        const timeoutId = window.setTimeout(async () => {
            try {
                const results = await searchInstruments(
                    query,
                    10,
                    undefined,
                    controller.signal
                );

                setInstruments(results);
            } catch (error) {
                if (error instanceof DOMException && error.name === "AbortError") {
                    return;
                }

                console.error(error);
                setInstruments([]);
            } finally {
                setIsLoadingInstruments(false);
            }
        }, searchDebounceMs);

        return () => {
            controller.abort();
            window.clearTimeout(timeoutId);
        };
    }, [instrumentSearch, searchOpen]);


    return (
        <header className="border bg-white">
            <div className="mx-auto grid h-24 w-full max-w-7xl grid-cols-3 items-center px-6 gap-4">
                <div className="flex justify-start">Header</div>

                <div className="flex justify-center">
                    <Popover open={searchOpen}>
                        <PopoverPrimitive.Anchor asChild>
                            <div ref={searchRef} className="relative w-full max-w-xl">
                                <SearchIcon className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                                <Input
                                    id="search-instrument"
                                    type="text"
                                    value={instrumentSearch}
                                    onChange={(e) => {
                                        setInstrumentSearch(e.target.value)
                                        setSearchOpen(true)
                                    }}
                                    onFocus={() => setSearchOpen(true)}
                                    className="w-full pl-9"
                                    placeholder="Search Apple, MSFT, Novo..."
                                />
                            </div>
                        </PopoverPrimitive.Anchor>

                        <PopoverContent
                            align="center"
                            sideOffset={6}
                            className="w-[calc(100vw-2rem)] max-w-xl p-0"
                            onOpenAutoFocus={(e) => e.preventDefault()}
                            onInteractOutside={(e) => {
                                const target = e.target as Node

                                if (searchRef.current?.contains(target)) {
                                    e.preventDefault()
                                    return
                                }
                                setInstrumentSearch("")
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

                                <Command shouldFilter={false}>
                                    <CommandList
                                        className="max-h-72 overflow-y-auto"
                                        onWheel={(event) => event.stopPropagation()}>
                                        {isLoadingInstruments && (
                                            <div className="flex items-center gap-2 p-3 text-sm text-muted-foreground">
                                                <Spinner className="size-4" />
                                                <span>Searching...</span>
                                            </div>
                                        )}

                                        {!isLoadingInstruments && instruments.length === 0 && instrumentSearch.trim().length >= 3 && (
                                            <CommandEmpty>No instruments found.</CommandEmpty>
                                        )}

                                        {!isLoadingInstruments && instrumentSearch.trim().length < 3 && (
                                            <div className="p-3 text-sm text-muted-foreground">
                                                Type at least 3 characters.
                                            </div>
                                        )}

                                        <CommandGroup>
                                            {instruments.map((instrument) => (
                                                <CommandItem className="pr-0"
                                                    key={instrument.id}
                                                    value={`${instrument.symbol} ${instrument.name}`}
                                                    onSelect={() => {                                                  
                                                        setInstrumentSearch("");
                                                        setSearchOpen(false);
                                                        navigate(`/instruments/${instrument.symbol}`);
                                                    }}
                                                >
                                                    <div className="grid w-full grid-cols-12 items-start gap-x-2">
                                                        <div className="col-span-2 min-w-0">
                                                            <div className="font-semibold">
                                                                {instrument.symbol}
                                                            </div>

                                                            <div className="truncate text-xs text-muted-foreground">
                                                                {formatExchangeName(instrument.exchangeCode)}
                                                            </div>
                                                        </div>

                                                        <div className="col-span-7 min-w-0 truncate text-muted-foreground">
                                                            {instrument.name}
                                                        </div>

                                                        <div className="col-span-3 text-right font-semibold tabular-nums">
                                                            {instrument.latestPrice != null
                                                                ? formatCurrency(instrument.latestPrice, instrument.currency)
                                                                : "No price"}
                                                        </div>
                                                    </div>
                                                </CommandItem>
                                            ))}
                                        </CommandGroup>
                                    </CommandList>
                                </Command>

                            </div>
                        </PopoverContent>
                    </Popover>
                </div>

                <div className="flex justify-end">
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

            </div>
        </header>
    )
}