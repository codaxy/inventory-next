import { Controller } from "cx/ui";

import { ApiError } from "../../../api/http";
import { getSoftwareServiceOptions, listSoftwareServices } from "../../../api/softwareServices";
import { pager, pageSize } from "../../../paging";
import m, { type FilterKey, toChips, toRows } from "./model";

const searchDelay = 300;
const s = m.list;

export default class extends Controller {
    private search: string | null = null;
    private timer?: ReturnType<typeof setTimeout>;
    private request = 0;

    onInit() {
        this.store.delete(s.search);
        this.store.set(s.filters, {});
        this.store.set(s.filtersOpen, false);
        this.store.set(s.chips, []);
        this.store.set(s.categories, []);
        this.store.set(s.manufacturers, []);
        this.store.set(s.sort, "name");
        this.store.set(s.page, 1);
        this.store.set(s.rows, []);
        this.store.set(s.total, 0);
        this.store.set(s.loading, false);
        this.store.set(s.loaded, false);
        this.store.delete(s.error);
        this.store.set(s.pager, pager(1, pageSize, 0));
        this.store.set(s.totalText, "");
        this.search = null;

        this.addTrigger("search", [s.search], () => {
            clearTimeout(this.timer);
            this.timer = setTimeout(() => {
                this.search = this.store.get(s.search) ?? null;
                this.goTo(1);
            }, searchDelay);
        });

        // Filters apply as they change; the chips say what is filtering while the pane is shut.
        this.addTrigger("filters", [s.filters], (filters) => {
            this.store.set(s.chips, toChips(filters ?? {}));
            this.goTo(1);
        });

        getSoftwareServiceOptions()
            .then((o) => {
                this.store.set(s.categories, o.categories);
                this.store.set(s.manufacturers, o.manufacturers);
            })
            .catch(() => {});

        this.load();
    }

    onDestroy() {
        clearTimeout(this.timer);
    }

    goTo(page: number, scroll = false) {
        this.store.set(s.page, page);
        this.load();
        if (scroll) window.scrollTo({ top: 0 });
    }

    async load() {
        const request = ++this.request;
        const page = this.store.get(s.page);
        const filters = this.store.get(s.filters) ?? {};
        this.store.set(s.loading, true);

        try {
            const result = await listSoftwareServices({
                q: this.search?.trim() || undefined,
                categoryId: filters.categoryId ?? undefined,
                manufacturerId: filters.manufacturerId ?? undefined,
                sort: this.store.get(s.sort),
                page,
                pageSize,
            });
            if (request !== this.request) return;

            const state = pager(page, pageSize, result.total);
            if (result.items.length === 0 && result.total > 0) return this.goTo(state.pageCount);

            this.store.set(s.rows, toRows(result.items));
            this.store.set(s.total, result.total);
            this.store.set(s.pager, state);
            this.store.set(
                s.totalText,
                result.total === 0 ? "None" : `${state.summary} ${result.total === 1 ? "entry" : "entries"}`,
            );
            this.store.delete(s.error);
            this.store.set(s.loaded, true);
        } catch (error) {
            if (request !== this.request) return;
            this.store.set(
                s.error,
                error instanceof ApiError ? error.message : "The list could not be loaded.",
            );
        } finally {
            if (request === this.request) this.store.set(s.loading, false);
        }
    }

    /** A column header: its key ascending, then descending, then back. */
    sortBy(key: "name" | "category" | "manufacturer" | "volumes") {
        this.store.update(s.sort, (sort) => (sort === key ? `-${key}` : key) as typeof sort);
        this.goTo(1);
    }

    toggleFilters() {
        this.store.toggle(s.filtersOpen);
    }

    closeFilters() {
        this.store.set(s.filtersOpen, false);
    }

    removeFilter(key: FilterKey) {
        this.store.update(s.filters, (f) => ({ ...f, [`${key}Id`]: undefined, [`${key}Text`]: undefined }));
    }

    clearFilters() {
        this.store.set(s.filters, {});
    }

    /** Search and filters both: the empty state's way out. */
    clearAll() {
        this.store.delete(s.search);
        this.store.set(s.filters, {});
    }
}
