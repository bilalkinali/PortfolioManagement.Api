import { useState, useRef } from 'react';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogFooter, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Button } from '@/components/ui/button';
import { Spinner } from '@/components/ui/spinner';
import AddTradeForm from './AddTradeForm';
import { addTrade as addTradeRequest } from '../api/addTrade'
import type { AddTradeRequest } from '../api/addTrade'

type AddTradeDialogProps = {
    onSuccess: () => void;
    portfolioId: number;
}
export default function AddTradeDialog({ onSuccess, portfolioId }: AddTradeDialogProps) {
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
        symbol: string,
        quantity: number,
        price: number,
        executedDate: string) {

        setErrorMessage(null);
        setIsSubmitting(true);

        const request: AddTradeRequest = { symbol, quantity, price, executedDate };

        try {
            await addTradeRequest(portfolioId, request);

            setOpen(false);
            onSuccess();

        } catch (e: unknown) {
            if (e instanceof Error) {
                setErrorMessage("Couldn't connect to server");
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
                <Button>Add a Trade</Button>
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
                    errorMessage={errorMessage} />

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