import type { AccessorChain } from "cx/data";
import { Controller } from "cx/ui";

import { ApiError } from "./api/http";
import {
    type AddressValue,
    followAddress,
    intParam,
    listReturn,
    queryOf,
    toQueryString,
    writeAddress,
} from "./listAddress";
import $app from "./model";
import { type Page, pager, type PagerState, pageSize } from "./paging";

export { listReturn };

export interface Chip<K extends string = string> {
    key: K;
    text: string;
}

/** The state every list keeps, in the branch of the store named after its screen. */
export interface ListState<F, Row, Sort extends string = string, K extends string = string> {
    search?: string | null;
    filters: F;
    filtersOpen: boolean;
    chips: Chip<K>[];
    sort: Sort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    totalText: string;
}

const searchDelay = 300;

/**
 * A searchable, filtered, sorted, paged list whose whole state is in the address: read when the list
 * opens and whenever the address changes under it, written after every change — the search after its
 * pause — by replacing the history entry, so a view of the list can be linked, reloaded and returned to.
 * Only the latest request writes: an answer that is not the newest is dropped.
 */
export abstract class ListController<
    F extends object,
    Item,
    Row,
    Sort extends string = string,
    K extends string = string,
> extends Controller {
    /** The screen's branch of the store. */
    protected abstract readonly s: AccessorChain<ListState<F, Row, Sort, K>>;
    /** The list's own address, `~/licenses`. */
    protected abstract readonly path: string;
    protected abstract readonly defaultSort: Sort;
    protected abstract readonly sorts: readonly Sort[];
    /** "licence", "licences", "No licences". */
    protected abstract readonly nouns: readonly [string, string, string];
    protected abstract readonly failure: string;

    protected abstract fetch(q: {
        q?: string;
        sort: Sort;
        page: number;
        pageSize: number;
        filters: F;
    }): Promise<Page<Item>>;
    protected abstract toRows(items: Item[]): Row[];
    protected abstract toChips(filters: F): Chip<K>[];
    /** The filters as the address holds them: ids and values under the API's names. */
    protected abstract filtersFrom(query: URLSearchParams): F;
    protected abstract filtersTo(filters: F): Record<string, AddressValue>;
    /** Removes one chip's filter. */
    protected abstract without(filters: F, key: K): F;
    /** Loads the pickers' options; names the address's ids once they arrive. */
    protected loadOptions(): void {}
    /**
     * The export of what the list selects: its request without the page. A list without one has no
     * spreadsheet; a plain link, since a download carries the session cookie.
     */
    protected exportUrl?(q: { q?: string; sort: Sort; filters: F }): string;
    /** Where the export's link is kept, for a list that has one. */
    protected readonly exportHref?: AccessorChain<string | undefined>;
    /** A filter typed rather than picked — an inventory number — waits for the search's pause too. */
    protected readonly filterDelay: number = 0;

    private search: string | null = null;
    private timer?: ReturnType<typeof setTimeout>;
    private request = 0;
    private written = "";
    /** While the address is being applied, the triggers it sets off do not reset the page. */
    private applying = false;
    private filtersSeen = "";

    onInit() {
        const s = this.s;
        this.store.set(s.filtersOpen, false);
        this.store.set(s.rows, []);
        this.store.set(s.total, 0);
        this.store.set(s.loading, false);
        this.store.set(s.loaded, false);
        this.store.delete(s.error);
        this.store.set(s.pager, pager(1, pageSize, 0));
        this.store.set(s.totalText, "");

        this.apply(queryOf(this.store.get($app.url)), false);

        this.addTrigger("search", [s.search], (value) => {
            if (this.applying || (value ?? null) === this.search) return;
            clearTimeout(this.timer);
            this.timer = setTimeout(() => {
                this.search = this.store.get(s.search) ?? null;
                this.goTo(1);
            }, searchDelay);
        });

        // Filters apply as they change; the chips say what is filtering while the pane is shut.
        this.addTrigger("filters", [s.filters], (value) => {
            const filters = (value ?? {}) as F;
            this.store.set(s.chips, this.toChips(filters));
            const seen = JSON.stringify(this.filtersTo(filters));
            if (this.applying || seen === this.filtersSeen) return;
            this.filtersSeen = seen;
            clearTimeout(this.timer);
            if (this.filterDelay === 0) return this.goTo(1);
            this.timer = setTimeout(() => {
                this.search = this.store.get(s.search) ?? null;
                this.goTo(1);
            }, this.filterDelay);
        });

        followAddress(
            this,
            this.path,
            () => this.written,
            (query) => this.apply(query, true),
        );

        this.loadOptions();
        this.load();
    }

    onDestroy() {
        clearTimeout(this.timer);
    }

    /** The address's state into the store; the list reloads when it was already showing. */
    private apply(query: URLSearchParams, reload: boolean) {
        const s = this.s;
        this.applying = true;
        const q = query.get("q");
        this.search = q;
        if (q) this.store.set(s.search, q);
        else this.store.delete(s.search);

        const filters = this.filtersFrom(query);
        this.filtersSeen = JSON.stringify(this.filtersTo(filters));
        this.store.set(s.filters, filters);
        this.store.set(s.chips, this.toChips(filters));

        const sort = query.get("sort") as Sort | null;
        this.store.set(s.sort, sort && this.sorts.includes(sort) ? sort : this.defaultSort);
        this.store.set(s.page, intParam(query, "page", 1));
        this.applying = false;

        if (reload) this.load();
    }

    goTo(page: number, scroll = false) {
        this.store.set(this.s.page, page);
        this.load();
        if (scroll) window.scrollTo({ top: 0 });
    }

    async load() {
        const s = this.s;
        const request = ++this.request;
        const page = this.store.get(s.page);
        const sort = this.store.get(s.sort);
        const filters = this.store.get(s.filters) ?? ({} as F);
        const q = this.search?.trim() || undefined;
        this.store.set(s.loading, true);

        const address = {
            q,
            ...this.filtersTo(filters),
            sort: sort === this.defaultSort ? undefined : sort,
            page: page > 1 ? page : undefined,
        };
        // Known before it is written: the address trigger fires inside the write, and must recognise
        // the list's own change rather than apply it back over what only the store holds.
        this.written = toQueryString(address);
        writeAddress(this.store, this.path, address);
        if (this.exportUrl && this.exportHref)
            this.store.set(this.exportHref, this.exportUrl({ q, sort, filters }));

        try {
            const result = await this.fetch({ q, sort, page, pageSize, filters });
            if (request !== this.request) return;

            const state = pager(page, pageSize, result.total);
            if (result.items.length === 0 && result.total > 0) return this.goTo(state.pageCount);

            const [one, many, none] = this.nouns;
            this.store.set(s.rows, this.toRows(result.items));
            this.store.set(s.total, result.total);
            this.store.set(s.pager, state);
            this.store.set(
                s.totalText,
                result.total === 0 ? none : `${state.summary} ${result.total === 1 ? one : many}`,
            );
            this.store.delete(s.error);
            this.store.set(s.loaded, true);
        } catch (error) {
            if (request !== this.request) return;
            this.store.set(s.error, error instanceof ApiError ? error.message : this.failure);
        } finally {
            if (request === this.request) this.store.set(s.loading, false);
        }
    }

    /**
     * A column header: its key one way, then the other, then back. `firstDescending` for the keys a
     * reader wants newest or largest first.
     */
    protected sortOn(key: string, firstDescending = false) {
        const first = (firstDescending ? `-${key}` : key) as Sort;
        const second = (firstDescending ? key : `-${key}`) as Sort;
        this.store.update(this.s.sort, (sort) => (sort === first ? second : first));
        this.goTo(1);
    }

    toggleFilters() {
        this.store.toggle(this.s.filtersOpen);
    }

    closeFilters() {
        this.store.set(this.s.filtersOpen, false);
    }

    removeFilter(key: K) {
        this.store.update(this.s.filters, (f) => this.without(f ?? ({} as F), key));
    }

    clearFilters() {
        this.store.set(this.s.filters, {} as F);
    }

    /** Search and filters both: the empty state's way out. */
    clearAll() {
        this.store.delete(this.s.search);
        this.store.set(this.s.filters, {} as F);
    }
}
