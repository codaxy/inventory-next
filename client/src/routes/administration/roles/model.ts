import { createModel } from "cx/ui";

import type { RoleItem, RoleSort } from "../../../api/roles";
import { counted } from "../../../components/searchList";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    description?: string;
    builtIn: boolean;
    users?: string;
    usersWord: string;
}

/** Searched, not filtered; the list keeps the shape every list does. */
export type Filters = Record<string, never>;

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: { key: string; text: string }[];
    sort: RoleSort;
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

export const toRows = (items: RoleItem[]): Row[] =>
    items.map((r) => {
        const users = counted(r.users, "user", "users");
        return {
            id: r.id,
            name: r.name,
            description: r.description || undefined,
            builtIn: r.isBuiltIn,
            users: users.value,
            usersWord: users.word,
        };
    });
