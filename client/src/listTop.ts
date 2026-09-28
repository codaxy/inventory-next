/**
 * Publishes a list's pinned block (`list-top`) height as `--list-top-height` on its parent, for what
 * sticks beneath it where the document scrolls — the audit log's day headings. The height changes as
 * chips wrap and a filter comes and goes.
 *
 * Returns a ref callback for the block's element; cx calls it with `null` when the element goes.
 */
export function listTop() {
    let observer: ResizeObserver | undefined;

    return (block: HTMLElement | null) => {
        observer?.disconnect();
        observer = undefined;
        if (!block) return;

        const host = block.parentElement!;
        observer = new ResizeObserver(() =>
            host.style.setProperty("--list-top-height", `${block.offsetHeight}px`),
        );
        observer.observe(block);
    };
}
