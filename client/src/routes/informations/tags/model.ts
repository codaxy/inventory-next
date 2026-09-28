import { createModel } from "cx/ui";

import type { GroupItem, GroupSort } from "../../../api/informations";
import { counted } from "../../../components/searchList";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    description?: string;
    information?: string;
    informationWord: string;
}

/** Searched, not filtered; the list keeps the shape every list does. */
export type Filters = Record<string, never>;

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: { key: string; text: string }[];
    sort: GroupSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    /** The search that ran is one id: an empty answer then says no record has it. */
    idSearch?: boolean;
}

export interface Model {
    list: ListState;
    $row: Row;
}

export default createModel<Model>();

export const toRows = (items: GroupItem[]): Row[] =>
    items.map((t) => {
        const information = counted(t.information, "piece of information", "pieces of information");
        return {
            id: t.id,
            name: t.name,
            // The original saved an emptied description as "", so blank is absent too.
            description: t.description || undefined,
            information: information.value,
            informationWord: information.word,
        };
    });
