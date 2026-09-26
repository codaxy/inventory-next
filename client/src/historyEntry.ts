/**
 * Gives an overlay its own entry in the browser history, at the same URL, so Back closes the
 * overlay instead of leaving the screen — what a phone's back gesture is expected to do. Pair with
 * the window's `dismissOnPopState`, which only closes it.
 */
export function historyEntry() {
    let popped = false;
    const onPop = () => {
        popped = true;
    };

    window.history.pushState({ overlay: true }, "", window.location.href);
    window.addEventListener("popstate", onPop, { once: true });

    return {
        /**
         * Call once the overlay has closed. Closed any way but Back, the entry is still there and is
         * stepped back over first; `done` waits for that, since an overlay opened before the step
         * lands would be closed by it.
         */
        release(done: () => void) {
            if (popped) return done();

            window.removeEventListener("popstate", onPop);
            window.addEventListener("popstate", () => done(), { once: true });
            window.history.back();
        },
    };
}
