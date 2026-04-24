import { useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field"

type CreatePortfolioFormProps = {
    ref: RefObject<HTMLFormElement>;
    onSubmit: (name: string, description: string) => Promise<void>;
    isSubmitting: boolean;
    errorMessage: string | null;
}

export default function CreatePortfolioForm({
    ref,
    onSubmit,
    isSubmitting,
    errorMessage
}: CreatePortfolioFormProps) {
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");


    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        await onSubmit(name, description);
    }


    return (
        <form ref={ref} onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel htmlFor="portfolio-name">Portfolio name *</FieldLabel>
                    <Input
                        id="portfolio-name"
                        type="text"
                        value={name}
                        placeholder="My Portfolio"
                        disabled={isSubmitting}
                        onChange={(e) => setName(e.target.value)}
                        required />
                </Field>
                <Field>
                    <FieldLabel htmlFor="portfolio-description">Description</FieldLabel>
                    <Input
                        id="portfolio-description"
                        type="text"
                        value={description}
                        placeholder="A description of my portfolio"
                        disabled={isSubmitting}
                        onChange={(e) => setDescription(e.target.value)} />
                </Field>

                {errorMessage && <p className="text-destructive text-sm">{errorMessage}</p>}
            </FieldGroup>
        </form>
    );
}
