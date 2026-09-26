import { Controller } from "cx/ui";

import { signOut } from "../api/auth";
import $app from "../model";
import { lockScroll } from "../scrollLock";

/** Only below `lg` is the navigation a drawer over the page. */
const drawerWidth = window.matchMedia("(max-width: 1023.98px)");

export default class extends Controller {
    private unlock?: () => void;

    /**
     * The document scrolls, so while the drawer is open the page behind it is locked: otherwise a drag
     * on the backdrop scrolls the rows under the drawer, with Safari's toolbar resizing it as it goes.
     */
    onInit() {
        this.addTrigger("drawer-lock", [$app.ui.drawerOpen], (open) => {
            const drawer = document.querySelector<HTMLElement>(".nav-drawer");
            if (open && drawer && drawerWidth.matches) this.unlock ??= lockScroll(drawer);
            else this.release();
        });
    }

    onDestroy() {
        this.release();
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
