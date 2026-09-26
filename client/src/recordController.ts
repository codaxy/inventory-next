import type { AccessorChain } from "cx/data";
import { Controller, History } from "cx/ui";

import { ApiError, fieldErrors } from "./api/http";
import { confirm } from "./components/confirm";
import type { HoldingSection } from "./holdings";
import { guardLeaving } from "./leaveGuard";
import { listReturn } from "./listAddress";
import $app from "./model";

/** What a record page's markup binds; `RecordController` keeps it. */
export interface RecordState<D> {
    /** `null` while creating. */
    id: string | null;
    /** Read-only, as a row opens it; editing is `…/:id/edit`, creating opens in it. */
    viewing: boolean;
    title: string;
    draft: D;
    /** What is attached to the record, read-only beneath its fields. */
    sections: HoldingSection[];
    /** The kinds with nothing, as one line. */
    none?: string;
    /** What keeps the record from being deleted, as a phrase; absent when nothing does. */
    holds?: string;
    loaded: boolean;
    loading: boolean;
    saving: boolean;
    error?: string;
    errors: Record<string, string | undefined>;
    valid: boolean;
    visited: boolean;
}

export interface Attached {
    sections: HoldingSection[];
    none?: string;
    holds?: string;
}

/**
 * A record's page: the address says which record and which mode — `:id`, `:id/edit`, `new` — so it
 * reopens on every change of it; unsaved edits ask before leaving; a save returns to the view, a new
 * record's to the list; a record something is attached to says so instead of offering a delete.
 */
export abstract class RecordController<D extends object, Detail, Form> extends Controller {
    protected abstract readonly r: AccessorChain<RecordState<D>>;
    protected abstract readonly routeId: AccessorChain<string>;
    /** The list, `~/company/vendors`; records are beneath it. */
    protected abstract readonly path: string;
    /** "vendor": in the messages. */
    protected abstract readonly noun: string;
    protected abstract readonly newTitle: string;

    protected abstract load(id: string): Promise<Detail>;
    protected abstract create(form: Form): Promise<{ id: string }>;
    protected abstract update(id: string, form: Form): Promise<{ id: string }>;
    protected abstract destroy(id: string): Promise<void>;
    protected abstract toDraft(detail: Detail): D;
    protected abstract toForm(draft: D): Form;
    protected abstract titleOf(detail: Detail): string;

    /** What is attached, for the page and for a delete. */
    protected attached(_detail: Detail): Attached {
        return { sections: [] };
    }

    /** Why a delete is not offered, given what the record holds. */
    protected refusal(holds: string) {
        return `It has ${holds}. Move ${holds.includes(" and ") || !holds.startsWith("1 ") ? "them" : "it"} elsewhere first, or keep the ${this.noun}.`;
    }

    /** The server's field names that the form binds under another: a picker's `<key>Id`. */
    protected readonly fieldAliases: Record<string, string> = {};

    protected emptyDraft(): D {
        return {} as D;
    }

    /** The draft after the record loads: the options it depends on, say. */
    protected loaded(_detail: Detail) {}

    /** The form as loaded; what "unsaved" is measured against. */
    private saved = "";
    private release?: () => void;

    onInit() {
        // The server's messages go once the reader changes the form.
        this.addTrigger("draft-edited", [this.r.draft], () => this.store.set(this.r.errors, {}));
        this.addTrigger("address", [$app.url], () => this.open(), true);
    }

    onDestroy() {
        this.release?.();
    }

    private open() {
        const r = this.r;
        const routed = this.store.get(this.routeId);
        const id = routed === "new" ? null : routed;
        const viewing = !!id && !this.store.get($app.url).endsWith("/edit");

        this.store.set(r.id, id);
        this.store.set(r.viewing, viewing);
        this.store.set(r.loading, !!id);
        this.store.set(r.loaded, false);
        this.store.set(r.saving, false);
        this.store.delete(r.error);
        this.store.set(r.errors, {});
        this.store.set(r.visited, false);
        this.store.set(r.valid, true);
        this.store.set(r.sections, []);
        this.store.delete(r.none);
        this.store.delete(r.holds);
        this.fill(this.emptyDraft(), id ? "" : this.newTitle);

        this.release?.();
        this.release = viewing ? undefined : guardLeaving(() => this.dirty());

        if (id)
            this.load(id)
                .then((detail) => {
                    if (this.store.get(r.id) !== id) return;
                    this.fill(this.toDraft(detail), this.titleOf(detail));
                    const attached = this.attached(detail);
                    this.store.set(r.sections, attached.sections);
                    this.store.set(r.none, attached.none);
                    this.store.set(r.holds, attached.holds);
                    this.store.set(r.loaded, true);
                    this.loaded(detail);
                })
                .catch((error) =>
                    this.store.set(
                        r.error,
                        error instanceof ApiError && error.status === 404
                            ? `This ${this.noun} no longer exists.`
                            : `The ${this.noun} could not be loaded.`,
                    ),
                )
                .finally(() => this.store.set(r.loading, false));
    }

    /** Text keys stay absent rather than empty: '' is a value, and `required` would pass on it. */
    private fill(draft: D, title: string) {
        const clean = Object.fromEntries(
            Object.entries(draft).filter(([, v]) => v !== null && v !== "" && v !== undefined),
        ) as D;
        this.store.set(this.r.draft, clean);
        this.store.set(this.r.title, title);
        this.saved = JSON.stringify(this.toForm(clean));
    }

    /** After the draft is filled from code: where the form starts, not an edit to guard. */
    protected rebase() {
        this.saved = JSON.stringify(this.toForm(this.store.get(this.r.draft)));
    }

    dirty() {
        return JSON.stringify(this.toForm(this.store.get(this.r.draft))) !== this.saved;
    }

    async save() {
        const r = this.r;
        if (!this.store.get(r.valid)) return this.store.set(r.visited, true);

        const id = this.store.get(r.id);
        this.store.set(r.saving, true);
        this.store.delete(r.error);

        try {
            const form = this.toForm(this.store.get(r.draft));
            const saved = await (id ? this.update(id, form) : this.create(form));
            this.leave(id ? `${this.path}/${saved.id}` : listReturn(this.path));
        } catch (error) {
            if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(
                    r.errors,
                    Object.fromEntries(
                        Object.entries(fieldErrors<Record<string, string>>(error)).map(([k, v]) => [
                            this.fieldAliases[k] ?? k,
                            v,
                        ]),
                    ),
                );
            else
                this.store.set(
                    r.error,
                    error instanceof ApiError && error.status === 404
                        ? `This ${this.noun} was deleted while you were editing it.`
                        : `The ${this.noun} could not be saved.`,
                );
        } finally {
            this.store.set(r.saving, false);
        }
    }

    async remove() {
        const r = this.r;
        const id = this.store.get(r.id);
        if (!id || !this.store.get(r.loaded)) return;

        // What is attached keeps the record — and some of it would go with it, the database
        // cascading: say so rather than ask and then refuse.
        const holds = this.store.get(r.holds);
        if (holds) {
            await confirm({
                title: `This ${this.noun} is in use`,
                message: this.refusal(holds),
                cancelText: "Close",
            });
            return;
        }

        const confirmed = await confirm({
            title: `Delete this ${this.noun}?`,
            message: "Nothing is attached to it. This cannot be undone.",
            confirmText: `Delete ${this.noun}`,
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await this.destroy(id);
            this.leave(listReturn(this.path));
        } catch (error) {
            this.store.set(
                r.error,
                error instanceof ApiError && error.status === 409
                    ? error.message
                    : `The ${this.noun} could not be deleted.`,
            );
        }
    }

    private leave(to: string) {
        this.release?.();
        this.release = undefined;
        History.pushState({}, null, to);
    }
}
