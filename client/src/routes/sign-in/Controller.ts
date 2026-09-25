import { Controller } from "cx/ui";

import { ApiError, getAuthProviders, requestOneTimeCode, verifyOneTimeCode } from "../../api/auth";
import m from "./model";

export default class extends Controller {
    onInit() {
        this.store.set(m.signin.providers, { google: false, oneTimeCode: false });
        this.store.set(m.signin.codeSent, false);
        this.store.set(m.signin.busy, false);
        this.store.set(m.signin.error, null);
        this.store.delete(m.signin.emailError);
        this.store.delete(m.signin.codeError);
        this.store.delete(m.signin.email);
        this.store.delete(m.signin.code);

        this.addTrigger("email-edited", [m.signin.email], () => this.store.delete(m.signin.emailError));
        this.addTrigger("code-edited", [m.signin.code], () => this.store.delete(m.signin.codeError));

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
            this.fail(error, m.signin.emailError);
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
            await verifyOneTimeCode(email, code ?? "");
            window.location.href = "/";
        } catch (error) {
            this.fail(error, m.signin.codeError);
        } finally {
            this.store.set(m.signin.busy, false);
        }
    }

    /** The API's answer goes under the field it is about; anything else above the form. */
    fail(error: unknown, field: typeof m.signin.emailError) {
        if (error instanceof ApiError) this.store.set(field, error.message);
        else this.store.set(m.signin.error, "Something went wrong.");
    }

    onStartOver() {
        this.store.set(m.signin.codeSent, false);
        this.store.delete(m.signin.email);
        this.store.delete(m.signin.code);
        this.store.set(m.signin.error, null);
    }
}
