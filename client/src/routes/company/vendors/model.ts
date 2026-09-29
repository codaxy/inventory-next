import { createModel } from "cx/ui";

import type { VendorItem, VendorSort } from "../../../api/vendors";
import { counted } from "../../../components/searchList";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    contact?: string;
    email?: string;
    phone?: string;
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
    sort: VendorSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    /** The search that ran is one id: an empty answer then says no record has it. */
    idSearch?: boolean;
    exportHref?: string;
}

export interface Model {
    list: ListState;
    $row: Row;
}

export default createModel<Model>();

export const toRows = (items: VendorItem[]): Row[] =>
    items.map((v) => {
        const assets = counted(v.assets, "asset", "assets");
        return {
            id: v.id,
            name: v.name,
            contact: v.contactPerson || undefined,
            email: v.email || undefined,
            phone: v.phone || undefined,
            assets: assets.value,
            assetsWord: assets.word,
        };
    });
