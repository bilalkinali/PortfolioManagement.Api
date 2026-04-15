import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"

type LoginFormProps = {
    onLoginSuccess: () => void;
    onCancel: () => void;
}

export default function LoginForm({ onLoginSuccess, onCancel } : LoginFormProps) {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    async function handleLogin() {
        onLoginSuccess();
    }

    return (
        <FieldGroup>
            <Field>
                <FieldLabel htmlFor="login-email">Email</FieldLabel>
                <Input 
                    id="login-email"
                    type="email"
                    value={email}
                    placeholder="email@example.com"
                    onChange={(e) => setEmail(e.target.value)}/>
            </Field>
            <Field>
                <FieldLabel htmlFor="login-password">Password</FieldLabel>
                <Input 
                    id="login-password" 
                    type="password" 
                    value={password}
                    placeholder="******"
                    onChange={(e) => setPassword(e.target.value)}/>
            </Field>

            <Field orientation="horizontal">
                <Button
                    onClick={handleLogin}>
                    Login
                </Button>
                <Button 
                    variant="outline"
                    onClick={onCancel}>
                    Cancel
                </Button>
            </Field>
        </FieldGroup>
    )
}