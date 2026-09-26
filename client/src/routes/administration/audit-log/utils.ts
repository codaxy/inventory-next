import type { AuditEntryDetail, AuditQuery, FieldValue } from "../../../api/auditLog";
import { endOfDay, formatDateTime, formatDay, startOfDay } from "../../../dates";
import { pageSize } from "../../../paging";
import { actionText, type Chip, type Filters, humanize } from "./model";

/** Milliseconds of quiet after the last keystroke before the search runs. */
export const searchDelay = 300;

/** A piece of a value; `mark` when it is what changed. */
export interface Segment {
    text: string;
    mark: boolean;
}

/** One field of an entry, shaped for the window. */
export interface FieldRow {
    name: string;
    changed: boolean;
    /** Text compared word by word; a changed value that is not is replaced whole. */
    diffed: boolean;
    /** The value before, for an update or a delete. */
    before: Segment[];
    /** The value after, for an update or a create. */
    after: Segment[];
    beforeEmpty: boolean;
    afterEmpty: boolean;
    /** A key's raw value under its resolved name, so the name can be checked. */
    beforeHint?: string;
    afterHint?: string;
}

const naive = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/;

/** Text as the reader should see it. `null` is empty — the window shows a dash. */
export function formatValue(value: FieldValue | null): string | null {
    if (!value) return null;

    switch (value.kind) {
        case "Boolean":
            return value.text === "true" ? "Yes" : "No";
        case "Instant":
            return formatDateTime(new Date(value.text));
        case "Date":
            return formatDay(startOfDay(value.text));
        case "DateTime": {
            // No offset: shown as written, never shifted into the viewer's zone.
            const [, y, mo, d, h, mi] = value.text.match(naive)!;
            return `${formatDay(new Date(+y, +mo - 1, +d))}, ${h}:${mi}`;
        }
        default:
            return value.text;
    }
}

/** Words and the whitespace between them, so a diff never splits a word. */
const tokenize = (text: string) => text.split(/(\s+)/).filter((t) => t !== "");

/** Above this many token pairs the table is not worth building; both sides are marked whole. */
const diffLimit = 250_000;

/**
 * Marks what was removed from `before` and added in `after`: the longest common subsequence of
 * their words, everything outside it marked.
 */
export function diffWords(before: string, after: string): { before: Segment[]; after: Segment[] } {
    const a = tokenize(before);
    const b = tokenize(after);

    if (a.length * b.length > diffLimit)
        return { before: [{ text: before, mark: true }], after: [{ text: after, mark: true }] };

    // lengths[i][j]: the common subsequence of a[i..] and b[j..].
    const lengths = Array.from({ length: a.length + 1 }, () => new Uint32Array(b.length + 1));
    for (let i = a.length - 1; i >= 0; i--)
        for (let j = b.length - 1; j >= 0; j--)
            lengths[i][j] =
                a[i] === b[j] ? lengths[i + 1][j + 1] + 1 : Math.max(lengths[i + 1][j], lengths[i][j + 1]);

    const left: Segment[] = [];
    const right: Segment[] = [];
    const push = (into: Segment[], text: string, mark: boolean) => {
        const last = into[into.length - 1];
        if (last && last.mark === mark) last.text += text;
        else into.push({ text, mark });
    };

    let i = 0;
    let j = 0;
    while (i < a.length && j < b.length) {
        if (a[i] === b[j]) {
            push(left, a[i++], false);
            push(right, b[j++], false);
        } else if (lengths[i + 1][j] >= lengths[i][j + 1]) push(left, a[i++], true);
        else push(right, b[j++], true);
    }
    while (i < a.length) push(left, a[i++], true);
    while (j < b.length) push(right, b[j++], true);

    // Whitespace alone is not a change worth pointing at.
    for (const side of [left, right]) for (const s of side) if (s.mark && !s.text.trim()) s.mark = false;

    return { before: left, after: right };
}

/** Fields in the order written, the id last; for an update, changed ones first. */
export function toFieldRows(detail: AuditEntryDetail): FieldRow[] {
    const isUpdate = detail.entry.action === "Update";

    const rows = detail.fields.map((field): FieldRow => {
        const refs = detail.references[field.name] ?? {};
        const name = (value: FieldValue | null) =>
            value ? (refs[value.text] ?? refs[value.text.toLowerCase()]) : undefined;

        const beforeName = name(field.old);
        const afterName = name(field.new);
        const before = beforeName ?? formatValue(field.old);
        const after = afterName ?? formatValue(field.new);

        const diffed =
            isUpdate && field.changed && !!before && !!after && !beforeName && field.old?.kind === "Text";
        const segments = diffed
            ? diffWords(before!, after!)
            : {
                  before: before ? [{ text: before, mark: false }] : [],
                  after: after ? [{ text: after, mark: false }] : [],
              };

        return {
            name: humanize(field.name),
            changed: field.changed,
            diffed,
            ...segments,
            beforeEmpty: !before,
            afterEmpty: !after,
            beforeHint: beforeName ? field.old!.text : undefined,
            afterHint: afterName ? field.new!.text : undefined,
        };
    });

    // The record's own id says nothing a reader wants first; it goes last.
    const ordered = [...rows.filter((r) => r.name !== "ID"), ...rows.filter((r) => r.name === "ID")];

    return isUpdate ? [...ordered.filter((r) => r.changed), ...ordered.filter((r) => !r.changed)] : ordered;
}

const range = (from?: string | null, to?: string | null) => {
    if (from && to)
        return from === to
            ? formatDay(startOfDay(from))
            : `${formatDay(startOfDay(from))} – ${formatDay(startOfDay(to))}`;
    if (from) return `From ${formatDay(startOfDay(from))}`;
    return `Until ${formatDay(startOfDay(to!))}`;
};

/** One removable chip per active filter, in the pane's order. */
export function toChips(filters: Filters): Chip[] {
    const chips: Chip[] = [];

    if (filters.entityId)
        chips.push({
            key: "entityId",
            text: `Record: ${filters.entityLabel ?? filters.entityId.slice(0, 8)}`,
        });
    if (filters.action) chips.push({ key: "action", text: actionText[filters.action] });
    if (filters.table) chips.push({ key: "table", text: humanize(filters.table) });
    if (filters.email) chips.push({ key: "email", text: filters.email });
    if (filters.from || filters.to) chips.push({ key: "range", text: range(filters.from, filters.to) });
    if (filters.inventoryNumber) chips.push({ key: "inventoryNumber", text: `#${filters.inventoryNumber}` });

    return chips;
}

export const inventoryNumberPattern = /^\d{1,9}$/;

/**
 * The request for what the screen holds. Days become the viewer's own midnights, the end one day
 * on, since the server's `to` is exclusive.
 */
export function toQuery(
    search: string | null | undefined,
    filters: Filters,
    sort: "time" | "-time",
    page: number,
): AuditQuery {
    const number = filters.inventoryNumber?.trim();

    return {
        q: search?.trim() || undefined,
        action: filters.action ?? undefined,
        table: filters.table ?? undefined,
        email: filters.email ?? undefined,
        from: filters.from ? startOfDay(filters.from).toISOString() : undefined,
        to: filters.to ? endOfDay(filters.to).toISOString() : undefined,
        entityId: filters.entityId ?? undefined,
        inventoryNumber: number && inventoryNumberPattern.test(number) ? Number(number) : undefined,
        sort,
        page,
        pageSize,
    };
}
