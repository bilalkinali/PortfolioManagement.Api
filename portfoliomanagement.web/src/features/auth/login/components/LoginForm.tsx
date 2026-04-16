import { useState, type FormEvent } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"
import { login as loginRequest } from "../api/login"
import type { LoginRequest } from "../api/login"
import { useAuth } from "../../shared/auth-context"

type LoginFormProps = {
    onLoginSuccess: () => void;
    onCancel: () => void;
}

export default function LoginForm({ onLoginSuccess, onCancel } : LoginFormProps) {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const { login } = useAuth();

    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();

        setErrorMessage(null);
        setIsSubmitting(true);

        const request: LoginRequest = {
            email,
            password
        };

        try {
            const response = await loginRequest(request);

            login(response.token,
                {
                    email: response.email,
                    firstName: response.firstName,
                    lastName: response.lastName
                }
            );

            onLoginSuccess();

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
        <form onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel htmlFor="login-email">Email</FieldLabel>
                    <Input
                        id="login-email"
                        type="email"
                        value={email}
                        placeholder="email@example.com"
                        onChange={(e) => setEmail(e.target.value)} />
                </Field>
                <Field>
                    <FieldLabel htmlFor="login-password">Password</FieldLabel>
                    <Input
                        id="login-password"
                        type="password"
                        value={password}
                        placeholder="******"
                        onChange={(e) => setPassword(e.target.value)} />
                </Field>

                {errorMessage && <p>{errorMessage}</p>}

                <Field orientation="horizontal">
                    <Button
                        type="submit"
                        disabled={isSubmitting}>
                        {isSubmitting ? "Logging in..." : "Login"}
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