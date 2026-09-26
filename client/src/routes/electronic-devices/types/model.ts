import { createModel } from "cx/ui";

import type { TypeItem, TypeSort } from "../../../api/electronicDeviceTypes";
import type { PagerState } from "../../../paging";

export interface Option {
    id: string;
    text: string;
}

export interface Row {
    id: string;
    name: string;
    /** Present only where the type holds licenses: a flag, not a column of "No". */
    licenses?: string;
    description?: string;
    /** Absent when the type carries none: the cell shows "—", as any empty cell does. */
    tags?: string;
    /** "+6" when the type carries more tags than the row names. */
    more?: string;
    /** Absent when no device is of the type. */
    devices?: string;
}

export interface Filters {
    tags?: Option[];
    /** `null` or absent: either. */
    holdsLicenses?: boolean | null;
}

export type FilterKey = "holdsLicenses" | `tag:${string}`;

export interface Chip {
    key: FilterKey;
    text: string;
}

export interface TypeListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: Chip[];
    tagOptions: Option[];
    sort: TypeSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    totalText: string;
    /** The search that ran is one id: an empty answer then says no record has it. */
    idSearch?: boolean;
}

export interface Model {
    types: TypeListState;
    $row: Row;
    $chip: Chip;
}

export default createModel<Model>();

const devices = (n: number) => (n === 0 ? undefined : n === 1 ? "1 device" : `${n} devices`);

export const toRows = (items: TypeItem[]): Row[] =>
    items.map((t) => ({
        id: t.id,
        name: t.name,
        licenses: t.holdsLicenses ? "Holds licenses" : undefined,
        // The original saved an emptied description as "", so blank is absent too.
        description: t.description || undefined,
        tags: t.tagCount === 0 ? undefined : t.firstTags.join(", "),
        more: t.tagCount > t.firstTags.length ? `+${t.tagCount - t.firstTags.length}` : undefined,
        devices: devices(t.deviceCount),
    }));

export const toChips = (filters: Filters): Chip[] => [
    ...(filters.tags ?? []).map((t) => ({ key: `tag:${t.id}` as const, text: `Tag: ${t.text || "…"}` })),
    ...(filters.holdsLicenses == null
        ? []
        : [
              {
                  key: "holdsLicenses" as const,
                  text: filters.holdsLicenses ? "Holds licenses" : "Holds no licenses",
              },
          ]),
];
