import { Controller, History } from "cx/ui";

import { ApiError, fieldErrors } from "../../../api/http";
import {
    createLicense,
    deleteLicense,
    getLicense,
    getLicenseOptions,
    type LicenseDetail,
    updateLicense,
} from "../../../api/licenses";
import { importanceFor } from "../../../assets";
import { confirm } from "../../../components/confirm";
import { guardLeaving } from "../../../leaveGuard";
import { listReturn, queryOf } from "../../../listAddress";
import $app from "../../../model";
import m, { emptyOptions, expiryLine, rowKey, toDraft, toForm, type VolumeRow } from "./model";

const list = "~/licenses";
const l = m.license;

export default class extends Controller {
    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        this.store.set(l.options, emptyOptions);

        // The server's messages go once the reader changes the form; the importance follows the weights.
        this.addTrigger("draft-edited", [l.draft], () => this.store.set(l.errors, {}));
        this.addTrigger("weights", [l.draft, l.options], (draft, options) =>
            this.store.set(l.importance, draft ? importanceFor(draft, options ?? emptyOptions) : "—"),
        );

        this.addTrigger("address", [$app.url], () => this.open(), true);

        getLicenseOptions()
            .then((o) => this.store.set(l.options, o))
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

        this.store.set(l.id, id);
        this.store.set(l.viewing, viewing);
        this.store.set(l.loading, !!(id || from));
        this.store.set(l.saving, false);
        this.store.set(l.stale, false);
        this.store.delete(l.error);
        this.store.set(l.errors, {});
        this.store.set(l.visited, false);
        this.store.set(l.valid, true);
        this.store.delete(l.number);
        this.store.delete(l.lastModified);
        this.store.delete(l.expiry);
        this.store.delete(l.expiryText);
        this.load({ incomplete: false, autoRenew: false, volumes: [] }, id ? "" : "New licence");

        this.release?.();
        this.release = viewing ? undefined : guardLeaving(() => this.dirty());

        const source = id ?? from;
        if (source)
            getLicense(source)
                .then((license) => {
                    if (this.store.get(l.id) !== id) return;
                    this.show(license, !id);
                })
                .catch((error) =>
                    this.store.set(
                        l.error,
                        error instanceof ApiError && error.status === 404
                            ? "This licence no longer exists."
                            : "The licence could not be loaded.",
                    ),
                )
                .finally(() => this.store.set(l.loading, false));
    }

    /** A loaded licence into the form; as a duplicate, everything but the number and the volumes. */
    private show(license: LicenseDetail, duplicate: boolean) {
        this.load(toDraft(license, duplicate), duplicate ? "New licence" : license.name);
        if (duplicate) return;

        this.store.set(l.number, license.number ? `#${license.number}` : undefined);
        this.store.set(l.lastModified, license.lastModified);
        this.store.set(l.expiry, license.expiry ?? undefined);
        this.store.set(l.expiryText, expiryLine(license));
    }

    private load(draft: Parameters<typeof toForm>[0], title: string) {
        this.store.set(l.draft, draft);
        this.store.set(l.title, title);
        this.saved = JSON.stringify(toForm(draft));
    }

    dirty() {
        return JSON.stringify(toForm(this.store.get(l.draft))) !== this.saved;
    }

    addVolume() {
        this.store.update(l.draft.volumes, (volumes) => [
            ...(volumes ?? []),
            { key: rowKey(), quantity: 1 } satisfies VolumeRow,
        ]);
    }

    /**
     * A volume added in this edit goes at once — nothing is lost. An existing one is struck through
     * and goes only when the licence is saved, so the reader sees what the save will remove and can
     * take it back.
     */
    removeVolume(key: string) {
        this.store.update(l.draft.volumes, (volumes) =>
            (volumes ?? []).flatMap((v) => (v.key !== key ? [v] : v.id ? [{ ...v, removed: true }] : [])),
        );
    }

    keepVolume(key: string) {
        this.store.update(l.draft.volumes, (volumes) =>
            (volumes ?? []).map((v) => (v.key === key ? { ...v, removed: false } : v)),
        );
    }

    async save() {
        if (!this.store.get(l.valid)) return this.store.set(l.visited, true);

        const id = this.store.get(l.id);
        this.store.set(l.saving, true);
        this.store.delete(l.error);
        this.store.set(l.stale, false);

        try {
            const form = toForm(this.store.get(l.draft), this.store.get(l.lastModified));
            const saved = await (id ? updateLicense(id, form) : createLicense(form));
            this.leave(id ? `${list}/${saved.id}` : listReturn(list));
        } catch (error) {
            if (error instanceof ApiError && error.status === 409) {
                this.store.set(
                    l.error,
                    "Someone saved this licence since you opened it. Reload to see their changes; yours are not saved.",
                );
                this.store.set(l.stale, true);
            } else if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(l.errors, fieldErrors<Record<string, string>>(error));
            else
                this.store.set(
                    l.error,
                    error instanceof ApiError && error.status === 404
                        ? "This licence was deleted while you were editing it."
                        : "The licence could not be saved.",
                );
        } finally {
            this.store.set(l.saving, false);
        }
    }

    /** After a refused save: the licence as it is now, the reader's edits discarded. */
    reload() {
        this.release?.();
        this.release = undefined;
        this.open();
    }

    async remove() {
        const id = this.store.get(l.id);
        if (!id) return;

        // Something on a volume keeps the licence; say so rather than ask and then refuse.
        const held = this.store.get(l.draft.volumes).find((v) => v.held);
        if (held) {
            await confirm({
                title: "This licence is in use",
                message: `${held.held}. Deactivate and delete those first, or keep the licence.`,
                cancelText: "Close",
            });
            return;
        }

        const volumes = this.store.get(l.draft.volumes).length;
        const confirmed = await confirm({
            title: "Delete this licence?",
            message:
                volumes === 0
                    ? "This cannot be undone."
                    : `${volumes === 1 ? "Its volume goes" : `Its ${volumes} volumes go`} with it. This cannot be undone.`,
            confirmText: "Delete licence",
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await deleteLicense(id);
            this.leave(listReturn(list));
        } catch (error) {
            this.store.set(
                l.error,
                error instanceof ApiError && error.status === 409
                    ? error.message
                    : "The licence could not be deleted.",
            );
        }
    }

    private leave(to: string) {
        this.release?.();
        this.release = undefined;
        History.pushState({}, null, to);
    }
}
