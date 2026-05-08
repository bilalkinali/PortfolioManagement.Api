import { useState, type FormEvent, type RefObject } from "react"
import { Input } from "@/components/ui/input"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field"

type CreatePortfolioFormProps = {
    ref: RefObject<HTMLFormElement | null>;
    onSubmit: (name: string, description: string) => Promise<void>;
    isSubmitting: boolean;
    errorMessage: string | null;
}

type CreatePortfolioFormErrors = {
    name?: string;
    description?: string;
}

function validatePortfolioForm(name: string, description: string): CreatePortfolioFormErrors {
    const errors: CreatePortfolioFormErrors = {};

    if (name.trim() === "") {
        errors.name = "Portfolio name is required.";
    } else if (name.length > 50) {
        errors.name = "Portfolio name cannot exceed 100 characters.";
    }

    if (description.length > 100) {
        errors.description = "Description cannot exceed 500 characters.";
    }

    return errors;
}

function hasValidationErrors(errors: CreatePortfolioFormErrors) {
    return Boolean(errors.name || errors.description);
}

export default function CreatePortfolioForm({
    ref,
    onSubmit,
    isSubmitting,
    errorMessage
}: CreatePortfolioFormProps) {
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [validationErrors, setValidationErrors] = useState<CreatePortfolioFormErrors>({});


    async function handleSubmit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();

        const nextValidationErrors = validatePortfolioForm(name, description);
        setValidationErrors(nextValidationErrors);

        if (hasValidationErrors(nextValidationErrors)) {
            return;
        }

        await onSubmit(name, description);
    }


    return (
        <form ref={ref} onSubmit={handleSubmit}>
            <FieldGroup>
                <Field data-invalid={Boolean(validationErrors.name)}>
                    <FieldLabel htmlFor="portfolio-name">Portfolio name *</FieldLabel>
                    <Input
                        id="portfolio-name"
                        type="text"
                        value={name}
                        placeholder="My Portfolio"
                        disabled={isSubmitting}
                        aria-invalid={Boolean(validationErrors.name)}
                        onChange={(e) => {
                            setName(e.target.value)
                            setValidationErrors((current) => ({ ...current, name: undefined }))
                        }} />
                    <FieldError>{validationErrors.name}</FieldError>
                </Field>
                <Field data-invalid={Boolean(validationErrors.description)}>
                    <FieldLabel htmlFor="portfolio-description">Description</FieldLabel>
                    <Input
                        id="portfolio-description"
                        type="text"
                        value={description}
                        placeholder="A description of my portfolio"
                        disabled={isSubmitting}
                        aria-invalid={Boolean(validationErrors.description)}
                        onChange={(e) => {
                            setDescription(e.target.value)
                            setValidationErrors((current) => ({ ...current, description: undefined }))
                        }} />
                    <FieldError>{validationErrors.description}</FieldError>
                </Field>

                {errorMessage && <p className="text-destructive text-sm">{errorMessage}</p>}
            </FieldGroup>
        </form>
    );
}
