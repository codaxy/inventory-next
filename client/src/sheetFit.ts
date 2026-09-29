/** An A4 page's width in CSS pixels: 210mm at 96 per inch. */
const a4Width = (210 * 96) / 25.4;

/**
 * Scales a printable page (`.handover`) to its column, as a PDF viewer fits a page to the window: the
 * page keeps its A4 layout at every width and is zoomed down where the column is narrower, never up.
 * Publishes the factor as `--sheet-zoom` on the column.
 *
 * Returns a ref callback for the column's element; cx calls it with `null` when the element goes.
 */
export function sheetFit() {
    let observer: ResizeObserver | undefined;

    return (column: HTMLElement | null) => {
        observer?.disconnect();
        observer = undefined;
        if (!column) return;

        observer = new ResizeObserver(() =>
            column.style.setProperty("--sheet-zoom", String(Math.min(1, column.clientWidth / a4Width))),
        );
        observer.observe(column);
    };
}
