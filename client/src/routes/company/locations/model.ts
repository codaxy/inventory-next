import { createModel } from "cx/ui";

import type { LocationItem, LocationSort } from "../../../api/locations";
import { counted } from "../../../components/searchList";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    /** "Main 1, Banja Luka". */
    address: string;
    room?: string;
    assets?: string;
    assetsWord: string;
}

/** Searched, not filtered; the list keeps the shape every list does. */
export type Filters = Record<string, never>;

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: { key: string; text: string }[];
    sort: LocationSort;
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
    list: ListState;
    $row: Row;
}

export default createModel<Model>();

export const toRows = (items: LocationItem[]): Row[] =>
    items.map((l) => {
        const assets = counted(l.assets, "asset", "assets");
        const street = [l.street, l.houseNumber].filter((x) => x != null && x !== "").join(" ");
        return {
            id: l.id,
            name: l.name,
            address: [street, l.city].filter(Boolean).join(", "),
            room: l.room || undefined,
            assets: assets.value,
            assetsWord: assets.word,
        };
    });
