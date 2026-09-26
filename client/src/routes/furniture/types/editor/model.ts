import { createModel } from "cx/ui";

/** The form, as the fields bind it: text keys absent until typed. */
export interface TypeDraft {
    name?: string | null;
    description?: string | null;
}

export interface TypeEditorState {
    /** `null` while creating. */
    id: string | null;
    /** Read-only, as a row opens it; editing is `…/:id/edit`, creating opens in it. */
    viewing: boolean;
    title: string;
    draft: TypeDraft;
    /** How much furniture is of the type: what keeps it from being deleted. */
    furnitureCount: number;
    /** "3 pieces of furniture", absent when there is none. */
    furnitureText?: string;
    /** The furniture list filtered to this type. */
    furnitureHref?: string;
    loading: boolean;
    saving: boolean;
    /** Not found, or a failure no field owns. */
    error?: string;
    /** The server's messages by field. */
    errors: { name?: string; description?: string };
    valid: boolean;
    visited: boolean;
}

export interface Model {
    type: TypeEditorState;
    /** What the enclosing `Route` exposes: `new`, or the type's id. */
    $route: { id: string };
}

export default createModel<Model>();

/** What the server is sent: trimmed text, empty as null. */
export const toForm = (draft: TypeDraft) => ({
    name: (draft.name ?? "").trim(),
    description: draft.description?.trim() || null,
});
