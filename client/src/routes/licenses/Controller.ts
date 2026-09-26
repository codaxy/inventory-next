import { Controller } from "cx/ui";

import { ApiError } from "../../api/http";
import { getLicenseOptions, listLicenses } from "../../api/licenses";
import { encodeDate } from "../../dates";
import { pager, pageSize } from "../../paging";
import m, { type FilterKey, type Filters, toChips, toRows } from "./model";

const searchDelay = 300;
const s = m.list;

/** The day after, as `YYYY-MM-DD`: the server's `to` is exclusive, the reader's "to" includes the day. */
function dayAfter(day: string) {
    const [y, mo, d] = day.split("-").map(Number);
    return encodeDate(new Date(y, mo - 1, d + 1));
}

export default class extends Controller {
    private search: string | null = null;
    private timer?: ReturnType<typeof setTimeout>;
    private request = 0;

    onInit() {
        this.store.delete(s.search);
        this.store.set(s.filters, {});
        this.store.set(s.filtersOpen, false);
        this.store.set(s.filtersValid, true);
        this.store.set(s.chips, []);
        this.store.set(s.vendors, []);
        this.store.set(s.sort, "-modified");
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

        this.addTrigger("filters", [s.filters], (filters) => {
            this.store.set(s.chips, toChips(filters ?? {}));
            this.goTo(1);
        });

        getLicenseOptions()
            .then((o) => this.store.set(s.vendors, o.vendors))
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
        const f = this.store.get(s.filters) ?? {};
        this.store.set(s.loading, true);

        try {
            const result = await listLicenses({
                q: this.search?.trim() || undefined,
                vendorId: f.vendorId ?? undefined,
                purchasedFrom: f.from ?? undefined,
                purchasedTo: f.to ? dayAfter(f.to) : undefined,
                expiry: f.expiry ?? undefined,
                incomplete: f.incomplete ?? undefined,
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
                result.total === 0
                    ? "No licences"
                    : `${state.summary} ${result.total === 1 ? "licence" : "licences"}`,
            );
            this.store.delete(s.error);
            this.store.set(s.loaded, true);
        } catch (error) {
            if (request !== this.request) return;
            this.store.set(
                s.error,
                error instanceof ApiError ? error.message : "The licences could not be loaded.",
            );
        } finally {
            if (request === this.request) this.store.set(s.loading, false);
        }
    }

    /** A column header: dates and numbers newest or largest first, text A to Z; then the other way. */
    sortBy(key: "number" | "name" | "vendor" | "value" | "purchased" | "expires" | "modified") {
        const first = key === "name" || key === "vendor" ? key : `-${key}`;
        this.store.update(
            s.sort,
            (sort) => (sort === first ? (first.startsWith("-") ? key : `-${key}`) : first) as typeof sort,
        );
        this.goTo(1);
    }

    toggleFilters() {
        this.store.toggle(s.filtersOpen);
    }

    closeFilters() {
        this.store.set(s.filtersOpen, false);
    }

    setExpiry(expiry: Filters["expiry"]) {
        this.store.set(s.filters.expiry, expiry);
    }

    setIncomplete(incomplete: Filters["incomplete"]) {
        this.store.set(s.filters.incomplete, incomplete);
    }

    removeFilter(key: FilterKey) {
        this.store.update(s.filters, (f) => {
            if (key === "vendor") return { ...f, vendorId: undefined, vendorText: undefined };
            if (key === "range") return { ...f, from: undefined, to: undefined };
            return { ...f, [key]: undefined };
        });
    }

    clearFilters() {
        this.store.set(s.filters, {});
    }

    clearAll() {
        this.store.delete(s.search);
        this.store.set(s.filters, {});
    }
}
