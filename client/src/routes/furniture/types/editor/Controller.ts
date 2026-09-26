import { Controller, History } from "cx/ui";

import {
    createFurnitureType,
    deleteFurnitureType,
    type FurnitureTypeDetail,
    getFurnitureType,
    updateFurnitureType,
} from "../../../../api/furnitureTypes";
import { ApiError, fieldErrors } from "../../../../api/http";
import { confirm } from "../../../../components/confirm";
import { guardLeaving } from "../../../../leaveGuard";
import { listReturn } from "../../../../listAddress";
import $app from "../../../../model";
import { piecesOfFurniture } from "../../../../furniture";
import m, { type TypeDraft, type TypeEditorState, toForm } from "./model";

const list = "~/furniture/types";
const t = m.type;

export default class extends Controller {
    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        // A field's own message goes once its value changes.
        this.addTrigger("name-edited", [t.draft.name], () => this.store.delete(t.errors.name));
        this.addTrigger("description-edited", [t.draft.description], () =>
            this.store.delete(t.errors.description),
        );

        this.addTrigger("address", [$app.url], () => this.open(), true);
    }

    onDestroy() {
        this.release?.();
    }

    /** The record the address names, in the mode it names. */
    private open() {
        const routed = this.store.get(m.$route.id);
        const id = routed === "new" ? null : routed;
        const viewing = !!id && !this.store.get($app.url).endsWith("/edit");

        this.store.set(t.id, id);
        this.store.set(t.viewing, viewing);
        this.store.set(t.loading, !!id);
        this.store.set(t.saving, false);
        this.store.delete(t.error);
        this.store.set(t.errors, {});
        this.store.set(t.visited, false);
        this.store.set(t.valid, true);
        this.show(null, {}, id ? "" : "New type");

        this.release?.();
        this.release = viewing ? undefined : guardLeaving(() => this.dirty());

        if (id)
            getFurnitureType(id)
                .then((type) => {
                    if (this.store.get(t.id) !== id) return;
                    this.show(type, { name: type.name, description: type.description }, type.name);
                })
                .catch((error) =>
                    this.store.set(
                        t.error,
                        error instanceof ApiError && error.status === 404
                            ? "This type no longer exists."
                            : "The type could not be loaded.",
                    ),
                )
                .finally(() => this.store.set(t.loading, false));
    }

    private show(type: FurnitureTypeDetail | null, draft: TypeDraft, title: string) {
        // Text keys stay absent rather than empty: '' is a value, and `required` would pass on it.
        const clean: TypeDraft = {};
        if (draft.name) clean.name = draft.name;
        if (draft.description) clean.description = draft.description;

        const count = type?.furnitureCount ?? 0;
        this.store.set(t.draft, clean);
        this.store.set(t.title, title);
        this.store.set(t.furnitureCount, count);
        this.store.set(t.furnitureText, count > 0 ? `${count} ${piecesOfFurniture(count)}` : undefined);
        this.store.set(t.furnitureHref, type ? `~/furniture?typeId=${type.id}` : undefined);
        this.saved = JSON.stringify(toForm(clean));
    }

    dirty() {
        return JSON.stringify(toForm(this.store.get(t.draft))) !== this.saved;
    }

    async save() {
        if (!this.store.get(t.valid)) return this.store.set(t.visited, true);

        const id = this.store.get(t.id);
        this.store.set(t.saving, true);
        this.store.delete(t.error);

        try {
            const form = toForm(this.store.get(t.draft));
            const saved = await (id ? updateFurnitureType(id, form) : createFurnitureType(form));
            this.leave(id ? `${list}/${saved.id}` : listReturn(list));
        } catch (error) {
            if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(t.errors, fieldErrors<TypeEditorState["errors"]>(error));
            else
                this.store.set(
                    t.error,
                    error instanceof ApiError && error.status === 404
                        ? "This type was deleted while you were editing it."
                        : "The type could not be saved.",
                );
        } finally {
            this.store.set(t.saving, false);
        }
    }

    async remove() {
        const id = this.store.get(t.id);
        if (!id) return;

        // Furniture of the type keeps it; say so rather than ask and then refuse.
        const text = this.store.get(t.furnitureText);
        if (text) {
            await confirm({
                title: "This type is in use",
                message: `${text} ${this.store.get(t.furnitureCount) === 1 ? "is" : "are"} of it. Give ${this.store.get(t.furnitureCount) === 1 ? "it" : "them"} another type first, or keep this one.`,
                cancelText: "Close",
            });
            return;
        }

        const confirmed = await confirm({
            title: "Delete this type?",
            message: "No furniture is of it. This cannot be undone.",
            confirmText: "Delete type",
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await deleteFurnitureType(id);
            this.leave(listReturn(list));
        } catch (error) {
            this.store.set(
                t.error,
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
