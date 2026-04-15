import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import LoginForm from "./LoginForm"

type LoginDialogProps = {
    onSuccess: () => void;
}

export default function LoginDialog({ onSuccess }: LoginDialogProps) {
    const [open, setOpen] = useState(false);

    function handleOpenChange(nextOpen: boolean) {
        setOpen(nextOpen);
    }

    function handleCancel() {
        setOpen(false);
    }
    
    
    async function handleLoginSuccess() {
        onSuccess();
        setOpen(false);
    }


    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogTrigger asChild>
                <Button>Login</Button>
            </DialogTrigger>

            <DialogContent onInteractOutside={(e) => e.preventDefault()}>

                <DialogHeader>
                    <DialogTitle>Login</DialogTitle>
                    <DialogDescription>
                        Please enter your credentials to login.
                    </DialogDescription>
                </DialogHeader>

                <LoginForm
                    onLoginSuccess={handleLoginSuccess}
                    onCancel={handleCancel} 
                />

            </DialogContent>
        </Dialog>        
    )
}