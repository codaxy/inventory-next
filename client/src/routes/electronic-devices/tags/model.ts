import { createModel } from "cx/ui";

import type { TagItem } from "../../../api/electronicDeviceTags";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    description?: string;
    types: string;
}

export interface TagListState {
    search?: string | null;
    sort: "name" | "-name" | "types" | "-types";
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
    tags: TagListState;
    $row: Row;
}

export default createModel<Model>();

export const toRows = (items: TagItem[]): Row[] =>
    items.map((t) => ({
        id: t.id,
        name: t.name,
        description: t.description ?? undefined,
        types: t.typeCount === 0 ? "No types" : t.typeCount === 1 ? "1 type" : `${t.typeCount} types`,
    }));
