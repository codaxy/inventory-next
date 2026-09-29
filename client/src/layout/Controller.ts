import { Controller, History } from "cx/ui";

import { signOut } from "../api/auth";
import { getVersion } from "../api/version";
import $app from "../model";
import { lockScroll } from "../scrollLock";

/** Only below `lg` is the navigation a drawer over the page. */
const drawerWidth = window.matchMedia("(max-width: 1023.98px)");

export default class extends Controller {
    private unlock?: () => void;
    private unsubscribe?: () => void;

    /**
     * The document scrolls, so while the drawer is open the page behind it is locked: otherwise a drag
     * on the backdrop scrolls the rows under the drawer, with Safari's toolbar resizing it as it goes.
     */
    onInit() {
        // Nothing depends on it: a failure leaves the line out rather than saying so.
        getVersion().then(
            (version) => this.store.set($app.version, version),
            () => {},
        );

        this.addTrigger("drawer-lock", [$app.ui.drawerOpen], (open) => {
            const drawer = document.querySelector<HTMLElement>(".nav-drawer");
            if (open && drawer && drawerWidth.matches) this.unlock ??= lockScroll(drawer);
            else this.release();
        });

        // The document scrolls, so it keeps its position from page to page: a new page opens at its
        // top. Only a navigation forward — `replaceState` is a list writing its own address, and Back
        // sends nothing, leaving the browser to restore where the reader was.
        this.unsubscribe = History.subscribe((_url, op) => {
            if (op === "pushState") window.scrollTo({ top: 0 });
        });
    }

    onDestroy() {
        this.release();
        this.unsubscribe?.();
    }

    private release() {
        this.unlock?.();
        this.unlock = undefined;
    }

    async onSignOut() {
        await signOut();

        // A full load rather than a store update: the session cookie is gone, so everything the app
        // holds about the person is stale.
        window.location.href = "/";
    }
}
