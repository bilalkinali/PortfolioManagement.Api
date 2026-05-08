import { useEffect, useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"
import type { PortfolioTradeResponse } from '../../../portfolios/details/api/getPortfolio';
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Calendar } from "@/components/ui/calendar"
import { CalendarIcon } from "lucide-react"
import { Button } from "@/components/ui/button"
import { fromDateOnlyString, toDateOnlyString } from "@/shared/helpers/formatters";

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
    const [datePopoverOpen, setDatePopoverOpen] = useState(false)

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