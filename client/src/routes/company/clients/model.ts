import { createModel } from "cx/ui";

import type { ClientItem, ClientSort } from "../../../api/clients";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    /** "2", absent when there are none. */
    projects?: string;
    /** "projects": the count's word, for a phone's card, which has no header. */
    projectsWord: string;
}

/** Clients are searched, not filtered; the list keeps the shape every list does. */
export type Filters = Record<string, never>;

export interface ClientListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: { key: string; text: string }[];
    sort: ClientSort;
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
    clients: ClientListState;
    $row: Row;
}

export default createModel<Model>();

export const toRows = (items: ClientItem[]): Row[] =>
    items.map((c) => ({
        id: c.id,
        name: c.name,
        projects: c.projects ? String(c.projects) : undefined,
        projectsWord: c.projects === 1 ? "project" : "projects",
    }));
