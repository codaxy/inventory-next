import { createModel } from "cx/ui";

import type { CurrentUser } from "./api/auth";

export interface Session {
    status: "loading" | "ok";
    /** `null` once resolved and nobody is signed in. */
    user?: CurrentUser | null;
}

/** The root store: the address, and the session every route depends on. */
export interface AppModel {
    url: string;
    session: Session;
}

export default createModel<AppModel>();
