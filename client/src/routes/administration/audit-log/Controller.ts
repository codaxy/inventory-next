import { Controller } from "cx/ui";

import { getAuditEntry, getAuditFacets, listAuditEntries } from "../../../api/auditLog";
import { ApiError } from "../../../api/http";
import { pager, pageSize } from "../../../paging";
import { showEntryWindow } from "./EntryWindow";
import m, { type FilterKey, humanize, type Row, toRows } from "./model";
import { searchDelay, toChips, toQuery } from "./utils";

export default class extends Controller {
    /** The search the list reflects; the box runs ahead of it while someone is typing. */
    private search: string | null = null;
    private timer?: ReturnType<typeof setTimeout>;
    /** Only the latest request may write: an older answer arriving late would show stale rows. */
    private request = 0;

    onInit() {
        this.store.delete(m.auditLog.search);
        this.store.set(m.auditLog.filters, {});
        this.store.set(m.auditLog.filtersOpen, false);
        this.store.set(m.auditLog.filtersValid, true);
        this.store.set(m.auditLog.sort, "-time");
        this.store.set(m.auditLog.page, 1);
        this.store.set(m.auditLog.pageSize, pageSize);
        this.store.set(m.auditLog.rows, []);
        this.store.set(m.auditLog.total, 0);
        this.store.set(m.auditLog.loading, false);
        this.store.set(m.auditLog.loaded, false);
        this.store.delete(m.auditLog.error);
        this.store.set(m.auditLog.pager, pager(1, pageSize, 0));
        this.store.set(m.auditLog.chips, []);
        this.store.set(m.auditLog.totalText, "");
        this.store.set(m.auditLog.tables, []);
        this.store.set(m.auditLog.emails, []);
        this.search = null;

        // Both wait for a pause: the box reacts per keystroke, and so does the inventory number.
        this.addTrigger("search", [m.auditLog.search], () => this.debounce());
        this.addTrigger("filters", [m.auditLog.filters], (filters) => {
            this.store.set(m.auditLog.chips, toChips(filters ?? {}));
            this.debounce();
        });

        getAuditFacets()
            .then((facets) => {
                this.store.set(
                    m.auditLog.tables,
                    facets.tables.map((t) => ({ id: t, text: humanize(t) })),
                );
                this.store.set(
                    m.auditLog.emails,
                    facets.emails.map((e) => ({ id: e, text: e })),
                );
            })
            .catch(() => {});

        this.load();
    }

    onDestroy() {
        clearTimeout(this.timer);
    }

    private debounce() {
        clearTimeout(this.timer);
        this.timer = setTimeout(() => {
            this.search = this.store.get(m.auditLog.search) ?? null;
            this.goTo(1);
        }, searchDelay);
    }

    goTo(page: number, scroll = false) {
        this.store.set(m.auditLog.page, page);
        this.load();

        if (scroll) window.scrollTo({ top: 0 });
    }

    async load() {
        const request = ++this.request;
        const page = this.store.get(m.auditLog.page);

        this.store.set(m.auditLog.loading, true);

        try {
            const result = await listAuditEntries(
                toQuery(
                    this.search,
                    this.store.get(m.auditLog.filters) ?? {},
                    this.store.get(m.auditLog.sort),
                    page,
                ),
            );

            if (request !== this.request) return;

            const state = pager(page, pageSize, result.total);

            // The list shrank under the reader: show its new last page rather than an empty one.
            if (result.items.length === 0 && result.total > 0) return this.goTo(state.pageCount);

            this.store.set(m.auditLog.rows, toRows(result.items));
            this.store.set(m.auditLog.total, result.total);
            this.store.set(m.auditLog.pager, state);
            this.store.set(
                m.auditLog.totalText,
                result.total === 0
                    ? "No changes"
                    : `${state.summary} ${result.total === 1 ? "change" : "changes"}`,
            );
            this.store.delete(m.auditLog.error);
            this.store.set(m.auditLog.loaded, true);
        } catch (error) {
            if (request !== this.request) return;
            this.store.set(
                m.auditLog.error,
                error instanceof ApiError ? error.message : "The audit log could not be loaded.",
            );
        } finally {
            if (request === this.request) this.store.set(m.auditLog.loading, false);
        }
    }

    toggleFilters() {
        this.store.toggle(m.auditLog.filtersOpen);
    }

    closeFilters() {
        this.store.set(m.auditLog.filtersOpen, false);
    }

    toggleSort() {
        this.store.update(m.auditLog.sort, (sort) => (sort === "time" ? "-time" : "time"));
        this.goTo(1);
    }

    setAction(action: "Create" | "Update" | "Delete" | null) {
        this.store.set(m.auditLog.filters.action, action);
    }

    removeFilter(key: FilterKey) {
        const f = m.auditLog.filters;

        if (key === "range") {
            this.store.delete(f.from);
            this.store.delete(f.to);
        } else if (key === "entityId") {
            this.store.delete(f.entityId);
            this.store.delete(f.entityLabel);
        } else this.store.delete(f[key]);
    }

    /** Search and filters both: the empty state's way out. */
    clearAll() {
        this.store.delete(m.auditLog.search);
        this.store.set(m.auditLog.filters, {});
    }

    clearFilters() {
        this.store.set(m.auditLog.filters, {});
    }

    async openEntry(row: Pick<Row, "id">): Promise<void> {
        let detail;

        // The list dims while the entry loads, so a slow answer does not read as a dead tap.
        this.store.set(m.auditLog.loading, true);

        try {
            detail = await getAuditEntry(row.id);
        } catch (error) {
            this.store.set(
                m.auditLog.error,
                error instanceof ApiError ? error.message : "That entry could not be opened.",
            );
            return;
        } finally {
            this.store.set(m.auditLog.loading, false);
        }

        const result = await showEntryWindow({ detail });

        if (result && result.kind === "open") return this.openEntry({ id: result.id });

        if (result && result.kind === "history") {
            this.store.delete(m.auditLog.search);
            this.store.set(m.auditLog.filters, {
                entityId: result.entityId,
                entityLabel: result.label,
            });
        }
    }
}
