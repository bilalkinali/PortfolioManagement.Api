import { useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"
import {
    Command,
    CommandEmpty,
    CommandGroup,
    CommandInput,
    CommandItem,
    CommandList
} from "@/components/ui/command"

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
    { id: 1, symbol: "AAPL", name: "Apple Inc." },
    { id: 2, symbol: "MSFT", name: "Microsoft Corporation" },
    { id: 3, symbol: "NVDA", name: "NVIDIA Corporation" }
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
                    <FieldLabel htmlFor="trade-instrument">Symbol *</FieldLabel>
                    <Command className="rounded-md border">
                        <CommandInput
                            placeholder={selectedInstrument?.symbol ?? "Select an instrument"}
                            disabled={isSubmitting}
                        />
                        <CommandList>
                            <CommandEmpty>No instruments found.</CommandEmpty>

                            <CommandGroup>
                                {instruments.map((instrument) => (
                                    <CommandItem
                                        key={instrument.id}
                                        value={`${instrument.symbol} ${instrument.name}`}
                                        onSelect={() => setSelectedInstrument(instrument)}
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

                    {selectedInstrument && (
                        <p className="text-muted-foreground text-sm">
                            Selected: {selectedInstrument.symbol} - {selectedInstrument.name}
                        </p>
                    )}
                    {/*<Input*/}
                    {/*    id="trade-instrument"*/}
                    {/*    type="number"*/}
                    {/*    value={selectedInstrument?.symbol ?? ""}*/}
                    {/*    placeholder="AAPL"*/}
                    {/*    disabled={isSubmitting}*/}
                    {/*    onChange={(e) => setSelectedInstrument({*/}
                    {/*        id: 1,*/}
                    {/*        symbol: e.target.value.toUpperCase(),*/}
                    {/*        name: ""*/}
                    {/*    })}*/}
                    {/*    required />*/}
                </Field>
                <Field>
                    <FieldLabel htmlFor="trade-quantity">Quantity *</FieldLabel>
                    <Input
                        id="trade-quantity"
                        type="number"
                        value={quantity}
                        placeholder="100"
                        disabled={isSubmitting}
                        onChange={(e) => setQuantity(e.target.value)} />
                </Field>
                <Field>
                    <FieldLabel htmlFor="trade-price">Price *</FieldLabel>
                    <Input
                        id="trade-price"
                        type="number"
                        value={price}
                        placeholder="100"
                        disabled={isSubmitting}
                        onChange={(e) => setPrice(e.target.value)} />
                </Field>
                <Field>
                    <FieldLabel htmlFor="trade-date">Date *</FieldLabel>
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