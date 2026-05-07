import { useEffect, useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"
import type { PortfolioTradeResponse } from '../../../portfolios/details/api/getPortfolio';

type EditTradeFormProps = {
    ref: RefObject<HTMLFormElement | null>;
    trade: PortfolioTradeResponse;
    onSubmit: (
        quantity: number,
        price: number,
        executedDate: string
    ) => Promise<void>;
    isSubmitting: boolean;
    errorMessage: string | null;
    onChangeState: (hasChanges: boolean) => void;
}

export default function AddTradeForm({
    ref,
    trade,
    onSubmit,
    isSubmitting,
    errorMessage,
    onChangeState
}: EditTradeFormProps) {
    const [quantity, setQuantity] = useState(trade.quantity.toString());
    const [price, setPrice] = useState(trade.price.toString());
    const [executedDate, setExecutedDate] = useState(trade.executedDate);

    useEffect(() => {
        const hasChanges =
            Number(quantity) !== trade.quantity ||
            Number(price) !== trade.price ||
            executedDate !== trade.executedDate;

        onChangeState(hasChanges);
    }, [quantity, price, executedDate, trade, onChangeState]);

    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();

        await onSubmit(
            Number(quantity),
            Number(price),
            executedDate
        );
    }

    return (
        <form ref={ref} onSubmit={handleSubmit}>
            <FieldGroup>
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