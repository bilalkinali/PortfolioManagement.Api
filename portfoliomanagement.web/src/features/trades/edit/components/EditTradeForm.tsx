import { useEffect, useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field"
import type { PortfolioTradeResponse, TradeType } from '../../../portfolios/details/api/getPortfolio';
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Calendar } from "@/components/ui/calendar"
import { CalendarIcon } from "lucide-react"
import { Button } from "@/components/ui/button"
import { fromDateOnlyString, toDateOnlyString } from "@/shared/helpers/formatters";

type EditTradeFormProps = {
    ref: RefObject<HTMLFormElement | null>;
    trade: PortfolioTradeResponse;
    onSubmit: (
        type: TradeType,
        shares: number,
        price: number,
        executedDate: string
    ) => Promise<void>;
    isSubmitting: boolean;
    errorMessage: string | null;
    onChangeState: (hasChanges: boolean) => void;
}

type EditTradeFormErrors = {
    shares?: string;
    price?: string;
    executedDate?: string;
}

function validateTradeForm(shares: string, price: string, executedDate: string): EditTradeFormErrors {
    const errors: EditTradeFormErrors = {};
    const sharesValue = Number(shares);
    const priceValue = Number(price);
    const today = toDateOnlyString(new Date());

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

function hasValidationErrors(errors: EditTradeFormErrors) {
    return Boolean(errors.shares || errors.price || errors.executedDate);
}

export default function EditTradeForm({
    ref,
    trade,
    onSubmit,
    isSubmitting,
    errorMessage,
    onChangeState
}: EditTradeFormProps) {
    const [type, setType] = useState<TradeType>(trade.type);
    const [shares, setShares] = useState(trade.shares.toString());
    const [price, setPrice] = useState(trade.price.toString());
    const [executedDate, setExecutedDate] = useState(trade.executedDate);
    const [validationErrors, setValidationErrors] = useState<EditTradeFormErrors>({});
    const [datePopoverOpen, setDatePopoverOpen] = useState(false)

    useEffect(() => {
        const hasChanges =
            type !== trade.type ||
            Number(shares) !== trade.shares ||
            Number(price) !== trade.price ||
            executedDate !== trade.executedDate;

        onChangeState(hasChanges);
    }, [type, shares, price, executedDate, trade, onChangeState]);

    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();

        const nextValidationErrors = validateTradeForm(shares, price, executedDate);
        setValidationErrors(nextValidationErrors);

        if (hasValidationErrors(nextValidationErrors)) {
            return;
        }

        await onSubmit(
            type,
            Number(shares),
            Number(price),
            executedDate
        );
    }

    return (
        <form ref={ref} onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel>Type *</FieldLabel>
                    <select
                        id="trade-type"
                        value={type}
                        disabled={isSubmitting}
                        onChange={(e) => setType(e.target.value as TradeType)}
                        className="border-input bg-background ring-offset-background focus-visible:ring-ring flex h-9 w-full rounded-md border px-3 py-1 text-sm shadow-xs outline-none focus-visible:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                        <option value="Buy">Buy</option>
                        <option value="Sell">Sell</option>
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
