import { createModel } from "cx/ui";

import type { OnVolumeItem, Sort } from "../../../api/infrastructure";
import { counted } from "../../../components/searchList";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    license: string;
    software: string;
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
    sort: Sort;
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
}

export default createModel<Model>();

export const toRows = (items: OnVolumeItem[]): Row[] =>
    items.map((t) => {
        const information = counted(t.information, "piece of information", "pieces of information");
        return {
            id: t.id,
            name: t.name,
            license: t.license,
            software: t.software,
            information: information.value,
            informationWord: information.word,
        };
    });
