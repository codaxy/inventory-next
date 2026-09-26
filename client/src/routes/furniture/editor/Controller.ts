import { Controller, History } from "cx/ui";

import {
    createFurniture,
    deleteFurniture,
    type FurnitureDetail,
    getFurniture,
    getFurnitureOptions,
    updateFurniture,
} from "../../../api/furniture";
import { ApiError, fieldErrors } from "../../../api/http";
import { importanceFor } from "../../../assets";
import { confirm } from "../../../components/confirm";
import { guardLeaving } from "../../../leaveGuard";
import { listReturn, queryOf } from "../../../listAddress";
import $app from "../../../model";
import m, { blankDraft, type Draft, emptyOptions, toDraft, toForm } from "./model";

const list = "~/furniture";
const f = m.furniture;

export default class extends Controller {
    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        this.store.set(f.options, emptyOptions);

        // The server's messages go once the reader changes the form; the importance follows the weights.
        this.addTrigger("draft-edited", [f.draft], () => this.store.set(f.errors, {}));
        this.addTrigger("weights", [f.draft, f.options], (draft, options) =>
            this.store.set(f.importance, draft ? importanceFor(draft, options ?? emptyOptions) : "—"),
        );

        this.addTrigger("address", [$app.url], () => this.open(), true);

        getFurnitureOptions()
            .then((o) => this.store.set(f.options, o))
            .catch(() => {});
    }

    onDestroy() {
        this.release?.();
    }

    /** The record the address names, in the mode it names: `:id`, `:id/edit`, `new`, `new?from=:id`. */
    private open() {
        const routed = this.store.get(m.$route.id);
        const id = routed === "new" ? null : routed;
        const url = this.store.get($app.url);
        const viewing = !!id && !url.endsWith("/edit");
        // From the store, not `window.location`: cx moves the browser's address only once the page has rendered.
        const from = id ? null : queryOf(url).get("from");

        this.store.set(f.id, id);
        this.store.set(f.viewing, viewing);
        this.store.set(f.loading, !!(id || from));
        this.store.set(f.saving, false);
        this.store.set(f.stale, false);
        this.store.delete(f.error);
        this.store.set(f.errors, {});
        this.store.set(f.visited, false);
        this.store.set(f.valid, true);
        this.store.delete(f.number);
        this.store.delete(f.lastModified);
        this.load(blankDraft(), id ? "" : "New furniture");

        this.release?.();
        this.release = viewing ? undefined : guardLeaving(() => this.dirty());

        const source = id ?? from;
        if (source)
            getFurniture(source)
                .then((furniture) => {
                    if (this.store.get(f.id) !== id) return;
                    this.show(furniture, !id);
                })
                .catch((error) =>
                    this.store.set(
                        f.error,
                        error instanceof ApiError && error.status === 404
                            ? "This furniture no longer exists."
                            : "The furniture could not be loaded.",
                    ),
                )
                .finally(() => this.store.set(f.loading, false));
    }

    /** A loaded piece into the form; as a duplicate, everything but the number. */
    private show(furniture: FurnitureDetail, duplicate: boolean) {
        this.load(toDraft(furniture), duplicate ? "New furniture" : furniture.name);
        if (duplicate) return;

        this.store.set(f.number, furniture.number ? `#${furniture.number}` : undefined);
        this.store.set(f.lastModified, furniture.lastModified);
    }

    private load(draft: Draft, title: string) {
        this.store.set(f.draft, draft);
        this.store.set(f.title, title);
        this.saved = JSON.stringify(toForm(draft));
    }

    dirty() {
        return JSON.stringify(toForm(this.store.get(f.draft))) !== this.saved;
    }

    async save() {
        if (!this.store.get(f.valid)) return this.store.set(f.visited, true);

        const id = this.store.get(f.id);
        this.store.set(f.saving, true);
        this.store.delete(f.error);
        this.store.set(f.stale, false);

        try {
            const form = toForm(this.store.get(f.draft), this.store.get(f.lastModified));
            const saved = await (id ? updateFurniture(id, form) : createFurniture(form));
            this.leave(id ? `${list}/${saved.id}` : listReturn(list));
        } catch (error) {
            if (error instanceof ApiError && error.status === 409) {
                this.store.set(
                    f.error,
                    "Someone saved this furniture since you opened it. Reload to see their changes; yours are not saved.",
                );
                this.store.set(f.stale, true);
            } else if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(f.errors, fieldErrors<Record<string, string>>(error));
            else
                this.store.set(
                    f.error,
                    error instanceof ApiError && error.status === 404
                        ? "This furniture was deleted while you were editing it."
                        : "The furniture could not be saved.",
                );
        } finally {
            this.store.set(f.saving, false);
        }
    }

    /** After a refused save: the furniture as it is now, the reader's edits discarded. */
    reload() {
        this.release?.();
        this.release = undefined;
        this.open();
    }

    async remove() {
        const id = this.store.get(f.id);
        if (!id) return;

        const confirmed = await confirm({
            title: "Delete this furniture?",
            message: "This cannot be undone.",
            confirmText: "Delete furniture",
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await deleteFurniture(id);
            this.leave(listReturn(list));
        } catch (error) {
            this.store.set(
                f.error,
                error instanceof ApiError && error.status === 409
                    ? error.message
                    : "The furniture could not be deleted.",
            );
        }
    }

    private leave(to: string) {
        this.release?.();
        this.release = undefined;
        History.pushState({}, null, to);
    }
}
