import { useState, type FormEvent } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Spinner }from "@/components/ui/spinner"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"

type LoginFormProps = {
    onSubmit: (email: string, password: string) => Promise<void>;
    onCancel: () => void;
    isSubmitting: boolean;
    errorMessage: string | null;
}

export default function LoginForm({
    onSubmit,
    onCancel,
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
        <form onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel htmlFor="login-email">Email</FieldLabel>
                    <Input
                        id="login-email"
                        type="email"
                        value={email}
                        placeholder="email@example.com"
                        disabled={isSubmitting}
                        onChange={(e) => setEmail(e.target.value)} />
                </Field>
                <Field>
                    <FieldLabel htmlFor="login-password">Password</FieldLabel>
                    <Input
                        id="login-password"
                        type="password"
                        value={password}
                        placeholder="******"
                        disabled={isSubmitting}
                        onChange={(e) => setPassword(e.target.value)} />
                </Field>

                {errorMessage && <p>{errorMessage}</p>}

                <Field orientation="horizontal">
                    <Button
                        type="submit"
                        disabled={isSubmitting}>
                        Login
                        { isSubmitting && <Spinner className="mr-2" />}
                    </Button>

                    <Button
                        type="button"
                        variant="outline"
                        onClick={onCancel}
                        disabled={isSubmitting}>
                        Cancel
                    </Button>
                </Field>
            </FieldGroup>
        </form>
    );
}