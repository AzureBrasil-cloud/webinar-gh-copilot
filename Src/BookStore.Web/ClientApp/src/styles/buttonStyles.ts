// Shared action-button classes (Figma "dark slate" primary action + neutral secondary/outlined),
// reused by dialog footers, page header "New X" buttons, and the Home page CTAs.
const buttonBaseClass = "gap-1.75! rounded-md! px-[11.5px]! py-2! text-sm! font-medium!";

export const secondaryButtonClass = `${buttonBaseClass} border-surface-300!`;
export const primaryButtonClass = `${buttonBaseClass} border-surface-700! bg-surface-700! hover:bg-surface-800!`;
export const dangerButtonClass = buttonBaseClass;
