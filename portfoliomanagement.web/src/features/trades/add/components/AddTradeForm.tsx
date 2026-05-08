import { useState, useEffect, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"
import { Button } from "@/components/ui/button"
import { ChevronsUpDown, CalendarIcon } from "lucide-react"
import { searchInstruments, type SearchInstrumentResult } from "@/features/instruments/searchInstruments/api/searchInstruments"
import { formatCurrency, formatExchangeName, fromDateOnlyString, toDateOnlyString } from "@/shared/helpers/formatters"
import { Spinner } from "@/components/ui/spinner"
import { Calendar } from "@/components/ui/calendar"
import {
    Command,
    CommandEmpty,
    CommandGroup,
    CommandInput,
    CommandItem,
    CommandList
} from "@/components/ui/command"
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover"

type AddTradeFormProps = {
    ref: RefObject<HTMLFormElement | null>;
    isSubmitting: boolean;
    errorMessage: string | null;
    onSubmit: (
        instrumentId: number,
        quantity: number,
        price: number,
        executedDate: string
    ) => Promise<void>;
}

export default function AddTradeForm({
    ref,
    onSubmit,
    isSubmitting,
    errorMessage
}: AddTradeFormProps) {
    const [instrumentSearch, setInstrumentSearch] = useState("");
    const [instruments, setInstruments] = useState<SearchInstrumentResult[]>([]);
    const [isLoadingInstruments, setIsLoadingInstruments] = useState(false);
    const [selectedInstrument, setSelectedInstrument] = useState<SearchInstrumentResult | null>(null);
    const [quantity, setQuantity] = useState("");
    const [price, setPrice] = useState("");
    const [executedDate, setExecutedDate] = useState("");
    const [instrumentPopoverOpen, setInstrumentPopoverOpen] = useState(false);
    const [datePopoverOpen, setDatePopoverOpen] = useState(false);

    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();

        if (!selectedInstrument) {
            return;
        }

        await onSubmit(
            selectedInstrument.id,
            Number(quantity),
            Number(price),
            executedDate
        );
    }

    useEffect(() => {
        const query = instrumentSearch.trim();

        if (!instrumentPopoverOpen) {
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
    }, [instrumentSearch, instrumentPopoverOpen]);

    return (
        <form ref={ref} onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel>Symbol *</FieldLabel>
                    <Popover
                        open={instrumentPopoverOpen}
                        onOpenChange={(open) => {
                            setInstrumentPopoverOpen(open);

                            if (open) {
                                setInstrumentSearch("");
                                setInstruments([]);
                            }
                        }}
                    >
                        <PopoverTrigger asChild>
                            <Button
                                id="trade-instrument"
                                type="button"
                                variant="outline"
                                disabled={isSubmitting}
                                className="w-full justify-between font-normal"
                            >
                                {selectedInstrument
                                    ? 
                                    (
                                        <div className="grid w-full grid-cols-12 items-center gap-x-2 overflow-hidden">
                                            <span className="col-span-2 min-w-0 truncate text-left font-semibold">
                                                {selectedInstrument.symbol}
                                            </span>

                                            <span className="col-span-5 min-w-0 truncate text-left text-muted-foreground">
                                                {selectedInstrument.name}
                                            </span>

                                            <span className="col-span-2 min-w-0 truncate text-left text-muted-foreground">
                                                {formatExchangeName(selectedInstrument.exchangeCode)}
                                            </span>

                                            <span className="col-span-3 min-w-0 truncate text-right font-semibold tabular-nums">
                                                {selectedInstrument.latestPrice != null
                                                    ? formatCurrency(selectedInstrument.latestPrice, selectedInstrument.currency)
                                                    : "No price"}
                                            </span>
                                        </div>
                                    )
                                    : "Select an instrument"}
                                <ChevronsUpDown className="opacity-50" />
                            </Button>
                        </PopoverTrigger>

                        <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0">
                            <Command shouldFilter={false}>
                                <CommandInput
                                    placeholder="Search instrument..."
                                    value={instrumentSearch}
                                    onValueChange={setInstrumentSearch}
                                />

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
                                                    setSelectedInstrument(instrument);
                                                    setInstrumentSearch("");
                                                    setInstrumentPopoverOpen(false);
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
                        </PopoverContent>
                    </Popover>
                    
                </Field>
                <Field>
                    <FieldLabel>Quantity *</FieldLabel>
                    <Input
                        id="trade-quantity"
                        type="number"
                        value={quantity}
                        placeholder="0"
                        disabled={isSubmitting}
                        onChange={(e) => setQuantity(e.target.value)} />
                </Field>
                <Field>
                    <FieldLabel>Price *</FieldLabel>
                    <Input
                        id="trade-price"
                        type="number"
                        step="0.01"
                        min="0.01"
                        placeholder="0.00"
                        value={price}
                        disabled={isSubmitting}
                        onChange={(e) => setPrice(e.target.value)} />
                </Field>
                <Field>
                    <FieldLabel>Date *</FieldLabel>

                    <Popover open={datePopoverOpen} onOpenChange={setDatePopoverOpen}>
                        <PopoverTrigger asChild>
                            <Button
                                id="trade-date"
                                type="button"
                                variant="outline"
                                disabled={isSubmitting}
                                className="w-full justify-start text-left font-normal"
                            >
                                <CalendarIcon className="mr-2 size-4" />

                                {executedDate || <span className="text-muted-foreground">YYYY-MM-DD</span>}
                            </Button>
                        </PopoverTrigger>

                        <PopoverContent className="w-auto p-0" align="start">
                            <Calendar className="rounded-lg border"
                                mode="single"
                                captionLayout="dropdown"
                                selected={fromDateOnlyString(executedDate)}
                                onSelect={(date) => {
                                    if (!date) {
                                        return
                                    }

                                    setExecutedDate(toDateOnlyString(date))
                                    setDatePopoverOpen(false)
                                }}
                            />
                        </PopoverContent>
                    </Popover>
                </Field>

                {errorMessage && <p className="text-destructive text-sm">{errorMessage}</p>}
            </FieldGroup>
        </form>
    )
}