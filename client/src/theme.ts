import { defaultPreset, densityComfortable } from "cx-theme-variables";

/**
 * CxJS's theme variables mapped onto the tokens in `tailwind.css`, so widgets and hand-written
 * markup draw from one palette. Values point at the tokens rather than repeating hex codes.
 *
 * Controls are 36px: `densityComfortable`'s 24px line height, 5px padding each side and the 1px
 * borders. A touch screen raises the padding to 9px, the 44px tap target, in `_surfaces.scss`.
 */
export const theme = {
    ...defaultPreset,
    ...densityComfortable,

    primaryColor: "var(--color-primary)",
    accentTextColor: "var(--color-primary-text)",
    textColor: "var(--color-ink)",
    backgroundColor: "var(--color-surface)",
    surfaceColor: "var(--color-surface)",
    borderColor: "var(--color-line)",
    dangerColor: "var(--color-danger)",
    dangerTextColor: "var(--color-danger-text)",
    successColor: "var(--color-success)",
    successTextColor: "var(--color-success-text)",
    warningColor: "var(--color-warn)",
    warningTextColor: "var(--color-warn-text)",
    focusBoxShadow: "0 0 0 2px var(--color-primary-text)",
    boxShadow: "var(--shadow-control)",
    overlayBoxShadow: "var(--shadow-overlay)",
    borderRadius: "8px",
    fontFamily: "var(--font-sans)",
    fontSize: "14px",
    fontWeight: "500",

    labelColor: "var(--color-ink-soft)",
    labelFontWeight: "600",
    placeholderColor: "var(--color-ink-faint)",

    inputColor: "var(--color-ink)",
    inputBackgroundColor: "var(--color-surface)",
    inputBorderColor: "var(--color-line-field)",
    inputPaddingX: "12px",
    inputPaddingY: "5px",

    buttonBackgroundColor: "var(--color-raised)",
    buttonColor: "var(--color-ink)",
    buttonBorderColor: "var(--color-line-field)",
    buttonFontWeight: "600",
    buttonPaddingY: "5px",

    itemHoverBackgroundColor: "var(--color-hover)",
    cursorBoxShadow: "none",

    windowBackgroundColor: "var(--color-surface)",
    windowBodyBackgroundColor: "var(--color-surface)",
    windowBorderColor: "var(--color-line)",
    windowHeaderBackgroundColor: "var(--color-surface)",
    windowFooterBackgroundColor: "var(--color-canvas)",
    // The preset's footer has no top padding, invisible on a transparent footer and flush-top on ours;
    // its header takes the accent colour, which here reads as a link.
    windowHeaderColor: "var(--color-ink)",
    windowHeaderFontSize: "17px",
    windowHeaderFontWeight: "700",
    windowHeaderPadding: "18px 20px 0",
    windowBodyPadding: "12px 20px 20px",
    windowFooterPadding: "12px 20px",
    windowFooterBorderWidth: "1px",
    tooltipBackgroundColor: "var(--color-raised)",
    calendarBackgroundColor: "var(--color-surface)",

    gridBackground: "var(--color-surface)",
    gridHeaderBackgroundColor: "var(--color-canvas)",
    gridHeaderColor: "var(--color-ink-muted)",
    // A row is read, not touched: it keeps its height when the controls shrink, which the preset's
    // derivation from the input padding would not.
    gridHeaderPaddingY: "9px",
    gridDataPaddingY: "9px",
    gridHeaderBorderColor: "var(--color-line)",
    gridDataBorderColor: "var(--color-line-soft)",
    gridDataAlternateBackgroundColor: "transparent",
};
