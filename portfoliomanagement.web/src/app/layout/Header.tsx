import { useRef, useState, useEffect } from "react"
import * as PopoverPrimitive from "@radix-ui/react-popover"
import { searchInstruments, type SearchInstrumentResult } from "@/features/instruments/searchInstruments/api/searchInstruments"
import { formatCurrency, formatExchangeName } from "@/shared/helpers/formatters"
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
import {
    Command,
    CommandEmpty,
    CommandGroup,
    CommandItem,
    CommandList
} from "@/components/ui/command"

export default function Header() {
    const { isLoggedIn, user, logout } = useAuth()

    const [isSearching, setIsSearching] = useState(false)
    const [searchOpen, setSearchOpen] = useState(false)
    const [instrumentSearch, setInstrumentSearch] = useState("")
    const [instruments, setInstruments] = useState<SearchInstrumentResult[]>([])
    const [isLoadingInstruments, setIsLoadingInstruments] = useState(false);

    const searchRef = useRef<HTMLDivElement>(null)

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
            <div className="mx-auto flex h-24 w-full max-w-7xl items-center justify-between px-6">
                <div>Header</div>

                <Popover open={searchOpen}>
                    <PopoverPrimitive.Anchor asChild>
                        <div ref={searchRef} className="w-96">
                            <Input
                                id="search-instrument"
                                type="text"
                                value={instrumentSearch}
                                onChange={(e) => {
                                    setInstrumentSearch(e.target.value)
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
                            //setInstrumentSearch("") Handle non empty search on close?
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
                                                    //setSelectedInstrument(instrument);
                                                    //setValidationErrors((current) => ({ ...current, instrument: undefined }));
                                                    setInstrumentSearch("");
                                                    setSearchOpen(false);
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