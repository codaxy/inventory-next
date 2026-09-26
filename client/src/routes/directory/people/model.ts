import { createModel } from "cx/ui";

import type { PersonItem, PersonSort } from "../../../api/people";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    email: string;
    /** "12", absent when they hold none. */
    assets?: string;
    /** "assets": the count's word, for a phone's card, which has no header. */
    assetsWord: string;
    seats?: string;
    seatsWord: string;
}

/** People are searched, not filtered; the list keeps the shape every list does. */
export type Filters = Record<string, never>;

export interface PeopleListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: { key: string; text: string }[];
    sort: PersonSort;
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
    people: PeopleListState;
    $row: Row;
}

export default createModel<Model>();

export const toRows = (items: PersonItem[]): Row[] =>
    items.map((p) => ({
        id: p.id,
        name: p.name,
        email: p.email,
        assets: p.assets ? String(p.assets) : undefined,
        assetsWord: p.assets === 1 ? "asset" : "assets",
        seats: p.seats ? String(p.seats) : undefined,
        seatsWord: p.seats === 1 ? "seat" : "seats",
    }));
