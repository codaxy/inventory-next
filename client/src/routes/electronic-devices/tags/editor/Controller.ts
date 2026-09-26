import { Controller, History } from "cx/ui";
import { MsgBox } from "cx/widgets";

import { createTag, deleteTag, getTag, getTagOptions, updateTag } from "../../../../api/electronicDeviceTags";
import { ApiError, fieldErrors } from "../../../../api/http";
import { guardLeaving } from "../../../../leaveGuard";
import m, { type TagDraft, type TagEditorState, toForm } from "./model";

const list = "~/electronic-devices/tags";

export default class extends Controller {
    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        const routed = this.store.get(m.$route.id);
        const id = routed === "new" ? null : routed;

        this.store.set(m.tag.id, id);
        this.store.set(m.tag.loading, !!id);
        this.store.set(m.tag.saving, false);
        this.store.delete(m.tag.error);
        this.store.set(m.tag.errors, {});
        this.store.set(m.tag.visited, false);
        this.store.set(m.tag.valid, true);
        this.store.set(m.tag.typeOptions, []);
        this.load(id, { types: [] }, id ? "" : "New tag");

        // A field's own message goes once its value changes.
        this.addTrigger("name-edited", [m.tag.draft.name], () => this.store.delete(m.tag.errors.name));
        this.addTrigger("description-edited", [m.tag.draft.description], () =>
            this.store.delete(m.tag.errors.description),
        );
        this.addTrigger("types-edited", [m.tag.draft.types], () => this.store.delete(m.tag.errors.typeIds));

        this.release = guardLeaving(() => this.dirty());

        getTagOptions()
            .then((o) => this.store.set(m.tag.typeOptions, o.types))
            .catch(() => {});

        if (id)
            getTag(id)
                .then((tag) =>
                    this.load(
                        id,
                        {
                            name: tag.name,
                            description: tag.description,
                            types: tag.types.map((t) => ({ id: t.id, text: t.name })),
                        },
                        tag.name,
                    ),
                )
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
            await (id ? updateTag(id, form) : createTag(form));
            this.leave();
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
        const answer = await MsgBox.yesNo({
            title: "Delete this tag?",
            message:
                types === 0
                    ? "The tag is not on any type."
                    : `${types === 1 ? "1 type loses" : `${types} types lose`} it. This cannot be undone.`,
        });
        if (answer !== "yes") return;

        try {
            await deleteTag(id);
            this.leave();
        } catch {
            this.store.set(m.tag.error, "The tag could not be deleted.");
        }
    }

    cancel() {
        History.pushState({}, null, list);
    }

    /** Saved or deleted: nothing left to lose, so the guard goes before the navigation. */
    private leave() {
        this.release?.();
        this.release = undefined;
        History.pushState({}, null, list);
    }
}
