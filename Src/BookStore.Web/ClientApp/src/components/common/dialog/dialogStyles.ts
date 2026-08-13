// Shared PrimeVue Dialog `pt` shell + footer button classes, matching the Figma modal spec
// (rounded-[21px] white panel, 67px header, 3-gap footer). Only the width and content
// padding vary between dialogs (form dialogs use wider gaps than read-only "Details" dialogs),
// so callers pass those in.
import { primaryButtonClass, secondaryButtonClass, dangerButtonClass } from "../../../styles/buttonStyles";

const FORM_CONTENT_CLASS = "flex flex-col gap-[21px] px-6 py-2";
const DETAILS_CONTENT_CLASS = "flex flex-col gap-1.75 px-[21px] py-[17.5px]";

export function dialogShellPt(width: string, contentClass: string = FORM_CONTENT_CLASS) {
  return {
    root: { class: `${width} max-w-[92vw] rounded-[21px] border border-surface-300 bg-surface-0 p-0 overflow-hidden` },
    header: { class: "h-[67px] items-center justify-between pt-5 pb-4 pl-6 pr-4" },
    content: { class: contentClass },
    footer: { class: "gap-3 justify-end pb-5 pt-4 px-6" },
  };
}

export function detailsDialogPt(width: string) {
  return dialogShellPt(width, DETAILS_CONTENT_CLASS);
}

export const dialogSecondaryButtonClass = secondaryButtonClass;
export const dialogPrimaryButtonClass = primaryButtonClass;
export const dialogDangerButtonClass = dangerButtonClass;
