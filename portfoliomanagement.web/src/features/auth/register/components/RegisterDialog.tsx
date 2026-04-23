import { useState, useRef } from 'react';
import RegisterForm from './RegisterForm';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogFooter, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Button } from '@/components/ui/button';
import { Spinner } from "@/components/ui/spinner"
import { register as registerRequest } from '../api/register';
import type { RegisterRequest } from '../api/register';

type RegisterDialogProps = {
    onSuccess: () => void;
}

export default function RegisterDialog({ onSuccess }: RegisterDialogProps) {
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
        firstname: string,
        lastname: string,
        email: string,
        username: string,
        password: string) {

        setErrorMessage(null);
        setIsSubmitting(true);

        const request: RegisterRequest = {
            firstname,
            lastname,
            email,
            username,
            password
        };

        try {
            await new Promise(resolve => setTimeout(resolve, 2000)); // Debug spinner
            await registerRequest(request);

            setOpen(false);
            onSuccess();

        } catch (e: unknown) {
            if (e instanceof Error) {
                setErrorMessage(e.message);
            } else {
                setErrorMessage("Registration failed");
            }
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogTrigger asChild>
                <Button>Register</Button>
            </DialogTrigger>

            <DialogContent
                onInteractOutside={(e) => e.preventDefault()}
                onEscapeKeyDown={(e) => { if (isSubmitting) e.preventDefault(); }}>

            <DialogHeader>
                <DialogTitle>Create an account</DialogTitle>
                <DialogDescription>
                    Please enter your details.
                </DialogDescription>
            </DialogHeader>

            <RegisterForm
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
                            Registering...
                            <Spinner className="mr-2" />
                        </>
                        : "Register"
                    }
                </Button>
            </DialogFooter>

            </DialogContent>
        </Dialog>
    );
}
