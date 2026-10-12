import { createModel } from "cx/ui";

import type { UserItem, UserSort } from "../../../api/users";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    email: string;
    role: string;
    ended: boolean;
}

/** Searched, not filtered; the list keeps the shape every list does. */
export type Filters = Record<string, never>;

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: { key: string; text: string }[];
    sort: UserSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    idSearch?: boolean;
}

export interface Model {
    list: ListState;
    $row: Row;
}

export default createModel<Model>();

export const toRows = (items: UserItem[]): Row[] =>
    items.map((u) => ({ id: u.id, name: u.name, email: u.email, role: u.role, ended: !u.isActive }));
