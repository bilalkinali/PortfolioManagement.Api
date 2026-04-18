import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import LoginForm from "./LoginForm"
import { login as loginRequest } from "../api/login"
import type { LoginRequest } from "../api/login"
import { useAuth } from "../../shared/auth-context"

type LoginDialogProps = {
    onSuccess: () => void;
}

export default function LoginDialog({ onSuccess }: LoginDialogProps) {
    const [open, setOpen] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const { login } = useAuth();

    function handleOpenChange(nextOpen: boolean) {
        if (isSubmitting) return; // Prevent closing the dialog while submitting
        setOpen(nextOpen);
    }

    function handleCancel() {
        if (isSubmitting) return;
        setOpen(false);
    }

    async function handleSubmit(email: string, password: string) {
        setErrorMessage(null);
        setIsSubmitting(true);

        const request: LoginRequest = {
            email,
            password
        };

        try {
            //await new Promise(resolve => setTimeout(resolve, 5000)); // Debug spinner

            const response = await loginRequest(request);

            login(response.token, {
                email: response.email,
                firstName: response.firstName,
                lastName: response.lastName
            });

            setOpen(false);
            onSuccess();

        } catch (e: unknown) {
            if (e instanceof Error) {
                setErrorMessage(e.message);
            } else {
                setErrorMessage("Login failed");
            }
        } finally {
            setIsSubmitting(false);
        }
    }


    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogTrigger asChild>
                <Button>Login</Button>
            </DialogTrigger>

            <DialogContent
                onInteractOutside={(e) => e.preventDefault()}
                onEscapeKeyDown={(e) => { if (isSubmitting) e.preventDefault(); }}>

                <DialogHeader>
                    <DialogTitle>Login</DialogTitle>
                    <DialogDescription>
                        Please enter your credentials to login.
                    </DialogDescription>
                </DialogHeader>

                <LoginForm
                    onSubmit={handleSubmit}
                    onCancel={handleCancel}
                    isSubmitting={isSubmitting}
                    errorMessage={errorMessage}
                />

            </DialogContent>
        </Dialog>        
    )
}