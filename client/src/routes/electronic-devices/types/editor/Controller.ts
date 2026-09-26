import { Controller, History } from "cx/ui";

import {
    createType,
    deleteType,
    getType,
    getTypeOptions,
    updateType,
} from "../../../../api/electronicDeviceTypes";
import { ApiError, fieldErrors } from "../../../../api/http";
import { confirm } from "../../../../components/confirm";
import { guardLeaving } from "../../../../leaveGuard";
import { listReturn } from "../../../../listAddress";
import $app from "../../../../model";
import m, { devicesText, type TypeDraft, type TypeEditorState, toForm } from "./model";

const list = "~/electronic-devices/types";

export default class extends Controller {
    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        // A field's own message goes once its value changes.
        this.addTrigger("name-edited", [m.type.draft.name], () => this.store.delete(m.type.errors.name));
        this.addTrigger("description-edited", [m.type.draft.description], () =>
            this.store.delete(m.type.errors.description),
        );
        this.addTrigger("tags-edited", [m.type.draft.tags], () => this.store.delete(m.type.errors.tagIds));

        // `new` and an id match one route: the address, not the mount, says which record and mode.
        this.addTrigger("address", [$app.url], () => this.open(), true);

        this.store.set(m.type.tagOptions, []);
        getTypeOptions()
            .then((o) => this.store.set(m.type.tagOptions, o.tags))
            .catch(() => {});
    }

    /** The record the address names, in the mode it names. */
    private open() {
        const routed = this.store.get(m.$route.id);
        const id = routed === "new" ? null : routed;
        const viewing = !!id && !this.store.get($app.url).endsWith("/edit");

        this.store.set(m.type.id, id);
        this.store.set(m.type.viewing, viewing);
        this.store.set(m.type.loading, !!id);
        this.store.set(m.type.saving, false);
        this.store.set(m.type.deviceCount, 0);
        this.store.delete(m.type.error);
        this.store.set(m.type.errors, {});
        this.store.set(m.type.visited, false);
        this.store.set(m.type.valid, true);
        this.load({ holdsLicences: false, tags: [] }, id ? "" : "New type");

        // Only an editor can hold unsaved changes; the read-only view has nothing to lose.
        this.release?.();
        this.release = viewing ? undefined : guardLeaving(() => this.dirty());

        if (id)
            getType(id)
                .then((type) => {
                    if (this.store.get(m.type.id) !== id) return;
                    this.store.set(m.type.deviceCount, type.deviceCount);
                    this.load(
                        {
                            name: type.name,
                            holdsLicences: type.holdsLicences,
                            description: type.description,
                            tags: type.tags.map((t) => ({ id: t.id, text: t.name })),
                        },
                        type.name,
                    );
                })
                .catch((error) =>
                    this.store.set(
                        m.type.error,
                        error instanceof ApiError && error.status === 404
                            ? "This type no longer exists."
                            : "The type could not be loaded.",
                    ),
                )
                .finally(() => this.store.set(m.type.loading, false));
    }

    onDestroy() {
        this.release?.();
    }

    private load(draft: TypeDraft, title: string) {
        // Text keys stay absent rather than empty: '' is a value, and `required` would pass on it.
        const clean: TypeDraft = { holdsLicences: draft.holdsLicences, tags: draft.tags };
        if (draft.name) clean.name = draft.name;
        if (draft.description) clean.description = draft.description;

        this.store.set(m.type.draft, clean);
        this.store.set(m.type.title, title);
        this.saved = JSON.stringify(toForm(clean));
    }

    dirty() {
        return JSON.stringify(toForm(this.store.get(m.type.draft))) !== this.saved;
    }

    async save() {
        if (!this.store.get(m.type.valid)) return this.store.set(m.type.visited, true);

        const id = this.store.get(m.type.id);
        this.store.set(m.type.saving, true);
        this.store.delete(m.type.error);

        try {
            const form = toForm(this.store.get(m.type.draft));
            const saved = await (id ? updateType(id, form) : createType(form));
            // A new type returns to the list it was started from; an edit to the view it came from.
            this.leave(id ? `${list}/${saved.id}` : listReturn(list));
        } catch (error) {
            if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(m.type.errors, fieldErrors<TypeEditorState["errors"]>(error));
            else
                this.store.set(
                    m.type.error,
                    error instanceof ApiError && error.status === 404
                        ? "This type was deleted while you were editing it."
                        : "The type could not be saved.",
                );
        } finally {
            this.store.set(m.type.saving, false);
        }
    }

    async remove() {
        const id = this.store.get(m.type.id);
        if (!id) return;

        // A type in use cannot go; say so rather than ask and then refuse.
        const devices = this.store.get(m.type.deviceCount);
        if (devices > 0) {
            await confirm({
                title: "This type is in use",
                message: `${devicesText(devices)} Give them another type before deleting it.`,
                cancelText: "Close",
            });
            return;
        }

        const tags = this.store.get(m.type.draft.tags).length;
        const confirmed = await confirm({
            title: "Delete this type?",
            message:
                tags === 0
                    ? "This cannot be undone."
                    : `${tags === 1 ? "Its tag goes" : `Its ${tags} tags go`} with it; the tags themselves stay. This cannot be undone.`,
            confirmText: "Delete type",
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await deleteType(id);
            this.leave(listReturn(list));
        } catch (error) {
            this.store.set(
                m.type.error,
                error instanceof ApiError && error.status === 409
                    ? error.message
                    : "The type could not be deleted.",
            );
        }
    }

    /** Saved or deleted: nothing left to lose, so the guard goes before the navigation. */
    private leave(to: string) {
        this.release?.();
        this.release = undefined;
        History.pushState({}, null, to);
    }
}
