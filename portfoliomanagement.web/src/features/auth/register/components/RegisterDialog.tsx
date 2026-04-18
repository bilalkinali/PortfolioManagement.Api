import { useState } from 'react';
import RegisterForm from './RegisterForm';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Button } from '@/components/ui/button';
import { register as registerRequest } from '../api/register';
import type { RegisterRequest } from '../api/register';

type RegisterDialogProps = {
    onSuccess: () => void;
}

export default function RegisterDialog({ onSuccess }: RegisterDialogProps) {
    const [open, setOpen] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    function handleOpenChange(nextOpen: boolean) {
        if (isSubmitting) return;
        setOpen(nextOpen);
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
                onEscapeKeyDown={(e) => { if (isSubmitting) e.preventDefault(); }}
            >
            <DialogHeader>
                <DialogTitle>Create an account</DialogTitle>
                <DialogDescription>
                    Please enter your details.
                </DialogDescription>
            </DialogHeader>

            <RegisterForm
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                isSubmitting={isSubmitting}
                errorMessage={errorMessage}
            />

            </DialogContent>
        </Dialog>
    );
}
