import { Controller } from "cx/ui";

import { getActivationOptions, listActivations } from "../../../api/activations";
import { ApiError } from "../../../api/http";
import { pager, pageSize } from "../../../paging";
import m, { type FilterKey, type Filters, toChips, toRows } from "./model";

const searchDelay = 300;
const s = m.list;

export default class extends Controller {
    private search: string | null = null;
    private timer?: ReturnType<typeof setTimeout>;
    private request = 0;

    onInit() {
        // A licence's volume links here filtered to itself: `?licenseId=…&softwareId=…`.
        const address = new URLSearchParams(window.location.search);
        const initial: Filters = {};
        if (address.get("softwareId")) initial.softwareId = address.get("softwareId");
        if (address.get("licenseId")) initial.licenseId = address.get("licenseId");

        this.store.delete(s.search);
        this.store.set(s.filters, initial);
        this.store.set(s.filtersOpen, false);
        this.store.set(s.chips, toChips(initial));
        this.store.set(s.software, []);
        this.store.set(s.licenses, []);
        this.store.set(s.sort, "-activated");
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

        getActivationOptions()
            .then((o) => {
                this.store.set(s.software, o.software);
                this.store.set(s.licenses, o.licenses);
                // A filter the address set has only its id; its name comes with the options.
                this.store.update(s.filters, (f) => ({
                    ...f,
                    softwareText: f.softwareText ?? o.software.find((x) => x.id === f.softwareId)?.text,
                    licenseText: f.licenseText ?? o.licenses.find((x) => x.id === f.licenseId)?.text,
                }));
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
        const f = this.store.get(s.filters) ?? {};
        this.store.set(s.loading, true);

        try {
            const result = await listActivations({
                q: this.search?.trim() || undefined,
                softwareId: f.softwareId ?? undefined,
                licenseId: f.licenseId ?? undefined,
                status: f.status ?? undefined,
                expiry: f.expiry ?? undefined,
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
                    ? "No activations"
                    : `${state.summary} ${result.total === 1 ? "activation" : "activations"}`,
            );
            this.store.delete(s.error);
            this.store.set(s.loaded, true);
        } catch (error) {
            if (request !== this.request) return;
            this.store.set(
                s.error,
                error instanceof ApiError ? error.message : "The activations could not be loaded.",
            );
        } finally {
            if (request === this.request) this.store.set(s.loading, false);
        }
    }

    /** A column header: descending first for dates, ascending first for text; then the other way. */
    sortBy(key: "activated" | "software" | "license" | "assignee" | "deactivated") {
        const first = key === "activated" || key === "deactivated" ? `-${key}` : key;
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

    setStatus(status: Filters["status"]) {
        this.store.set(s.filters.status, status);
    }

    setExpiry(expiry: Filters["expiry"]) {
        this.store.set(s.filters.expiry, expiry);
    }

    removeFilter(key: FilterKey) {
        this.store.update(s.filters, (f) =>
            key === "software" || key === "license"
                ? { ...f, [`${key}Id`]: undefined, [`${key}Text`]: undefined }
                : { ...f, [key]: undefined },
        );
    }

    clearFilters() {
        this.store.set(s.filters, {});
    }

    clearAll() {
        this.store.delete(s.search);
        this.store.set(s.filters, {});
    }
}
