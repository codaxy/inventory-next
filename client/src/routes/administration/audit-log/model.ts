import { createModel } from "cx/ui";

import type { AuditAction, AuditEntry } from "../../../api/auditLog";
import { formatDayHeading, formatTime, encodeDate } from "../../../dates";
import type { PagerState } from "../../../paging";

export interface Option {
    id: string;
    text: string;
}

/**
 * Everything the pane sets. Text keys are never initialised — an absent key is an unset filter —
 * and a picker writes `null` when cleared.
 */
export interface Filters {
    action?: AuditAction | null;
    table?: string | null;
    email?: string | null;
    /** `YYYY-MM-DD`, in the viewer's timezone; both ends inclusive on screen. */
    from?: string | null;
    to?: string | null;
    /** Digits only; validated by the field, parsed when the query is built. */
    inventoryNumber?: string | null;
    /** One record's history, set from an entry rather than typed. */
    entityId?: string | null;
    entityLabel?: string | null;
}

export type FilterKey = "action" | "table" | "email" | "range" | "inventoryNumber" | "entityId";

export interface Chip {
    key: FilterKey;
    text: string;
}

/** A list entry, shaped for display. */
export interface Row {
    id: string;
    action: AuditAction;
    actionText: string;
    actionIcon: string;
    type: string;
    label: string;
    inventoryNumber?: string;
    email: string;
    time: string;
    /** When the row is the first of its day: that day's heading. */
    dayHeading?: string;
    summary: string;
    /** "+2" when the update changed more fields than the summary names. */
    more?: string;
}

export interface AuditLogState {
    /** The search box as typed; the query takes it after a pause. */
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    filtersValid: boolean;
    sort: "-time" | "time";
    page: number;
    pageSize: number;

    rows: Row[];
    total: number;
    loading: boolean;
    /** False until the first answer, so an empty list is not claimed before one arrives. */
    loaded: boolean;
    error?: string;
    pager: PagerState;
    chips: Chip[];
    totalText: string;

    tables: Option[];
    emails: Option[];
}

export interface Model {
    auditLog: AuditLogState;
    $row: Row;
    $chip: Chip;
}

export default createModel<Model>();

const acronym = /^[A-Z0-9]{2,}$/;

/**
 * A class or property name as words: `ElectronicDevice` → "Electronic device", `IPAddress` →
 * "IP address", `URL` stays. A foreign key drops its `Id`: `LocationId` → "Location".
 */
export function humanize(name: string): string {
    if (name === "Id") return "ID";

    const words = name.replace(/(.)Id$/, "$1").match(/[A-Z]+(?=[A-Z][a-z])|[A-Z]?[a-z]+|[A-Z]+|\d+/g) ?? [
        name,
    ];

    return words
        .map((word, i) =>
            acronym.test(word) ? word : i === 0 ? word[0].toUpperCase() + word.slice(1) : word.toLowerCase(),
        )
        .join(" ");
}

export const actionText: Record<AuditAction, string> = {
    Create: "Created",
    Update: "Updated",
    Delete: "Deleted",
};

export const actionIcon: Record<AuditAction, string> = {
    Create: "created",
    Update: "updated",
    Delete: "deleted",
};

/** The record's own words, falling back to a short id where the log holds no name. */
export function describeEntity(entry: AuditEntry): { type: string; label: string; inventoryNumber?: string } {
    return {
        type: humanize(entry.table),
        label: entry.label || entry.entityId.slice(0, 8),
        inventoryNumber: entry.inventoryNumber == null ? undefined : `#${entry.inventoryNumber}`,
    };
}

/** At most three names, and how many more: a row is one line on a phone. */
export function summarise(entry: AuditEntry): { summary: string; more?: string } {
    if (entry.action !== "Update")
        return { summary: entry.action === "Create" ? "New record" : "Record removed" };
    if (entry.changed.length === 0) return { summary: "No field changed" };

    const names = entry.changed.map(humanize);
    return {
        summary: names.slice(0, 3).join(", "),
        more: names.length > 3 ? `+${names.length - 3}` : undefined,
    };
}

/** Rows in list order; a row opening a new local day carries that day's heading. */
export function toRows(entries: AuditEntry[], now = new Date()): Row[] {
    let day = "";

    return entries.map((entry) => {
        const time = new Date(entry.time);
        const thisDay = encodeDate(time);
        const dayHeading = thisDay === day ? undefined : formatDayHeading(time, now);
        day = thisDay;

        return {
            id: entry.id,
            action: entry.action,
            actionText: actionText[entry.action],
            actionIcon: actionIcon[entry.action],
            ...describeEntity(entry),
            email: entry.email,
            time: formatTime(time),
            dayHeading,
            ...summarise(entry),
        };
    });
}
