import { createModel } from "cx/ui";

import type { Option, SoftwareServiceItem, SoftwareServiceSort } from "../../../api/softwareServices";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    category: string;
    manufacturer: string;
    url?: string;
    /** Absent when no licence volume is of it. */
    volumes?: string;
}

/** A picked option is its id and its text, as a single `LookupField` binds them. */
export interface Filters {
    categoryId?: string | null;
    categoryText?: string;
    manufacturerId?: string | null;
    manufacturerText?: string;
}

export type FilterKey = "category" | "manufacturer";

export interface Chip {
    key: FilterKey;
    text: string;
}

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: Chip[];
    categories: Option[];
    manufacturers: Option[];
    sort: SoftwareServiceSort;
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
    list: ListState;
    $row: Row;
    $chip: Chip;
}

export default createModel<Model>();

export const toRows = (items: SoftwareServiceItem[]): Row[] =>
    items.map((s) => ({
        id: s.id,
        name: s.name,
        category: s.category,
        manufacturer: s.manufacturer,
        url: s.url || undefined,
        volumes:
            s.volumeCount === 0 ? undefined : s.volumeCount === 1 ? "1 volume" : `${s.volumeCount} volumes`,
    }));

export const toChips = (f: Filters): Chip[] => [
    ...(f.categoryId ? [{ key: "category" as const, text: `Category: ${f.categoryText}` }] : []),
    ...(f.manufacturerId
        ? [{ key: "manufacturer" as const, text: `Manufacturer: ${f.manufacturerText}` }]
        : []),
];
