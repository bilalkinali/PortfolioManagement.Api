import { useState, useEffect, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"
import { Button } from "@/components/ui/button"
import { ChevronsUpDown } from "lucide-react"
import { searchInstruments } from "@/features/instruments/searchInstruments/api/searchInstruments"
import type { SearchInstrumentResult } from "@/features/instruments/searchInstruments/api/searchInstruments"
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
            return;
        }

        const controller = new AbortController(); // Like CancellationToken in .NET
        const searchDebounceMs = 600; // Delay search until user stops typing for x ms

        const timeoutId = window.setTimeout(async () => {
            try {
                setIsLoadingInstruments(true);

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
        }, searchDebounceMs); // <-- debounce delay

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
                                    ? `${selectedInstrument.symbol} - ${selectedInstrument.name}`
                                    : "Select an instrument"}
                                <ChevronsUpDown className="opacity-50" />
                            </Button>
                        </PopoverTrigger>

                        <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0">
                            <Command>
                                <CommandInput
                                    placeholder="Search instrument..."
                                    value={instrumentSearch}
                                    onValueChange={setInstrumentSearch}
                                />

                                <CommandList>
                                    {isLoadingInstruments && (
                                        <div className="p-3 text-sm text-muted-foreground">
                                            Searching...
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
                                            <CommandItem
                                                key={instrument.id}
                                                value={`${instrument.symbol} ${instrument.name}`}
                                                onSelect={() => {
                                                    setSelectedInstrument(instrument);
                                                    setInstrumentSearch(`${instrument.symbol} - ${instrument.name}`);
                                                    setInstrumentPopoverOpen(false);
                                                }}
                                            >
                                                <span className="w-14 font-mono font-semibold">
                                                    {instrument.symbol}
                                                </span>
                                                <span className="text-muted-foreground">
                                                    {instrument.name}
                                                </span>
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
                    <Input
                        id="trade-date"
                        type="date"
                        value={executedDate}
                        placeholder="2018-08-18"
                        disabled={isSubmitting}
                        onChange={(e) => setExecutedDate(e.target.value)} />
                </Field>

                {errorMessage && <p className="text-destructive text-sm">{errorMessage}</p>}
            </FieldGroup>
        </form>
    )
}