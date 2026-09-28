import type { AuditEntry, AuditSort } from "../../../api/auditLog";
import { getAuditEntry, getAuditFacets, listAuditEntries } from "../../../api/auditLog";
import { ApiError } from "../../../api/http";
import type { AddressValue } from "../../../listAddress";
import { oneOf } from "../../../listAddress";
import { ListController } from "../../../listController";
import { showEntryWindow } from "./EntryWindow";
import m, { type FilterKey, type Filters, humanize, type Row, toRows } from "./model";
import { inventoryNumberPattern, recordIdPattern, searchDelay, toChips, toQuery } from "./utils";

const day = /^\d{4}-\d{2}-\d{2}$/;

export default class extends ListController<Filters, AuditEntry, Row, AuditSort, FilterKey> {
    protected readonly s = m.auditLog;
    protected readonly path = "~/administration/audit-log";
    protected readonly defaultSort = "-time";
    protected readonly sorts = ["-time", "time"] as const;
    protected readonly failure = "The audit log could not be loaded.";
    protected readonly filterDelay = searchDelay;

    /**
     * Records' names by id, learnt from the entry that opened a history or from the record's own
     * entries. The address and the filter field carry only the id, so the chip would otherwise read as
     * one; kept apart from the filters so an edited id never shows another record's name.
     */
    private readonly labels = new Map<string, string>();

    protected async fetch({
        q,
        sort,
        page,
        filters,
    }: {
        q?: string;
        sort: AuditSort;
        page: number;
        filters: Filters;
    }) {
        const result = await listAuditEntries(toQuery(q, filters, sort, page));
        const id = filters.entityId?.trim().toLowerCase();

        if (id && recordIdPattern.test(id) && !this.labels.has(id)) {
            const named = result.items.find((entry) => entry.entityId.toLowerCase() === id && entry.label);
            if (named?.label) {
                this.labels.set(id, named.label);
                this.store.set(this.s.chips, this.toChips(filters));
            }
        }

        return result;
    }

    protected toRows = toRows;

    protected toChips = (filters: Filters) =>
        toChips({ ...filters, entityLabel: this.labels.get(filters.entityId?.trim().toLowerCase() ?? "") });

    /** One record's history travels as its id; its name comes from the entry that opened it, when one did. */
    protected filtersFrom(query: URLSearchParams): Filters {
        const from = query.get("from");
        const to = query.get("to");
        const number = query.get("inventoryNumber");
        return {
            action: oneOf(query, "action", ["Create", "Update", "Delete"] as const),
            table: query.get("table"),
            email: query.get("email"),
            from: from && day.test(from) ? from : null,
            to: to && day.test(to) ? to : null,
            inventoryNumber: number && inventoryNumberPattern.test(number) ? number : null,
            entityId: query.get("entityId"),
        };
    }

    protected filtersTo = (f: Filters): Record<string, AddressValue> => ({
        action: f.action,
        table: f.table,
        email: f.email,
        from: f.from,
        to: f.to,
        inventoryNumber: f.inventoryNumber,
        entityId: f.entityId,
    });

    protected without(f: Filters, key: FilterKey): Filters {
        if (key === "range") return { ...f, from: undefined, to: undefined };
        return { ...f, [key]: undefined };
    }

    protected loadOptions() {
        this.store.set(m.auditLog.filtersValid, true);
        this.store.set(m.auditLog.tables, []);
        this.store.set(m.auditLog.emails, []);
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
    }

    /** Newest first, then oldest first. */
    sortBy(key: "time") {
        this.sortOn(key, true);
    }

    setAction(action: "Create" | "Update" | "Delete" | null) {
        this.store.set(m.auditLog.filters.action, action);
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
            this.labels.set(result.entityId.toLowerCase(), result.label);
            this.store.delete(m.auditLog.search);
            this.store.set(m.auditLog.filters, { entityId: result.entityId });
        }
    }
}
