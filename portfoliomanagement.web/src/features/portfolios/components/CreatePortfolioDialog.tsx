import { useState, useRef } from 'react';
import CreatePortfolioForm from './CreatePortfolioForm';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogFooter, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Button } from '@/components/ui/button';
import { Spinner } from '@/components/ui/spinner';
import { createPortfolio as createPortfolioRequest } from '../api/createPortfolio';
import type { CreatePortfolioRequest } from '../api/createPortfolio';

type CreatePortfolioDialogProps = {
    onSuccess: () => void;
}

export default function CreatePortfolioDialog({ onSuccess }: CreatePortfolioDialogProps) {
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
        setOpen(false);
    }

    async function handleSubmit(
        name: string,
        description: string) {

        setErrorMessage(null);
        setIsSubmitting(true);

        const request: CreatePortfolioRequest = { name, description };

        try {
            await new Promise(resolve => setTimeout(resolve, 2000)); // Debug spinner
            await createPortfolioRequest(request);

            setOpen(false);
            onSuccess();

        } catch (e: unknown) {
            if (e instanceof Error) {
                setErrorMessage("Couldn't connect to server");
            } else {
                setErrorMessage("Creation failed");
            }
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogTrigger asChild>
                <Button>Create Portfolio</Button>
            </DialogTrigger>

            <DialogContent
                onInteractOutside={(e) => e.preventDefault()}
                onEscapeKeyDown={(e) => { if (isSubmitting) e.preventDefault(); }}
            >
            <DialogHeader>
                <DialogTitle>Create a portfolio</DialogTitle>
                <DialogDescription>
                    Please enter the details for your new portfolio.
                </DialogDescription>
            </DialogHeader>

            <CreatePortfolioForm
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
                            Creating...
                            <Spinner className="mr-2" />
                        </>
                        : "Create"
                    }
                </Button>
            </DialogFooter>

            </DialogContent>
        </Dialog>
    );
}
