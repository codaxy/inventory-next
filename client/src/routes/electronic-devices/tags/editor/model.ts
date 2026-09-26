import { createModel } from "cx/ui";

export interface Option {
    id: string;
    text: string;
}

/** The form, as the fields bind it: text keys absent until typed, the types as picker records. */
export interface TagDraft {
    name?: string | null;
    description?: string | null;
    types: Option[];
}

export interface TagEditorState {
    /** `null` while creating. */
    id: string | null;
    title: string;
    draft: TagDraft;
    typeOptions: Option[];
    loading: boolean;
    saving: boolean;
    /** Not found, or a failure no field owns. */
    error?: string;
    /** The server's messages by field. */
    errors: { name?: string; description?: string; typeIds?: string };
    valid: boolean;
    visited: boolean;
}

export interface Model {
    tag: TagEditorState;
    /** What the enclosing `Route` exposes: `new`, or the tag's id. */
    $route: { id: string };
}

export default createModel<Model>();

/** What the server is sent: trimmed text, empty as null, the type ids. */
export const toForm = (draft: TagDraft) => ({
    name: (draft.name ?? "").trim(),
    description: draft.description?.trim() || null,
    typeIds: draft.types.map((t) => t.id),
});
