import { createModel } from "cx/ui";

import type { Option } from "../../api/assets";
import type { DeviceItem, DeviceSort } from "../../api/electronicDevices";
import { formatDate } from "../../licensing";
import { formatDay } from "../../dates";
import type { PagerState } from "../../paging";

export interface Row {
    id: string;
    number: string;
    name: string;
    /** Present only where the record is marked incomplete: a flag, not a column. */
    incomplete?: string;
    model?: string;
    assignee: string;
    location?: string;
    type?: string;
    manufacturer?: string;
    modelCode?: string;
    serial?: string;
    modified: string;
}

/** A picked option is its id and its text, as a single `LookupField` binds them. */
export interface Filters {
    typeId?: string | null;
    typeText?: string;
    tagId?: string | null;
    tagText?: string;
    personId?: string | null;
    personText?: string;
    vendorId?: string | null;
    vendorText?: string;
    locationId?: string | null;
    locationText?: string;
    manufacturerId?: string | null;
    manufacturerText?: string;
    from?: string | null;
    to?: string | null;
    incomplete?: boolean | null;
}

export type FilterKey =
    "type" | "tag" | "person" | "vendor" | "location" | "manufacturer" | "range" | "incomplete";

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
    tags: Option[];
    people: Option[];
    vendors: Option[];
    locations: Option[];
    manufacturers: Option[];
    sort: DeviceSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    /** The search that ran is one id: an empty answer then says no record has it. */
    idSearch?: boolean;
    /** The spreadsheet of what the list selects — every row, not the page. */
    exportHref?: string;
}

export interface Model {
    list: ListState;
    $row: Row;
    $chip: Chip;
}

export default createModel<Model>();

export const toRows = (items: DeviceItem[]): Row[] =>
    items.map((d) => ({
        id: d.id,
        number: d.number ? `#${d.number}` : "—",
        name: d.name,
        incomplete: d.incomplete ? "Incomplete" : undefined,
        model: d.modelName || undefined,
        assignee: d.assignee,
        location: d.location ?? undefined,
        type: d.type ?? undefined,
        manufacturer: d.manufacturer ?? undefined,
        modelCode: d.modelCode || undefined,
        serial: d.serialNumber || undefined,
        modified: formatDay(new Date(d.lastModified)),
    }));

const pickChip = (key: FilterKey, label: string, id?: string | null, text?: string): Chip[] =>
    id ? [{ key, text: `${label}: ${text ?? "…"}` }] : [];

export const toChips = (f: Filters): Chip[] => [
    ...pickChip("type", "Type", f.typeId, f.typeText),
    ...pickChip("tag", "Tag", f.tagId, f.tagText),
    ...pickChip("person", "Assignee", f.personId, f.personText),
    ...pickChip("vendor", "Vendor", f.vendorId, f.vendorText),
    ...pickChip("location", "Location", f.locationId, f.locationText),
    ...pickChip("manufacturer", "Manufacturer", f.manufacturerId, f.manufacturerText),
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
