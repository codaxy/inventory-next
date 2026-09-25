import { Controller } from "cx/ui";

import { ApiError, getAuthProviders, requestOneTimeCode, verifyOneTimeCode } from "../../api/auth";
import m from "./model";

export default class extends Controller {
    onInit() {
        this.store.set(m.signin.providers, { google: false, oneTimeCode: false });
        this.store.set(m.signin.codeSent, false);
        this.store.set(m.signin.busy, false);
        this.store.set(m.signin.error, null);
        this.store.delete(m.signin.email);
        this.store.delete(m.signin.code);

        getAuthProviders().then((providers) => this.store.set(m.signin.providers, providers));

        // Google redirects back here with a reason rather than a stack trace; the query is the only
        // thing that survives the round trip.
        const error = new URLSearchParams(window.location.search).get("error");

        if (error)
            this.store.set(
                m.signin.error,
                error === "refused"
                    ? "That account is not allowed to sign in."
                    : "Signing in with Google did not work.",
            );
    }

    async onRequestCode() {
        const email = this.store.get(m.signin.email) ?? "";

        this.store.set(m.signin.busy, true);
        this.store.set(m.signin.error, null);

        try {
            await requestOneTimeCode(email);
            this.store.set(m.signin.codeSent, true);
        } catch (error) {
            this.store.set(
                m.signin.error,
                error instanceof ApiError ? error.message : "Something went wrong.",
            );
        } finally {
            this.store.set(m.signin.busy, false);
        }
    }

    async onVerifyCode() {
        const email = this.store.get(m.signin.email) ?? "";
        const code = this.store.get(m.signin.code);

        this.store.set(m.signin.busy, true);
        this.store.set(m.signin.error, null);

        try {
            // Six digits with a leading zero are still six digits; the field holds a number.
            await verifyOneTimeCode(email, String(code ?? "").padStart(6, "0"));
            window.location.href = "/";
        } catch (error) {
            this.store.set(
                m.signin.error,
                error instanceof ApiError ? error.message : "Something went wrong.",
            );
        } finally {
            this.store.set(m.signin.busy, false);
        }
    }

    onStartOver() {
        this.store.set(m.signin.codeSent, false);
        this.store.delete(m.signin.code);
        this.store.set(m.signin.error, null);
    }
}
