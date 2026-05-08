import { useEffect, useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field"
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

type EditTradeFormErrors = {
    quantity?: string;
    price?: string;
    executedDate?: string;
}

function validateTradeForm(quantity: string, price: string, executedDate: string): EditTradeFormErrors {
    const errors: EditTradeFormErrors = {};
    const quantityValue = Number(quantity);
    const priceValue = Number(price);
    const today = toDateOnlyString(new Date());

    if (quantity.trim() === "") {
        errors.quantity = "Quantity is required.";
    } else if (!Number.isInteger(quantityValue)) {
        errors.quantity = "Quantity must be a whole number.";
    } else if (quantityValue === 0) {
        errors.quantity = "Quantity cannot be zero.";
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

function hasValidationErrors(errors: EditTradeFormErrors) {
    return Boolean(errors.quantity || errors.price || errors.executedDate);
}

export default function EditTradeForm({
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
    const [validationErrors, setValidationErrors] = useState<EditTradeFormErrors>({});
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

        const nextValidationErrors = validateTradeForm(quantity, price, executedDate);
        setValidationErrors(nextValidationErrors);

        if (hasValidationErrors(nextValidationErrors)) {
            return;
        }

        await onSubmit(
            Number(quantity),
            Number(price),
            executedDate
        );
    }

    return (
        <form ref={ref} onSubmit={handleSubmit}>
            <FieldGroup>
                <Field data-invalid={Boolean(validationErrors.quantity)}>
                    <FieldLabel>Quantity *</FieldLabel>
                    <Input
                        id="trade-quantity"
                        type="number"
                        value={quantity}
                        placeholder="0"
                        disabled={isSubmitting}
                        aria-invalid={Boolean(validationErrors.quantity)}
                        onChange={(e) => {
                            setQuantity(e.target.value)
                            setValidationErrors((current) => ({ ...current, quantity: undefined }))
                        }} />
                    <FieldError>{validationErrors.quantity}</FieldError>
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
