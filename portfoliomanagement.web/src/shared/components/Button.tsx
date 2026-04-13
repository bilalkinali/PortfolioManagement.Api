import React, { forwardRef, MouseEventHandler } from "react";

type Variant = "primary" | "secondary" | "danger" | "ghost";
type Size = "sm" | "md" | "lg";

export interface ButtonProps {
  children?: React.ReactNode;
  variant?: Variant;
  size?: Size;
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
  loading?: boolean;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  className?: string;
  startIcon?: React.ReactNode;
  endIcon?: React.ReactNode;
  "aria-label"?: string;
}

/**
 * Shared Button component used across the application.
 * - Accessible (supports aria-label and disabled state)
 * - ForwardRef to allow parent components to control focus
 * - Small API surface for common variants and sizes
 */
const variantClasses: Record<Variant, string> = {
  primary:
    "background: #1f6feb; color: #fff; border: 1px solid rgba(0,0,0,0.05);",
  secondary:
    "background: #e6eefc; color: #0b3b74; border: 1px solid rgba(11,59,116,0.08);",
  danger:
    "background: #ffebe9; color: #a12b2b; border: 1px solid rgba(161,43,43,0.08);",
  ghost: "background: transparent; color: #0b3b74; border: none;",
};

const sizeStyles: Record<Size, string> = {
  sm: "padding: 6px 10px; font-size: 12px;",
  md: "padding: 8px 14px; font-size: 14px;",
  lg: "padding: 12px 18px; font-size: 16px;",
};

const baseStyle =
  "display: inline-flex; align-items: center; justify-content: center; gap: 8px; border-radius: 6px; cursor: pointer; transition: opacity 150ms ease;";

const disabledStyle = "opacity: 0.6; cursor: not-allowed;";

const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  {
    children,
    variant = "primary",
    size = "md",
    type = "button",
    disabled = false,
    loading = false,
    onClick,
    className = "",
    startIcon,
    endIcon,
    ...rest
  },
  ref
) {
  const handleClick: MouseEventHandler<HTMLButtonElement> = (e) => {
    if (disabled || loading) {
      e.preventDefault();
      return;
    }
    onClick?.(e);
  };

  const styleString =
    [
      baseStyle,
      variantClasses[variant],
      sizeStyles[size],
      disabled || loading ? disabledStyle : "",
    ]
      .filter(Boolean)
      .join(" ");

  return (
    <button
      ref={ref}
      type={type}
      onClick={handleClick}
      disabled={disabled || loading}
      style={
        // Apply the computed inline style string as a CSS text string.
        // This keeps the component self-contained without requiring external CSS.
        (Object.assign({}, { cssText: styleString }) as unknown) as React.CSSProperties
      }
      className={className}
      {...rest}
    >
      {loading ? (
        <svg
          width="16"
          height="16"
          viewBox="0 0 50 50"
          aria-hidden="true"
          focusable="false"
        >
          <path
            fill="currentColor"
            d="M43.935,25.145c0-10.318-8.364-18.682-18.682-18.682c-10.318,0-18.682,8.364-18.682,18.682h4.068
            c0-8.063,6.551-14.613,14.613-14.613c8.063,0,14.613,6.55,14.613,14.613H43.935z"
          >
            <animateTransform
              attributeType="xml"
              attributeName="transform"
              type="rotate"
              from="0 25 25"
              to="360 25 25"
              dur="0.9s"
              repeatCount="indefinite"
            />
          </path>
        </svg>
      ) : null}
      {startIcon}
      {children}
      {endIcon}
    </button>
  );
});

export default Button;