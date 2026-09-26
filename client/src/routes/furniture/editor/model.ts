import { createModel } from "cx/ui";

import type { FurnitureDetail, FurnitureForm, FurnitureOptions } from "../../../api/furniture";
import {
    type AssetDraft,
    assetParts,
    emptyAssetOptions,
    text,
    toAssetForm,
    toDraftOf,
} from "../../../assets";

/** The form, as the fields bind it: the asset's fields, then the furniture's own. */
export interface Draft extends AssetDraft {
    typeId?: string | null;
    typeText?: string;
    model?: string | null;
}

export interface EditorState {
    /** `null` while creating. */
    id: string | null;
    viewing: boolean;
    title: string;
    /** "#100893" beside the title, once there is one. */
    number?: string;
    draft: Draft;
    /** What the server said it held when loaded: echoed on save so an edit meanwhile is not overwritten. */
    lastModified?: string;
    /** Computed from the three weights as the server will; "—" until all three are chosen. */
    importance: string;
    options: FurnitureOptions;
    loading: boolean;
    saving: boolean;
    error?: string;
    /** The save was refused because someone saved first. */
    stale: boolean;
    errors: Partial<Record<string, string>>;
    valid: boolean;
    visited: boolean;
}

export interface Model {
    furniture: EditorState;
    $route: { id: string };
}

export default createModel<Model>();

export const emptyOptions: FurnitureOptions = { ...emptyAssetOptions, types: [] };

export const blankDraft = (): Draft => ({ incomplete: false });

/** A loaded piece as the form: every pick as id and text. */
export function toDraft(f: FurnitureDetail): Draft {
    const asset = assetParts(f);
    return toDraftOf<Draft>({ ...asset.plain, model: f.model }, { ...asset.refs, type: f.type });
}

/** What the server is sent. */
export const toForm = (d: Draft, lastModified?: string): FurnitureForm => ({
    ...toAssetForm(d, lastModified),
    typeId: d.typeId ?? null,
    model: text(d.model),
});
