import { Controller } from "cx/ui";

import { getTypeOptions, listTypes } from "../../../api/electronicDeviceTypes";
import { ApiError } from "../../../api/http";
import { pager, pageSize } from "../../../paging";
import m, { type FilterKey, toChips, toRows } from "./model";

const searchDelay = 300;

export default class extends Controller {
    private search: string | null = null;
    private timer?: ReturnType<typeof setTimeout>;
    private request = 0;

    onInit() {
        this.store.delete(m.types.search);
        this.store.set(m.types.filters, {});
        this.store.set(m.types.filtersOpen, false);
        this.store.set(m.types.chips, []);
        this.store.set(m.types.tagOptions, []);
        this.store.set(m.types.sort, "name");
        this.store.set(m.types.page, 1);
        this.store.set(m.types.rows, []);
        this.store.set(m.types.total, 0);
        this.store.set(m.types.loading, false);
        this.store.set(m.types.loaded, false);
        this.store.delete(m.types.error);
        this.store.set(m.types.pager, pager(1, pageSize, 0));
        this.store.set(m.types.totalText, "");
        this.search = null;

        this.addTrigger("search", [m.types.search], () => {
            clearTimeout(this.timer);
            this.timer = setTimeout(() => {
                this.search = this.store.get(m.types.search) ?? null;
                this.goTo(1);
            }, searchDelay);
        });

        // Filters apply as they change; the chips say what is filtering while the pane is shut.
        this.addTrigger("filters", [m.types.filters], (filters) => {
            this.store.set(m.types.chips, toChips(filters ?? {}));
            this.goTo(1);
        });

        getTypeOptions()
            .then((o) => this.store.set(m.types.tagOptions, o.tags))
            .catch(() => {});

        this.load();
    }

    onDestroy() {
        clearTimeout(this.timer);
    }

    goTo(page: number, scroll = false) {
        this.store.set(m.types.page, page);
        this.load();
        if (scroll) window.scrollTo({ top: 0 });
    }

    async load() {
        const request = ++this.request;
        const page = this.store.get(m.types.page);
        const filters = this.store.get(m.types.filters) ?? {};
        this.store.set(m.types.loading, true);

        try {
            const result = await listTypes({
                q: this.search?.trim() || undefined,
                tagIds: filters.tags?.map((t) => t.id),
                holdsLicences: filters.holdsLicences ?? undefined,
                sort: this.store.get(m.types.sort),
                page,
                pageSize,
            });
            if (request !== this.request) return;

            const state = pager(page, pageSize, result.total);
            if (result.items.length === 0 && result.total > 0) return this.goTo(state.pageCount);

            this.store.set(m.types.rows, toRows(result.items));
            this.store.set(m.types.total, result.total);
            this.store.set(m.types.pager, state);
            this.store.set(
                m.types.totalText,
                result.total === 0 ? "No types" : `${state.summary} ${result.total === 1 ? "type" : "types"}`,
            );
            this.store.delete(m.types.error);
            this.store.set(m.types.loaded, true);
        } catch (error) {
            if (request !== this.request) return;
            this.store.set(
                m.types.error,
                error instanceof ApiError ? error.message : "The types could not be loaded.",
            );
        } finally {
            if (request === this.request) this.store.set(m.types.loading, false);
        }
    }

    /** A column header: its key ascending, then descending, then back. */
    sortBy(key: "name" | "tags" | "devices") {
        this.store.update(m.types.sort, (sort) => (sort === key ? `-${key}` : key) as typeof sort);
        this.goTo(1);
    }

    toggleFilters() {
        this.store.toggle(m.types.filtersOpen);
    }

    closeFilters() {
        this.store.set(m.types.filtersOpen, false);
    }

    setHoldsLicences(value: boolean | null) {
        this.store.set(m.types.filters.holdsLicences, value);
    }

    removeFilter(key: FilterKey) {
        if (key === "holdsLicences") return this.store.set(m.types.filters.holdsLicences, null);
        const id = key.slice("tag:".length);
        this.store.update(m.types.filters.tags, (tags) => (tags ?? []).filter((t) => t.id !== id));
    }

    clearFilters() {
        this.store.set(m.types.filters, {});
    }

    /** Search and filters both: the empty state's way out. */
    clearAll() {
        this.store.delete(m.types.search);
        this.store.set(m.types.filters, {});
    }
}
