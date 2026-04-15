import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"

export default function Header() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [isLoginOpen, setIsLoginOpen] = useState(false);

    return (
        <header className="bg-slate-950 text-white">
            <div className="mx-auto flex h-24 w-full max-w-7xl items-center justify-between px-6">
            {/* testing structure and content */}
                <div>Header</div>

                {isLoggedIn ? (
                    <div className="flex items-center gap-3">
                        <div>Hello user</div>
                        <Button onClick={() => setIsLoggedIn(false)}>Logout</Button>
                    </div>
                ) : (
                    <div className="flex items-center gap-3">

                        <Dialog open={isLoginOpen} onOpenChange={setIsLoginOpen}>
                            <DialogTrigger asChild>
                                <Button onClick={() => setIsLoginOpen(true)}>Login</Button>
                            </DialogTrigger>
                            <DialogContent onInteractOutside={(e) => e.preventDefault()}>

                                <DialogHeader>
                                    <DialogTitle>Login</DialogTitle>
                                    <DialogDescription>
                                        Please enter your credentials to login.
                                    </DialogDescription>
                                </DialogHeader>

                                <FieldGroup>
                                    <Field>
                                        <FieldLabel htmlFor="fieldgroup-email">Email</FieldLabel>
                                        <Input id="fieldgroup-email" type="email" placeholder="email@example.com"/>
                                    </Field>
                                    <Field>
                                        <FieldLabel htmlFor="fieldgroup-password">Password</FieldLabel>
                                        <Input id="fieldgroup-password" type="password" placeholder="******"/>
                                    </Field>

                                    <Field orientation="horizontal">
                                        <Button 
                                            onClick={() => {
                                                setIsLoggedIn(true);
                                                setIsLoginOpen(false);
                                            }}>
                                            Submit
                                        </Button>
                                        <Button 
                                            variant="outline"
                                            onClick={() => setIsLoginOpen(false)}>
                                            Cancel
                                        </Button>
                                    </Field>
                                </FieldGroup>

                            </DialogContent>
                        </Dialog>

                        <Button>Register</Button>
                    </div>
                )}
            </div>
        </header>
    );
}