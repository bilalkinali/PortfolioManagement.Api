import { useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"
import { Button } from "@/components/ui/button"
import { ChevronsUpDown } from "lucide-react"
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

type SelectedInstrument = {
    id: number;
    symbol: string;
    name: string;
}

const instruments: SelectedInstrument[] = [
    { id: 7485, symbol: "AAPL", name: "Apple Inc." },
    { id: 2, symbol: "MSFT", name: "Microsoft Corporation" },
    { id: 7483, symbol: "NVDA", name: "NVIDIA Corporation" }
]

export default function AddTradeForm({
    ref,
    onSubmit,
    isSubmitting,
    errorMessage
}: AddTradeFormProps) {
    const [selectedInstrument, setSelectedInstrument] = useState<SelectedInstrument | null>(null);
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

    return (
        <form ref={ref} onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel>Symbol *</FieldLabel>
                    <Popover
                        open={instrumentPopoverOpen}
                        onOpenChange={setInstrumentPopoverOpen}
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
                                <CommandInput placeholder="Search instrument..." />

                                <CommandList>
                                    <CommandEmpty>No instruments found.</CommandEmpty>

                                    <CommandGroup>
                                        {instruments.map((instrument) => (
                                            <CommandItem
                                                key={instrument.id}
                                                value={`${instrument.symbol} ${instrument.name}`}
                                                onSelect={() => {
                                                    setSelectedInstrument(instrument);
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