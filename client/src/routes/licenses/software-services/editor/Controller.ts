import { Controller, History } from "cx/ui";

import { ApiError, fieldErrors } from "../../../../api/http";
import {
    createSoftwareService,
    deleteSoftwareService,
    getSoftwareService,
    getSoftwareServiceOptions,
    updateSoftwareService,
} from "../../../../api/softwareServices";
import { confirm } from "../../../../components/confirm";
import { guardLeaving } from "../../../../leaveGuard";
import $app from "../../../../model";
import m, { type Draft, type EditorState, toForm, volumesText } from "./model";

const list = "~/licenses/software-services";
const e = m.entry;

export default class extends Controller {
    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        // A field's own message goes once its value changes.
        this.addTrigger("name-edited", [e.draft.name], () => this.store.delete(e.errors.name));
        this.addTrigger("category-edited", [e.draft.categoryId], () =>
            this.store.delete(e.errors.categoryId),
        );
        this.addTrigger("manufacturer-edited", [e.draft.manufacturerId], () =>
            this.store.delete(e.errors.manufacturerId),
        );
        this.addTrigger("url-edited", [e.draft.url], () => this.store.delete(e.errors.url));

        // `new` and an id match one route: the address, not the mount, says which record and mode.
        this.addTrigger("address", [$app.url], () => this.open(), true);

        this.store.set(e.categories, []);
        this.store.set(e.manufacturers, []);
        getSoftwareServiceOptions()
            .then((o) => {
                this.store.set(e.categories, o.categories);
                this.store.set(e.manufacturers, o.manufacturers);
            })
            .catch(() => {});
    }

    private open() {
        const routed = this.store.get(m.$route.id);
        const id = routed === "new" ? null : routed;
        const viewing = !!id && !this.store.get($app.url).endsWith("/edit");

        this.store.set(e.id, id);
        this.store.set(e.viewing, viewing);
        this.store.set(e.loading, !!id);
        this.store.set(e.saving, false);
        this.store.set(e.volumeCount, 0);
        this.store.delete(e.error);
        this.store.set(e.errors, {});
        this.store.set(e.visited, false);
        this.store.set(e.valid, true);
        this.load({}, id ? "" : "New software or service");

        this.release?.();
        this.release = viewing ? undefined : guardLeaving(() => this.dirty());

        if (id)
            getSoftwareService(id)
                .then((entry) => {
                    if (this.store.get(e.id) !== id) return;
                    this.store.set(e.volumeCount, entry.volumeCount);
                    this.load(
                        {
                            name: entry.name,
                            categoryId: entry.category.id,
                            categoryText: entry.category.name,
                            manufacturerId: entry.manufacturer.id,
                            manufacturerText: entry.manufacturer.name,
                            url: entry.url,
                        },
                        entry.name,
                    );
                })
                .catch((error) =>
                    this.store.set(
                        e.error,
                        error instanceof ApiError && error.status === 404
                            ? "This entry no longer exists."
                            : "The entry could not be loaded.",
                    ),
                )
                .finally(() => this.store.set(e.loading, false));
    }

    onDestroy() {
        this.release?.();
    }

    private load(draft: Draft, title: string) {
        // Text keys stay absent rather than empty: '' is a value, and `required` would pass on it.
        const clean: Draft = {};
        for (const [key, value] of Object.entries(draft)) if (value) (clean as any)[key] = value;

        this.store.set(e.draft, clean);
        this.store.set(e.title, title);
        this.saved = JSON.stringify(toForm(clean));
    }

    dirty() {
        return JSON.stringify(toForm(this.store.get(e.draft))) !== this.saved;
    }

    async save() {
        if (!this.store.get(e.valid)) return this.store.set(e.visited, true);

        const id = this.store.get(e.id);
        this.store.set(e.saving, true);
        this.store.delete(e.error);

        try {
            const form = toForm(this.store.get(e.draft));
            const saved = await (id ? updateSoftwareService(id, form) : createSoftwareService(form));
            // A new entry returns to the list it was started from; an edit to the view it came from.
            this.leave(id ? `${list}/${saved.id}` : list);
        } catch (error) {
            if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(e.errors, fieldErrors<EditorState["errors"]>(error));
            else
                this.store.set(
                    e.error,
                    error instanceof ApiError && error.status === 404
                        ? "This entry was deleted while you were editing it."
                        : "The entry could not be saved.",
                );
        } finally {
            this.store.set(e.saving, false);
        }
    }

    async remove() {
        const id = this.store.get(e.id);
        if (!id) return;

        // An entry a volume is of cannot go; say so rather than ask and then refuse.
        const volumes = this.store.get(e.volumeCount);
        if (volumes > 0) {
            await confirm({
                title: "This entry is in use",
                message: `${volumesText(volumes)} Remove those volumes from their licences before deleting it.`,
                cancelText: "Close",
            });
            return;
        }

        const confirmed = await confirm({
            title: "Delete this entry?",
            message: "This cannot be undone.",
            confirmText: "Delete",
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await deleteSoftwareService(id);
            this.leave(list);
        } catch (error) {
            this.store.set(
                e.error,
                error instanceof ApiError && error.status === 409
                    ? error.message
                    : "The entry could not be deleted.",
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
