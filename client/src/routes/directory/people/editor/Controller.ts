import { Controller, History } from "cx/ui";

import { ApiError, fieldErrors } from "../../../../api/http";
import { createPerson, deletePerson, getHoldings, getPerson, updatePerson } from "../../../../api/people";
import { confirm } from "../../../../components/confirm";
import { guardLeaving } from "../../../../leaveGuard";
import { listReturn } from "../../../../listAddress";
import $app from "../../../../model";
import m, { type PersonDraft, type PersonEditorState, toForm, toHoldings } from "./model";

const list = "~/directory/people";
const p = m.person;

export default class extends Controller {
    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        this.addTrigger("name-edited", [p.draft.name], () => this.store.delete(p.errors.name));
        this.addTrigger("email-edited", [p.draft.email], () => this.store.delete(p.errors.email));

        this.addTrigger("address", [$app.url], () => this.open(), true);
    }

    onDestroy() {
        this.release?.();
    }

    /** The record the address names, in the mode it names; in view, what is attached to them too. */
    private open() {
        const routed = this.store.get(m.$route.id);
        const id = routed === "new" ? null : routed;
        const viewing = !!id && !this.store.get($app.url).endsWith("/edit");

        this.store.set(p.id, id);
        this.store.set(p.viewing, viewing);
        this.store.set(p.loading, !!id);
        this.store.set(p.saving, false);
        this.store.delete(p.error);
        this.store.set(p.errors, {});
        this.store.set(p.visited, false);
        this.store.set(p.valid, true);
        this.store.set(p.sections, []);
        this.store.delete(p.none);
        this.store.delete(p.holds);
        this.store.set(p.holdingsLoaded, false);
        this.load({}, id ? "" : "New person");

        this.release?.();
        this.release = viewing ? undefined : guardLeaving(() => this.dirty());

        if (!id) return;

        getPerson(id)
            .then((person) => {
                if (this.store.get(p.id) !== id) return;
                this.load({ name: person.name, email: person.email }, person.name);
            })
            .catch((error) =>
                this.store.set(
                    p.error,
                    error instanceof ApiError && error.status === 404
                        ? "This person no longer exists."
                        : "The person could not be loaded.",
                ),
            )
            .finally(() => this.store.set(p.loading, false));

        if (viewing)
            getHoldings(id)
                .then((holdings) => {
                    if (this.store.get(p.id) !== id) return;
                    const shaped = toHoldings(holdings, id);
                    this.store.set(p.sections, shaped.sections);
                    this.store.set(p.none, shaped.none);
                    this.store.set(p.holds, shaped.holds);
                    this.store.set(p.holdingsLoaded, true);
                })
                .catch(() => {});
    }

    private load(draft: PersonDraft, title: string) {
        // Text keys stay absent rather than empty: '' is a value, and `required` would pass on it.
        const clean: PersonDraft = {};
        if (draft.name) clean.name = draft.name;
        if (draft.email) clean.email = draft.email;

        this.store.set(p.draft, clean);
        this.store.set(p.title, title);
        this.saved = JSON.stringify(toForm(clean));
    }

    dirty() {
        return JSON.stringify(toForm(this.store.get(p.draft))) !== this.saved;
    }

    async save() {
        if (!this.store.get(p.valid)) return this.store.set(p.visited, true);

        const id = this.store.get(p.id);
        this.store.set(p.saving, true);
        this.store.delete(p.error);

        try {
            const form = toForm(this.store.get(p.draft));
            const saved = await (id ? updatePerson(id, form) : createPerson(form));
            this.leave(id ? `${list}/${saved.id}` : listReturn(list));
        } catch (error) {
            if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(p.errors, fieldErrors<PersonEditorState["errors"]>(error));
            else
                this.store.set(
                    p.error,
                    error instanceof ApiError && error.status === 404
                        ? "This person was deleted while you were editing them."
                        : "The person could not be saved.",
                );
        } finally {
            this.store.set(p.saving, false);
        }
    }

    async remove() {
        const id = this.store.get(p.id);
        if (!id || !this.store.get(p.holdingsLoaded)) return;

        // What they hold keeps them — and would go with them, the database cascading: say so first.
        const holds = this.store.get(p.holds);
        if (holds) {
            await confirm({
                title: "This person holds things",
                message: `They hold ${holds}. Give them to someone else first, or keep the person.`,
                cancelText: "Close",
            });
            return;
        }

        const confirmed = await confirm({
            title: "Delete this person?",
            message: "Nothing is attached to them. This cannot be undone.",
            confirmText: "Delete person",
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await deletePerson(id);
            this.leave(listReturn(list));
        } catch (error) {
            this.store.set(
                p.error,
                error instanceof ApiError && error.status === 409
                    ? error.message
                    : "The person could not be deleted.",
            );
        }
    }

    private leave(to: string) {
        this.release?.();
        this.release = undefined;
        History.pushState({}, null, to);
    }
}
