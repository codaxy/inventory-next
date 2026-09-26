import { Controller, History } from "cx/ui";

import { createTag, deleteTag, getTag, getTagOptions, updateTag } from "../../../../api/electronicDeviceTags";
import { ApiError, fieldErrors } from "../../../../api/http";
import { confirm } from "../../../../components/confirm";
import { guardLeaving } from "../../../../leaveGuard";
import $app from "../../../../model";
import m, { type TagDraft, type TagEditorState, toForm } from "./model";

const list = "~/electronic-devices/tags";

export default class extends Controller {
    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        // A field's own message goes once its value changes.
        this.addTrigger("name-edited", [m.tag.draft.name], () => this.store.delete(m.tag.errors.name));
        this.addTrigger("description-edited", [m.tag.draft.description], () =>
            this.store.delete(m.tag.errors.description),
        );
        this.addTrigger("types-edited", [m.tag.draft.types], () => this.store.delete(m.tag.errors.typeIds));

        // `new` and an id match one route, so saving a new tag keeps this page and this controller:
        // the address, not the mount, says which record and which mode are open.
        this.addTrigger("address", [$app.url], () => this.open(), true);

        this.store.set(m.tag.typeOptions, []);
        getTagOptions()
            .then((o) => this.store.set(m.tag.typeOptions, o.types))
            .catch(() => {});
    }

    /** The record the address names, in the mode it names. */
    private open() {
        const routed = this.store.get(m.$route.id);
        const id = routed === "new" ? null : routed;
        const viewing = !!id && !this.store.get($app.url).endsWith("/edit");

        this.store.set(m.tag.id, id);
        this.store.set(m.tag.viewing, viewing);
        this.store.set(m.tag.loading, !!id);
        this.store.set(m.tag.saving, false);
        this.store.delete(m.tag.error);
        this.store.set(m.tag.errors, {});
        this.store.set(m.tag.visited, false);
        this.store.set(m.tag.valid, true);
        this.load(id, { types: [] }, id ? "" : "New tag");

        // Only an editor can hold unsaved changes; the read-only view has nothing to lose.
        this.release?.();
        this.release = viewing ? undefined : guardLeaving(() => this.dirty());

        if (id)
            getTag(id)
                .then((tag) => {
                    if (this.store.get(m.tag.id) !== id) return;
                    this.load(
                        id,
                        {
                            name: tag.name,
                            description: tag.description,
                            types: tag.types.map((t) => ({ id: t.id, text: t.name })),
                        },
                        tag.name,
                    );
                })
                .catch((error) =>
                    this.store.set(
                        m.tag.error,
                        error instanceof ApiError && error.status === 404
                            ? "This tag no longer exists."
                            : "The tag could not be loaded.",
                    ),
                )
                .finally(() => this.store.set(m.tag.loading, false));
    }

    onDestroy() {
        this.release?.();
    }

    private load(id: string | null, draft: TagDraft, title: string) {
        // Text keys stay absent rather than empty: '' is a value, and `required` would pass on it.
        const clean: TagDraft = { types: draft.types };
        if (draft.name) clean.name = draft.name;
        if (draft.description) clean.description = draft.description;

        this.store.set(m.tag.draft, clean);
        this.store.set(m.tag.title, title);
        this.saved = JSON.stringify(toForm(clean));
    }

    dirty() {
        return JSON.stringify(toForm(this.store.get(m.tag.draft))) !== this.saved;
    }

    async save() {
        if (!this.store.get(m.tag.valid)) return this.store.set(m.tag.visited, true);

        const id = this.store.get(m.tag.id);
        this.store.set(m.tag.saving, true);
        this.store.delete(m.tag.error);

        try {
            const form = toForm(this.store.get(m.tag.draft));
            const saved = await (id ? updateTag(id, form) : createTag(form));
            this.leave(`${list}/${saved.id}`);
        } catch (error) {
            if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(m.tag.errors, fieldErrors<TagEditorState["errors"]>(error));
            else
                this.store.set(
                    m.tag.error,
                    error instanceof ApiError && error.status === 404
                        ? "This tag was deleted while you were editing it."
                        : "The tag could not be saved.",
                );
        } finally {
            this.store.set(m.tag.saving, false);
        }
    }

    async remove() {
        const id = this.store.get(m.tag.id);
        if (!id) return;

        const types = this.store.get(m.tag.draft.types).length;
        const confirmed = await confirm({
            title: "Delete this tag?",
            message:
                types === 0
                    ? "The tag is not on any type."
                    : `${types === 1 ? "1 type loses" : `${types} types lose`} it. This cannot be undone.`,
            confirmText: "Delete tag",
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await deleteTag(id);
            this.leave(list);
        } catch {
            this.store.set(m.tag.error, "The tag could not be deleted.");
        }
    }

    /** Saved or deleted: nothing left to lose, so the guard goes before the navigation. */
    private leave(to: string) {
        this.release?.();
        this.release = undefined;
        History.pushState({}, null, to);
    }
}
