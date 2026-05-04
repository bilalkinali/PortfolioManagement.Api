import { useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"

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
                    <Input
                        id="trade-instrument"
                        type="number"
                        value={selectedInstrument?.symbol ?? ""}
                        placeholder="AAPL"
                        disabled={isSubmitting}
                        onChange={(e) => setSelectedInstrument({
                            id: 1,
                            symbol: e.target.value.toUpperCase(),
                            name: ""
                        })}
                        required />
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
                        type="text"
                        value={executedDate}
                        placeholder="100"
                        disabled={isSubmitting}
                        onChange={(e) => setExecutedDate(e.target.value)} />
                </Field>

                {errorMessage && <p className="text-destructive text-sm">{errorMessage}</p>}
            </FieldGroup>
        </form>
    )
}