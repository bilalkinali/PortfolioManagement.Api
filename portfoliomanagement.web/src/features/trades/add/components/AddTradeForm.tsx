import { useState, useEffect, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field"
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
import type { TradeType } from "@/features/portfolios/details/api/getPortfolio"

type AddTradeFormProps = {
    ref: RefObject<HTMLFormElement | null>;
    isSubmitting: boolean;
    errorMessage: string | null;
    onSubmit: (
        instrumentId: number,
        type: TradeType,
        shares: number,
        price: number,
        executedDate: string
    ) => Promise<void>;
}

type AddTradeFormErrors = {
    instrument?: string;
    shares?: string;
    price?: string;
    executedDate?: string;
}

function validateTradeForm(
    selectedInstrument: SearchInstrumentResult | null,
    shares: string,
    price: string,
    executedDate: string
): AddTradeFormErrors {
    const errors: AddTradeFormErrors = {};
    const sharesValue = Number(shares);
    const priceValue = Number(price);
    const today = toDateOnlyString(new Date());

    if (!selectedInstrument) {
        errors.instrument = "Instrument is required.";
    }

    if (shares.trim() === "") {
        errors.shares = "Shares are required.";
    } else if (!Number.isInteger(sharesValue)) {
        errors.shares = "Shares must be a whole number.";
    } else if (sharesValue <= 0) {
        errors.shares = "Shares must be greater than zero.";
    }

    if (price.trim() === "") {
        errors.price = "Price is required.";
    } else if (!Number.isFinite(priceValue) || priceValue <= 0) {
        errors.price = "Price must be greater than zero.";
    }

    if (!executedDate) {
        errors.executedDate = "Date is required.";
    } else if (executedDate > today) {
        errors.executedDate = "Date cannot be in the future.";
    }

    return errors;
}

function hasValidationErrors(errors: AddTradeFormErrors) {
    return Boolean(errors.instrument || errors.shares || errors.price || errors.executedDate);
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
    const [type, setType] = useState<TradeType>("Buy");
    const [shares, setShares] = useState("");
    const [price, setPrice] = useState("");
    const [executedDate, setExecutedDate] = useState("");
    const [validationErrors, setValidationErrors] = useState<AddTradeFormErrors>({});
    const [instrumentPopoverOpen, setInstrumentPopoverOpen] = useState(false);
    const [datePopoverOpen, setDatePopoverOpen] = useState(false);

    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();

        const nextValidationErrors = validateTradeForm(selectedInstrument, shares, price, executedDate);
        setValidationErrors(nextValidationErrors);

        if (hasValidationErrors(nextValidationErrors) || !selectedInstrument) {
            return;
        }

        await onSubmit(
            selectedInstrument.id,
            type,
            Number(shares),
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
                <Field data-invalid={Boolean(validationErrors.instrument)}>
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
                                aria-invalid={Boolean(validationErrors.instrument)}
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
                                                    setValidationErrors((current) => ({ ...current, instrument: undefined }));
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
                    <FieldError>{validationErrors.instrument}</FieldError>
                    
                </Field>
                <Field>
                    <FieldLabel>Type *</FieldLabel>
                    <select
                        id="trade-type"
                        value={type}
                        disabled={isSubmitting}
                        onChange={(e) => setType(e.target.value as TradeType)}
                        className="border-input bg-transparent text-foreground ring-offset-background focus-visible:ring-ring flex h-9 w-full rounded-md border px-3 py-1 text-sm shadow-xs outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50 dark:bg-input/30"
                    >
                        <option className="bg-popover text-popover-foreground" value="Buy">Buy</option>
                        <option className="bg-popover text-popover-foreground" value="Sell">Sell</option>
                    </select>
                </Field>
                <Field data-invalid={Boolean(validationErrors.shares)}>
                    <FieldLabel>Shares *</FieldLabel>
                    <Input
                        id="trade-shares"
                        type="number"
                        min="1"
                        value={shares}
                        placeholder="0"
                        disabled={isSubmitting}
                        aria-invalid={Boolean(validationErrors.shares)}
                        onChange={(e) => {
                            setShares(e.target.value)
                            setValidationErrors((current) => ({ ...current, shares: undefined }))
                        }} />
                    <FieldError>{validationErrors.shares}</FieldError>
                </Field>
                <Field data-invalid={Boolean(validationErrors.price)}>
                    <FieldLabel>Price *</FieldLabel>
                    <Input
                        id="trade-price"
                        type="number"
                        step="0.01"
                        min="0.01"
                        placeholder="0.00"
                        value={price}
                        disabled={isSubmitting}
                        aria-invalid={Boolean(validationErrors.price)}
                        onChange={(e) => {
                            setPrice(e.target.value)
                            setValidationErrors((current) => ({ ...current, price: undefined }))
                        }} />
                    <FieldError>{validationErrors.price}</FieldError>
                </Field>
                <Field data-invalid={Boolean(validationErrors.executedDate)}>
                    <FieldLabel>Date *</FieldLabel>

                    <Popover open={datePopoverOpen} onOpenChange={setDatePopoverOpen}>
                        <PopoverTrigger asChild>
                            <Button
                                id="trade-date"
                                type="button"
                                variant="outline"
                                disabled={isSubmitting}
                                className="w-full justify-start text-left font-normal"
                                aria-invalid={Boolean(validationErrors.executedDate)}
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
                                    setValidationErrors((current) => ({ ...current, executedDate: undefined }))
                                    setDatePopoverOpen(false)
                                }}
                            />
                        </PopoverContent>
                    </Popover>
                    <FieldError>{validationErrors.executedDate}</FieldError>
                </Field>

                {errorMessage && <p className="text-destructive text-sm">{errorMessage}</p>}
            </FieldGroup>
        </form>
    )
}
