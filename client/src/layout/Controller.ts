import { Controller } from "cx/ui";

import { signOut } from "../api/auth";

export default class extends Controller {
    async onSignOut() {
        await signOut();

        // A full load rather than a store update: the session cookie is gone, so everything the app
        // holds about the person is stale.
        window.location.href = "/";
    }
}
