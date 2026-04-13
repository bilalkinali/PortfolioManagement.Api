import React, { ReactNode, useEffect, useRef } from "react";
import ReactDOM from "react-dom";

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children?: ReactNode;
  /**
   * width can be e.g. '500px' or '50%'
   */
  width?: string;
  /**
   * Prevent closing when clicking the overlay
   */
  disableOverlayClose?: boolean;
  /**
   * Additional className for modal content container
   */
  className?: string;
}

/**
 * Simple accessible modal component.
 * - Creates its own portal container appended to document.body
 * - Closes on ESC (unless prevented)
 * - Prevents body scroll while open
 * - Focus management: restores focus to previously focused element
 */
export default function Modal({
  isOpen,
  onClose,
  title,
  children,
  width = "640px",
  disableOverlayClose = false,
  className,
}: ModalProps) {
  const portalRef = useRef<HTMLDivElement | null>(null);
  const previouslyFocused = useRef<Element | null>(null);
  const contentRef = useRef<HTMLDivElement | null>(null);
  const titleId = useRef<string>("modal-title-" + Math.random().toString(36).slice(2));

  // Create portal container
  useEffect(() => {
    const container = document.createElement("div");
    container.setAttribute("data-modal-portal", "");
    portalRef.current = container;
    document.body.appendChild(container);
    return () => {
      if (portalRef.current && portalRef.current.parentNode) {
        portalRef.current.parentNode.removeChild(portalRef.current);
      }
      portalRef.current = null;
    };
  }, []);

  // Open/close side effects: body scroll lock, focus management, Esc handling
  useEffect(() => {
    if (!isOpen) return;

    previouslyFocused.current = document.activeElement;

    // lock scroll
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    // focus first focusable element or content container
    const focusableSelector =
      'a[href], button:not([disabled]), textarea, input[type="text"], input[type="radio"], input[type="checkbox"], select, [tabindex]:not([tabindex="-1"])';
    const firstFocusable =
      contentRef.current?.querySelector<HTMLElement>(focusableSelector) ?? contentRef.current;
    firstFocusable?.focus();

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        e.stopPropagation();
        onClose();
      }
      if (e.key === "Tab") {
        // basic focus trap
        const focusable = contentRef.current?.querySelectorAll<HTMLElement>(focusableSelector);
        if (!focusable || focusable.length === 0) return;
        const first = focusable[0]!;
        const last = focusable[focusable.length - 1]!;
        if (e.shiftKey && document.activeElement === first) {
          e.preventDefault();
          last.focus();
        } else if (!e.shiftKey && document.activeElement === last) {
          e.preventDefault();
          first.focus();
        }
      }
    };

    document.addEventListener("keydown", onKeyDown, true);

    return () => {
      document.body.style.overflow = previousOverflow;
      document.removeEventListener("keydown", onKeyDown, true);
      // restore focus
      if (previouslyFocused.current && (previouslyFocused.current as HTMLElement).focus) {
        (previouslyFocused.current as HTMLElement).focus();
      }
    };
  }, [isOpen, onClose]);

  if (!portalRef.current) {
    // portal not ready yet
    return null;
  }

  if (!isOpen) {
    return null;
  }

  const overlayClick = (e: React.MouseEvent) => {
    if (disableOverlayClose) return;
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  const contentStyle: React.CSSProperties = {
    width,
    maxWidth: "calc(100% - 32px)",
    background: "#fff",
    borderRadius: 8,
    padding: 20,
    boxShadow: "0 10px 30px rgba(0,0,0,0.2)",
    outline: "none",
  };

  const overlayStyle: React.CSSProperties = {
    position: "fixed",
    inset: 0,
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    background: "rgba(0,0,0,0.5)",
    zIndex: 1000,
    padding: 16,
  };

  const headerStyle: React.CSSProperties = {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    marginBottom: 12,
  };

  const closeButtonStyle: React.CSSProperties = {
    background: "transparent",
    border: "none",
    fontSize: 18,
    cursor: "pointer",
    lineHeight: 1,
  };

  const modalContent = (
    <div style={overlayStyle} onMouseDown={overlayClick} aria-hidden={false}>
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={title ? titleId.current : undefined}
        ref={contentRef}
        tabIndex={-1}
        className={className}
        style={contentStyle}
        onMouseDown={(e) => e.stopPropagation()}
      >
        <div style={headerStyle}>
          {title ? (
            <h2 id={titleId.current} style={{ margin: 0, fontSize: 18 }}>
              {title}
            </h2>
          ) : (
            <div />
          )}
          <button
            aria-label="Close modal"
            onClick={onClose}
            style={closeButtonStyle}
            type="button"
          >
            ×
          </button>
        </div>
        <div>{children}</div>
      </div>
    </div>
  );

  return ReactDOM.createPortal(modalContent, portalRef.current);
}