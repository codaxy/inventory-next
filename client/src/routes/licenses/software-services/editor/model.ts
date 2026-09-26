import { createModel } from "cx/ui";

import type { Option } from "../../../../api/softwareServices";

/** The form, as the fields bind it: text keys absent until typed, a pick as its id and text. */
export interface Draft {
    name?: string | null;
    categoryId?: string | null;
    categoryText?: string;
    manufacturerId?: string | null;
    manufacturerText?: string;
    url?: string | null;
}

export interface EditorState {
    /** `null` while creating. */
    id: string | null;
    /** Read-only, as a row opens it; editing is `…/:id/edit`, creating opens in it. */
    viewing: boolean;
    title: string;
    draft: Draft;
    /** How many licence volumes are of it: what keeps it from being deleted. */
    volumeCount: number;
    categories: Option[];
    manufacturers: Option[];
    loading: boolean;
    saving: boolean;
    error?: string;
    errors: { name?: string; categoryId?: string; manufacturerId?: string; url?: string };
    valid: boolean;
    visited: boolean;
}

export interface Model {
    entry: EditorState;
    /** What the enclosing `Route` exposes: `new`, or the entry's id. */
    $route: { id: string };
}

export default createModel<Model>();

export const toForm = (d: Draft) => ({
    name: (d.name ?? "").trim(),
    categoryId: d.categoryId ?? null,
    manufacturerId: d.manufacturerId ?? null,
    url: d.url?.trim() || null,
});

export const volumesText = (n: number) =>
    n === 0
        ? "No licence volume is of it."
        : n === 1
          ? "1 licence volume is of it."
          : `${n} licence volumes are of it.`;
