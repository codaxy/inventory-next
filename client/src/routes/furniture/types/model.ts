import { createModel } from "cx/ui";

import type { FurnitureTypeItem, FurnitureTypeSort } from "../../../api/furnitureTypes";
import { piecesOfFurniture } from "../../../furniture";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    description?: string;
    /** "3", absent when no furniture is of the type. */
    furniture?: string;
    /** "pieces of furniture": the count's word, for a phone's card, which has no header. */
    furnitureWord: string;
}

/** Types are searched, not filtered; the list keeps the shape every list does. */
export type Filters = Record<string, never>;

export interface TypeListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: { key: string; text: string }[];
    sort: FurnitureTypeSort;
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
    types: TypeListState;
    $row: Row;
}

export default createModel<Model>();

export const toRows = (items: FurnitureTypeItem[]): Row[] =>
    items.map((t) => ({
        id: t.id,
        name: t.name,
        // The original saved an emptied description as "", so blank is absent too.
        description: t.description || undefined,
        furniture: t.furnitureCount === 0 ? undefined : String(t.furnitureCount),
        furnitureWord: piecesOfFurniture(t.furnitureCount),
    }));
