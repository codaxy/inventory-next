/**
 * Stops the page scrolling behind an overlay by refusing the gestures, not by freezing the page:
 * freezing it — `position: fixed` on the body — makes the document unscrollable, and iPhone Safari
 * answers by expanding its collapsed toolbar. `overflow: hidden` is no better: it jumps to the top and
 * Safari still scrolls on a drag.
 *
 * A touch or wheel scroll is let through only inside a scrollable element of `overlay`, or of a cx
 * overlay opened over it, that can still move in that direction; everything else — the backdrop, the
 * overlay's fixed parts, a list already at its end — is cancelled before it can reach the page.
 */
export function lockScroll(overlay: HTMLElement): () => void {
    let startY = 0;

    const onTouchStart = (e: TouchEvent) => {
        startY = e.touches[0]?.clientY ?? 0;
    };

    const onTouchMove = (e: TouchEvent) => {
        const y = e.touches[0]?.clientY ?? startY;
        // A finger moving down scrolls content up.
        // Not cancelable once the browser is already scrolling a list inside the overlay; at its end
        // `overscroll-behavior: contain` keeps that scroll from reaching the page instead.
        if (e.cancelable && !canScroll(e.target, overlay, startY - y)) e.preventDefault();
    };

    const onWheel = (e: WheelEvent) => {
        if (!canScroll(e.target, overlay, e.deltaY)) e.preventDefault();
    };

    const options = { passive: false, capture: true } as const;
    document.addEventListener("touchstart", onTouchStart, options);
    document.addEventListener("touchmove", onTouchMove, options);
    document.addEventListener("wheel", onWheel, options);

    return () => {
        document.removeEventListener("touchstart", onTouchStart, options);
        document.removeEventListener("touchmove", onTouchMove, options);
        document.removeEventListener("wheel", onWheel, options);
    };
}

/**
 * cx overlays opened over a locked page — a window, a picker's list, which on a touch screen is
 * attached to the body rather than to its field — are allowed regions like `overlay` itself.
 */
const overlays = ".cxb-window, .cxb-dropdown";

/**
 * Whether a scroll of `delta` (positive: down) starting at `target` moves something: the nearest
 * scrollable element inside an allowed region, if it can still move that way.
 */
function canScroll(target: EventTarget | null, overlay: HTMLElement, delta: number): boolean {
    if (delta === 0) return true;

    for (let el = target instanceof HTMLElement ? target : null; el; el = el.parentElement) {
        const { overflowY } = getComputedStyle(el);
        if ((overflowY === "auto" || overflowY === "scroll") && el.scrollHeight > el.clientHeight)
            return (
                inRegion(el, overlay) &&
                (delta < 0 ? el.scrollTop > 0 : el.scrollTop + el.clientHeight < el.scrollHeight - 1)
            );

        // The edge of a region with nothing scrollable inside it: the gesture would reach the page.
        if (el === overlay || el.matches(overlays)) return false;
    }

    return false;
}

const inRegion = (el: HTMLElement, overlay: HTMLElement) => overlay.contains(el) || !!el.closest(overlays);
