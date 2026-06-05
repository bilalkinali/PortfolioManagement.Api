import { useState, useRef } from 'react';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogFooter, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Button } from '@/components/ui/button';
import { Spinner } from '@/components/ui/spinner';
import { editTrade, type EditTradeRequest } from '../api/editTrade'
import { PencilIcon } from "lucide-react"
import EditTradeForm from './EditTradeForm';
import type { PortfolioTradeResponse, TradeType } from '../../../portfolios/details/api/getPortfolio';

type EditTradeDialogProps = {
    portfolioId: number;
    positionId: number;
    trade: PortfolioTradeResponse;
    onSuccess: () => void;
}
export default function EditTradeDialog({
    portfolioId,
    positionId,
    trade,
    onSuccess
}: EditTradeDialogProps) {
    const [open, setOpen] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [hasChanges, setHasChanges] = useState(false);
    const formRef = useRef<HTMLFormElement>(null);


    function handleOpenChange(nextOpen: boolean) {
        if (!nextOpen && isSubmitting) return;
        setOpen(nextOpen);

        if (!nextOpen) {
            setErrorMessage(null);
            setHasChanges(false);
        }
    }

    function handleCancel() {
        if (isSubmitting) return;

        setErrorMessage(null);
        setOpen(false);
    }

    async function handleSubmit(
        type: TradeType,
        shares: number,
        price: number,
        executedDate: string) {

        setErrorMessage(null);
        setIsSubmitting(true);

        const request: EditTradeRequest = { type, shares, price, executedDate };

        try {
            await editTrade(request, portfolioId, positionId, trade.id);

            setOpen(false);
            onSuccess();

        } catch (e: unknown) {
            console.error("Edit trade failed", e);

            if (e instanceof Error) {
                setErrorMessage(e.message);
            } else {
                setErrorMessage("Edit Trade failed");
            }
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogTrigger asChild>
                <Button
                    variant="ghost"
                    size="icon-xs"
                    aria-label={`Edit trade ${trade.id}`}
                >
                    <PencilIcon />
                </Button>
            </DialogTrigger>

            <DialogContent
                onInteractOutside={(e) => e.preventDefault()}
                onEscapeKeyDown={(e) => { if (isSubmitting) e.preventDefault(); }}
            >
                <DialogHeader>
                    <DialogTitle>Edit Trade</DialogTitle>
                    <DialogDescription>
                        Edit the details of this trade.
                    </DialogDescription>
                </DialogHeader>

                <EditTradeForm
                    ref={formRef}
                    trade={trade}
                    onSubmit={handleSubmit}
                    isSubmitting={isSubmitting}
                    errorMessage={errorMessage}
                    onChangeState={setHasChanges}
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
                        disabled={isSubmitting || !hasChanges}
                    >
                        {isSubmitting ?
                            <>
                                Updating...
                                <Spinner className="mr-2" />
                            </>
                            : "Edit Trade"
                        }
                    </Button>
                </DialogFooter>

            </DialogContent>
        </Dialog>
    )
}
