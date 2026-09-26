import { History } from "cx/ui";
import { confirm } from "./components/confirm";

/**
 * Asks before an editor with unsaved changes is left: an in-app link through cx's navigation
 * confirmation, a reload or a closed tab through the browser's own prompt. Browser Back is neither —
 * cx cannot hold a navigation the browser has already made — so it leaves without asking.
 *
 * Returns the release; call it before navigating away on purpose, after a save or a delete.
 */
export function guardLeaving(isDirty: () => boolean): () => void {
    const onBeforeUnload = (e: BeforeUnloadEvent) => {
        if (isDirty()) e.preventDefault();
    };

    window.addEventListener("beforeunload", onBeforeUnload);
    History.addNavigateConfirmation(
        () =>
            !isDirty() ||
            confirm({
                title: "Unsaved changes",
                message: "Leave and lose what you changed?",
                confirmText: "Discard changes",
                cancelText: "Keep editing",
                danger: true,
            }),
        true,
    );

    return () => {
        window.removeEventListener("beforeunload", onBeforeUnload);
        History.addNavigateConfirmation(null as any, false);
    };
}
