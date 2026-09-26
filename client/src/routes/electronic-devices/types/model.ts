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
    /** Present only where the type holds licences: a flag, not a column of "No". */
    licences?: string;
    description?: string;
    tags: string;
    /** "+6" when the type carries more tags than the row names. */
    more?: string;
    devices: string;
}

export interface Filters {
    tags?: Option[];
    /** `null` or absent: either. */
    holdsLicences?: boolean | null;
}

export type FilterKey = "holdsLicences" | `tag:${string}`;

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
}

export interface Model {
    types: TypeListState;
    $row: Row;
    $chip: Chip;
}

export default createModel<Model>();

const devices = (n: number) => (n === 0 ? "No devices" : n === 1 ? "1 device" : `${n} devices`);

export const toRows = (items: TypeItem[]): Row[] =>
    items.map((t) => ({
        id: t.id,
        name: t.name,
        licences: t.holdsLicences ? "Holds licences" : undefined,
        // The original saved an emptied description as "", so blank is absent too.
        description: t.description || undefined,
        tags: t.tagCount === 0 ? "No tags" : t.firstTags.join(", "),
        more: t.tagCount > t.firstTags.length ? `+${t.tagCount - t.firstTags.length}` : undefined,
        devices: devices(t.deviceCount),
    }));

export const toChips = (filters: Filters): Chip[] => [
    ...(filters.tags ?? []).map((t) => ({ key: `tag:${t.id}` as const, text: `Tag: ${t.text}` })),
    ...(filters.holdsLicences == null
        ? []
        : [
              {
                  key: "holdsLicences" as const,
                  text: filters.holdsLicences ? "Holds licences" : "Holds no licences",
              },
          ]),
];
