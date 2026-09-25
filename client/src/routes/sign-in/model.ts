import { createModel } from "cx/ui";

import type { AuthProviders } from "../../api/auth";

/**
 * The screen's state, on one branch of the root store. The screen renders for the anonymous
 * session rather than inside a route of its own, so there is no sandbox to hold it: the branch
 * outlives the screen, and the controller resets it on init.
 */
export interface SignInState {
    providers: AuthProviders;
    /** Never initialised: an absent key is what `required` treats as empty. */
    email?: string | null;
    code?: number | null;
    codeSent: boolean;
    busy: boolean;
    /** Written by the form's `ValidationGroup`. */
    invalid: boolean;
    error: string | null;
}

export interface Model {
    signin: SignInState;
}

export default createModel<Model>();
