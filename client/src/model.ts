import { createModel } from "cx/ui";

import type { CurrentUser } from "./api/auth";

export interface Session {
    status: "loading" | "ok";
    /** `null` once resolved and nobody is signed in. */
    user?: CurrentUser | null;
}

/** The root store: the address, the session every route depends on, and the shell's own state. */
export interface AppModel {
    url: string;
    session: Session;
    ui: {
        /** The navigation drawer below `lg`; above it the navigation is always shown. */
        drawerOpen: boolean;
    };
}

export default createModel<AppModel>();
