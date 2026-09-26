/** Below this width the bar gives the screen back while scrolling down. */
const narrow = "(max-width: 767.98px)";

/** Scrolled this far in one direction before the bar reacts, so a jittery thumb does not flicker it. */
const threshold = 8;

/**
 * Pins a list's toolbar beneath whatever the shell pins at the top of the document. Its height is published as
 * `--sticky-bar-height` on its parent, so headings further down can stick beneath it. On a narrow
 * screen it slides away while the reader scrolls down and returns on the first scroll up, the way a
 * phone browser's address bar does; a bar marked `list-bar-static` — its filters open — stays put.
 *
 * Returns a ref callback for the bar's element; cx calls it with `null` when the element goes.
 */
export function stickyBar() {
    let release: (() => void) | undefined;

    return (bar: HTMLElement | null) => {
        release?.();
        release = bar ? pin(bar) : undefined;
    };
}

function pin(bar: HTMLElement): () => void {
    const host = bar.parentElement!;
    const media = window.matchMedia(narrow);
    let last = window.scrollY;
    let hidden = false;

    // Where the bar sits before it sticks, less the offset it sticks at: the scroll position at which it
    // starts to stick. Measured once: `offsetTop` follows a stuck element, so comparing against it live
    // is never true.
    const sticksAt = bar.getBoundingClientRect().top + window.scrollY - parseFloat(getComputedStyle(bar).top);

    const publish = () =>
        host.style.setProperty("--sticky-bar-height", hidden ? "0px" : `${bar.offsetHeight}px`);

    const setHidden = (value: boolean) => {
        if (value === hidden) return;
        hidden = value;
        bar.classList.toggle("list-bar-hidden", value);
        publish();
    };

    const onScroll = () => {
        const top = window.scrollY;
        const delta = top - last;
        if (Math.abs(delta) < threshold) return;
        last = top;

        const canHide = media.matches && !bar.classList.contains("list-bar-static") && top > sticksAt;
        setHidden(canHide && delta > 0);
    };

    const onMedia = () => setHidden(false);
    const resize = new ResizeObserver(publish);

    resize.observe(bar);
    window.addEventListener("scroll", onScroll, { passive: true });
    media.addEventListener("change", onMedia);
    publish();

    return () => {
        resize.disconnect();
        window.removeEventListener("scroll", onScroll);
        media.removeEventListener("change", onMedia);
    };
}
