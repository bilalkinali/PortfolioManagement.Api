import { useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"

type LoginFormProps = {
    ref: RefObject<HTMLFormElement>;
    onSubmit: (email: string, password: string) => Promise<void>;
    isSubmitting: boolean;
    errorMessage: string | null;
}

export default function LoginForm({
    ref,
    onSubmit,
    isSubmitting,
    errorMessage
}: LoginFormProps) {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");


    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        await onSubmit(email, password);
    }
    

    return (
        <form ref={ref} onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel htmlFor="login-email">Email</FieldLabel>
                    <Input
                        id="login-email"
                        type="email"
                        value={email}
                        placeholder="email@example.com"
                        disabled={isSubmitting}
                        onChange={(e) => setEmail(e.target.value)}
                        required />
                </Field>
                <Field>
                    <FieldLabel htmlFor="login-password">Password</FieldLabel>
                    <Input
                        id="login-password"
                        type="password"
                        value={password}
                        placeholder="******"
                        disabled={isSubmitting}
                        onChange={(e) => setPassword(e.target.value)}
                        required />
                </Field>

                {errorMessage && <p className="text-destructive text-sm">{errorMessage}</p>}
            </FieldGroup>
        </form>
    )
}