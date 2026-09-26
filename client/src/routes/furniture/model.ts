import { createModel } from "cx/ui";

import type { Option } from "../../api/assets";
import type { FurnitureItem, FurnitureSort } from "../../api/furniture";
import { formatDate, formatMoney } from "../../licensing";
import { formatDay } from "../../dates";
import type { PagerState } from "../../paging";

export interface Row {
    id: string;
    number: string;
    name: string;
    /** Present only where the record is marked incomplete: a flag, not a column. */
    incomplete?: string;
    assignee: string;
    location?: string;
    type?: string;
    vendor: string;
    value: string;
    modified: string;
}

/** A picked option is its id and its text, as a single `LookupField` binds them. */
export interface Filters {
    typeId?: string | null;
    typeText?: string;
    vendorId?: string | null;
    vendorText?: string;
    personId?: string | null;
    personText?: string;
    locationId?: string | null;
    locationText?: string;
    from?: string | null;
    to?: string | null;
    incomplete?: boolean | null;
}

export type FilterKey = "type" | "vendor" | "person" | "location" | "range" | "incomplete";

export interface Chip {
    key: FilterKey;
    text: string;
}

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: Chip[];
    types: Option[];
    vendors: Option[];
    people: Option[];
    locations: Option[];
    sort: FurnitureSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    totalText: string;
    /** The spreadsheet of what the list selects — every row, not the page. */
    exportHref?: string;
}

export interface Model {
    list: ListState;
    $row: Row;
    $chip: Chip;
}

export default createModel<Model>();

export const toRows = (items: FurnitureItem[]): Row[] =>
    items.map((f) => ({
        id: f.id,
        number: f.number ? `#${f.number}` : "—",
        name: f.name,
        incomplete: f.incomplete ? "Incomplete" : undefined,
        assignee: f.assignee,
        location: f.location ?? undefined,
        type: f.type ?? undefined,
        vendor: f.vendor,
        value: formatMoney(f.purchaseValue)!,
        modified: formatDay(new Date(f.lastModified)),
    }));

const pickChip = (key: FilterKey, label: string, id?: string | null, text?: string): Chip[] =>
    id ? [{ key, text: `${label}: ${text ?? "…"}` }] : [];

export const toChips = (f: Filters): Chip[] => [
    ...pickChip("type", "Type", f.typeId, f.typeText),
    ...pickChip("vendor", "Vendor", f.vendorId, f.vendorText),
    ...pickChip("person", "Assignee", f.personId, f.personText),
    ...pickChip("location", "Location", f.locationId, f.locationText),
    ...(f.from || f.to
        ? [
              {
                  key: "range" as const,
                  text: `Bought ${f.from ? `from ${formatDate(f.from)}` : ""}${f.from && f.to ? " " : ""}${f.to ? `to ${formatDate(f.to)}` : ""}`,
              },
          ]
        : []),
    ...(f.incomplete == null
        ? []
        : [{ key: "incomplete" as const, text: f.incomplete ? "Incomplete" : "Complete" }]),
];
