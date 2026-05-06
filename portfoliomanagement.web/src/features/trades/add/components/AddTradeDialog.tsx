import { useState, useRef } from 'react';
import type { ComponentPropsWithoutRef } from 'react'; 
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogFooter, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Button } from '@/components/ui/button';
import { Spinner } from '@/components/ui/spinner';
import AddTradeForm from './AddTradeForm';
import { addTrade, type AddTradeRequest } from '../api/addTrade'

type ButtonVariant = ComponentPropsWithoutRef<typeof Button>["variant"]
type ButtonSize = ComponentPropsWithoutRef<typeof Button>["size"]

type AddTradeDialogProps = {
    onSuccess: () => void;
    portfolioId: number;
    buttonVariant?: ButtonVariant;
    buttonSize?: ButtonSize;
    buttonText?: string;
}
export default function AddTradeDialog({
    onSuccess,
    portfolioId,
    buttonVariant = "default",
    buttonSize = "default",
    buttonText = "Add a Trade"
}: AddTradeDialogProps) {
    const [open, setOpen] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const formRef = useRef<HTMLFormElement>(null);

    function handleOpenChange(nextOpen: boolean) {
        if (!nextOpen && isSubmitting) return;
        setOpen(nextOpen);

        if (!nextOpen) setErrorMessage(null);
    }

    function handleCancel() {
        if (isSubmitting) return;

        setErrorMessage(null);
        setOpen(false);
    }

    async function handleSubmit(
        instrumentId: number,
        quantity: number,
        price: number,
        executedDate: string) {

        setErrorMessage(null);
        setIsSubmitting(true);

        const request: AddTradeRequest = { instrumentId, quantity, price, executedDate };

        try {
            await addTrade(request, portfolioId);

            setOpen(false);
            onSuccess();

        } catch (e: unknown) {
            console.error("Add trade failed", e);

            if (e instanceof Error) {
                setErrorMessage(e.message);
            } else {
                setErrorMessage("Add Trade failed");
            }
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogTrigger asChild>
                <Button variant={buttonVariant} size={buttonSize}>{buttonText}</Button>
            </DialogTrigger>

            <DialogContent
                onInteractOutside={(e) => e.preventDefault()}
                onEscapeKeyDown={(e) => { if (isSubmitting) e.preventDefault();}}
            >
                <DialogHeader>
                    <DialogTitle>Add a Trade</DialogTitle>
                    <DialogDescription>
                        Register a trade to be added as a Position
                    </DialogDescription>
                </DialogHeader>

                <AddTradeForm
                    ref={formRef}
                    onSubmit={handleSubmit}
                    isSubmitting={isSubmitting}
                    errorMessage={errorMessage}
                />

                <DialogFooter>
                    <Button
                        type="button"
                        variant="outline"
                        onClick={handleCancel}
                        disabled={isSubmitting}>
                        Cancel
                    </Button>
                    <Button
                        type="button"
                        onClick={() => formRef.current?.requestSubmit()}
                        disabled={isSubmitting}>
                        {isSubmitting ?
                            <>
                                Adding...
                                <Spinner className="mr-2" />
                            </>
                            : "Add Trade"
                        }
                    </Button>
                </DialogFooter>

            </DialogContent>
        </Dialog>
    )
}