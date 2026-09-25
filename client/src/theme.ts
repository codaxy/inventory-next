import { defaultPreset, densityComfortable } from "cx-theme-variables";

/**
 * CxJS's theme variables mapped onto the tokens in `tailwind.css`, so widgets and hand-written
 * markup draw from one palette. Values point at the tokens rather than repeating hex codes.
 *
 * Controls are 44px — the tap target `web-client.md` requires — which no density preset reaches:
 * `densityComfortable`'s 24px line height plus 9px padding each side and the 1px borders.
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
    inputBorderColor: "var(--color-line-strong)",
    inputPaddingX: "12px",
    inputPaddingY: "9px",

    buttonBackgroundColor: "var(--color-raised)",
    buttonColor: "var(--color-ink)",
    buttonBorderColor: "var(--color-line-strong)",
    buttonFontWeight: "600",
    buttonPaddingY: "9px",

    itemHoverBackgroundColor: "var(--color-hover)",
    cursorBoxShadow: "none",

    windowBackgroundColor: "var(--color-surface)",
    windowBodyBackgroundColor: "var(--color-surface)",
    windowBorderColor: "var(--color-line)",
    windowHeaderBackgroundColor: "var(--color-surface)",
    windowFooterBackgroundColor: "var(--color-canvas)",
    tooltipBackgroundColor: "var(--color-raised)",
    calendarBackgroundColor: "var(--color-surface)",

    gridBackground: "var(--color-surface)",
    gridHeaderBackgroundColor: "var(--color-canvas)",
    gridHeaderColor: "var(--color-ink-muted)",
    gridHeaderBorderColor: "var(--color-line)",
    gridDataBorderColor: "var(--color-line-soft)",
    gridDataAlternateBackgroundColor: "transparent",
};
