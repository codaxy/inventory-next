import { Controller, History } from "cx/ui";

import { createClient, deleteClient, getClient, updateClient } from "../../../../api/clients";
import { ApiError, fieldErrors } from "../../../../api/http";
import { confirm } from "../../../../components/confirm";
import { guardLeaving } from "../../../../leaveGuard";
import { listReturn } from "../../../../listAddress";
import $app from "../../../../model";
import m, { type ClientDraft, type ClientEditorState, projectsText, toForm, toSections } from "./model";

const list = "~/company/clients";
const c = m.client;

export default class extends Controller {
    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        this.addTrigger("name-edited", [c.draft.name], () => this.store.delete(c.errors.name));
        this.addTrigger("address", [$app.url], () => this.open(), true);
    }

    onDestroy() {
        this.release?.();
    }

    private open() {
        const routed = this.store.get(m.$route.id);
        const id = routed === "new" ? null : routed;
        const viewing = !!id && !this.store.get($app.url).endsWith("/edit");

        this.store.set(c.id, id);
        this.store.set(c.viewing, viewing);
        this.store.set(c.loading, !!id);
        this.store.set(c.loaded, false);
        this.store.set(c.saving, false);
        this.store.delete(c.error);
        this.store.set(c.errors, {});
        this.store.set(c.visited, false);
        this.store.set(c.valid, true);
        this.store.set(c.sections, []);
        this.store.set(c.projectCount, 0);
        this.load({}, id ? "" : "New client");

        this.release?.();
        this.release = viewing ? undefined : guardLeaving(() => this.dirty());

        if (id)
            getClient(id)
                .then((client) => {
                    if (this.store.get(c.id) !== id) return;
                    this.load({ name: client.name }, client.name);
                    this.store.set(c.sections, toSections(client));
                    this.store.set(c.projectCount, client.projectCount);
                    this.store.set(c.loaded, true);
                })
                .catch((error) =>
                    this.store.set(
                        c.error,
                        error instanceof ApiError && error.status === 404
                            ? "This client no longer exists."
                            : "The client could not be loaded.",
                    ),
                )
                .finally(() => this.store.set(c.loading, false));
    }

    private load(draft: ClientDraft, title: string) {
        // Text keys stay absent rather than empty: '' is a value, and `required` would pass on it.
        const clean: ClientDraft = draft.name ? { name: draft.name } : {};
        this.store.set(c.draft, clean);
        this.store.set(c.title, title);
        this.saved = JSON.stringify(toForm(clean));
    }

    dirty() {
        return JSON.stringify(toForm(this.store.get(c.draft))) !== this.saved;
    }

    async save() {
        if (!this.store.get(c.valid)) return this.store.set(c.visited, true);

        const id = this.store.get(c.id);
        this.store.set(c.saving, true);
        this.store.delete(c.error);

        try {
            const form = toForm(this.store.get(c.draft));
            const saved = await (id ? updateClient(id, form) : createClient(form));
            this.leave(id ? `${list}/${saved.id}` : listReturn(list));
        } catch (error) {
            if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(c.errors, fieldErrors<ClientEditorState["errors"]>(error));
            else
                this.store.set(
                    c.error,
                    error instanceof ApiError && error.status === 404
                        ? "This client was deleted while you were editing it."
                        : "The client could not be saved.",
                );
        } finally {
            this.store.set(c.saving, false);
        }
    }

    async remove() {
        const id = this.store.get(c.id);
        if (!id || !this.store.get(c.loaded)) return;

        // Its projects keep it — and would go with it, the database cascading: say so first.
        const projects = this.store.get(c.projectCount);
        if (projects > 0) {
            await confirm({
                title: "This client has projects",
                message: `${projectsText(projects)} ${projects === 1 ? "is" : "are"} of it. Give ${projects === 1 ? "it" : "them"} another client first, or keep this one.`,
                cancelText: "Close",
            });
            return;
        }

        const confirmed = await confirm({
            title: "Delete this client?",
            message: "It has no projects. This cannot be undone.",
            confirmText: "Delete client",
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await deleteClient(id);
            this.leave(listReturn(list));
        } catch (error) {
            this.store.set(
                c.error,
                error instanceof ApiError && error.status === 409
                    ? error.message
                    : "The client could not be deleted.",
            );
        }
    }

    private leave(to: string) {
        this.release?.();
        this.release = undefined;
        History.pushState({}, null, to);
    }
}
