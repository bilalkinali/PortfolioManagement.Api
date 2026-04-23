import { useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"

type RegisterFormProps = {
    ref: RefObject<HTMLFormElement>;
    onSubmit: (
        firstname: string,
        lastname: string,
        email: string,
        username: string,
        password: string) => Promise<void>;
    isSubmitting: boolean;
    errorMessage: string | null;
}

export default function RegisterForm({
    ref,
    onSubmit,
    isSubmitting,
    errorMessage
}: RegisterFormProps) {
    const [firstname, setFirstname] = useState("");
    const [lastname, setLastname] = useState("");
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");


    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        await onSubmit(firstname, lastname, email, username, password);
    }

    return (
        <form ref={ref} onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel htmlFor="register-firstname">Firstname *</FieldLabel>
                    <Input
                        id="register-firstname"
                        type="firstname"
                        value={firstname}
                        placeholder="Alice"
                        disabled={isSubmitting}
                        onChange={(e) => setFirstname(e.target.value)}
                        required />
                </Field>
                <Field>
                    <FieldLabel htmlFor="register-lastname">Lastname *</FieldLabel>
                    <Input
                        id="register-lastname"
                        type="lastname"
                        value={lastname}
                        placeholder="Alisson"
                        disabled={isSubmitting}
                        onChange={(e) => setLastname(e.target.value)}
                        required />
                </Field>
                <Field>
                    <FieldLabel htmlFor="register-email">Email *</FieldLabel>
                    <Input
                        id="register-email"
                        type="email"
                        value={email}
                        placeholder="email@example.com"
                        disabled={isSubmitting}
                        onChange={(e) => setEmail(e.target.value)}
                        required />
                </Field>
                <Field>
                    <FieldLabel htmlFor="register-username">Username *</FieldLabel>
                    <Input
                        id="register-username"
                        type="username"
                        value={username}
                        placeholder="johndoe"
                        disabled={isSubmitting}
                        onChange={(e) => setUsername(e.target.value)}
                        required />
                </Field>
                <Field>
                    <FieldLabel htmlFor="register-password">Password *</FieldLabel>
                    <Input
                        id="register-password"
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
    );
}