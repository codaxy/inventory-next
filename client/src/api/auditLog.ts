import type { Page } from "../paging";
import { send } from "./http";

export type AuditAction = "Create" | "Update" | "Delete";

export type AuditSort = "time" | "-time";

export interface AuditEntry {
    id: string;
    time: string;
    email: string;
    action: AuditAction;
    /** The entity's class name, as the log records it: `ElectronicDevice`. */
    table: string;
    entityId: string;
    label: string | null;
    inventoryNumber: number | null;
    /** What an update changed; empty for a create or a delete. */
    changed: string[];
}

export type ValueKind = "Text" | "Number" | "Boolean" | "Instant" | "DateTime" | "Date";

export interface FieldValue {
    text: string;
    kind: ValueKind;
}

export interface FieldChange {
    name: string;
    old: FieldValue | null;
    new: FieldValue | null;
    changed: boolean;
}

export interface AuditEntryDetail {
    entry: AuditEntry;
    fields: FieldChange[];
    /** Display text for foreign-key values, by field and then by value. */
    references: Record<string, Record<string, string>>;
    /** The other rows of the same save. */
    related: AuditEntry[];
}

export interface AuditFacets {
    tables: string[];
    emails: string[];
}

export interface AuditQuery {
    q?: string;
    action?: string;
    table?: string;
    email?: string;
    /** Instants, ISO 8601; `from` inclusive, `to` exclusive. */
    from?: string;
    to?: string;
    entityId?: string;
    inventoryNumber?: number;
    sort?: AuditSort;
    page: number;
    pageSize: number;
}

const base = "/api/administration/audit-log";

export function listAuditEntries(query: AuditQuery) {
    const params = new URLSearchParams();

    for (const [key, value] of Object.entries(query))
        if (value !== undefined && value !== null && value !== "") params.set(key, String(value));

    return send<Page<AuditEntry>>(`${base}/?${params}`);
}

export const getAuditEntry = (id: string) => send<AuditEntryDetail>(`${base}/${id}`);

export const getAuditFacets = () => send<AuditFacets>(`${base}/facets`);
