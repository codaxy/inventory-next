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
    code?: string | null;
    codeSent: boolean;
    busy: boolean;
    /** Written by the form's `ValidationGroup`. */
    invalid: boolean;
    /** A failure that is not about a field: Google's refusal, a network error. */
    error: string | null;
    /** The server's answer about a field, shown under it and deleted when it is edited. */
    emailError?: string;
    codeError?: string;
}

export interface Model {
    signin: SignInState;
}

export default createModel<Model>();
