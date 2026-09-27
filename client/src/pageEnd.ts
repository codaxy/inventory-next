/**
 * Marks the document `page-end` while it is scrolled to its last screen, so the root's background —
 * which a bounce past either end shows — can be the chrome's navy above the page and the page colour
 * below it. A browser paints nothing but that background beyond the page, a shadow or a pseudo-element
 * reaching there included, so one colour has to be swapped for the other.
 *
 * A page with nothing to scroll is never at its end: a bounce there is most likely a pull at the top.
 *
 * Returns a ref callback for the shell's element, whose size is watched: the page grows and shrinks
 * as a list loads or a filter narrows it, and a browser restoring the scroll position after a reload
 * sends no scroll event. The body's size says nothing — it is the window's height, and the page
 * overflows it.
 */
export function pageEnd() {
    let release: (() => void) | undefined;

    return (shell: HTMLElement | null) => {
        release?.();
        release = shell ? watch(shell) : undefined;
    };
}

function watch(shell: HTMLElement): () => void {
    const root = document.documentElement;

    const update = () => {
        const end = window.scrollY > 0 && window.scrollY + window.innerHeight >= root.scrollHeight - 1;
        root.classList.toggle("page-end", end);
    };

    // A gesture that meets the end moves nothing and sends no scroll event, so the start of one checks
    // too: a reload restored to the end would otherwise bounce navy until the reader scrolled away.
    const resize = new ResizeObserver(update);
    resize.observe(shell);
    const events = ["scroll", "wheel", "touchstart", "resize"] as const;
    for (const e of events) window.addEventListener(e, update, { passive: true });
    update();

    return () => {
        resize.disconnect();
        for (const e of events) window.removeEventListener(e, update);
        root.classList.remove("page-end");
    };
}
