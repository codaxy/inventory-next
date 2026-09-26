import { createModel } from "cx/ui";

import type { TagItem } from "../../../api/electronicDeviceTags";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    description?: string;
    /** Absent when no type carries the tag: the cell shows "—", as any empty cell does. */
    types?: string;
    /** "+6" when more types carry the tag than the row names. */
    more?: string;
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
        // The original saved an emptied description as "", so blank is absent too.
        description: t.description || undefined,
        types: t.typeCount === 0 ? undefined : t.firstTypes.join(", "),
        more: t.typeCount > t.firstTypes.length ? `+${t.typeCount - t.firstTypes.length}` : undefined,
    }));
