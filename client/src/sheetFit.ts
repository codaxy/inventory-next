/** An A4 page's width in CSS pixels: 210mm at 96 per inch. */
const a4Width = (210 * 96) / 25.4;

/**
 * Scales a printable page (`.handover`) to its column, as a PDF viewer fits a page to the width: the
 * page keeps its A4 layout at every width and spans the column exactly, so its edge is the header
 * band's — zoomed down on a phone, a little up on a desktop's 52rem. Publishes the factor as
 * `--sheet-zoom` on the column; print resets it.
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
            column.style.setProperty("--sheet-zoom", String(column.clientWidth / a4Width)),
        );
        observer.observe(column);
    };
}
