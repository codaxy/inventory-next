import { createModel } from "cx/ui";

export interface Option {
    id: string;
    text: string;
}

/** The form, as the fields bind it: text keys absent until typed, the tags as picker records. */
export interface TypeDraft {
    name?: string | null;
    holdsLicences: boolean;
    description?: string | null;
    tags: Option[];
}

export interface TypeEditorState {
    /** `null` while creating. */
    id: string | null;
    /** Read-only, as a row opens it; editing is `…/:id/edit`, creating opens in it. */
    viewing: boolean;
    title: string;
    draft: TypeDraft;
    /** How many devices are of the type: what keeps it from being deleted. */
    deviceCount: number;
    tagOptions: Option[];
    loading: boolean;
    saving: boolean;
    /** Not found, or a failure no field owns. */
    error?: string;
    /** The server's messages by field. */
    errors: { name?: string; description?: string; tagIds?: string };
    valid: boolean;
    visited: boolean;
}

export interface Model {
    type: TypeEditorState;
    /** What the enclosing `Route` exposes: `new`, or the type's id. */
    $route: { id: string };
    $tag: Option;
}

export default createModel<Model>();

/** What the server is sent: trimmed text, empty as null, the tag ids. */
export const toForm = (draft: TypeDraft) => ({
    name: (draft.name ?? "").trim(),
    holdsLicences: !!draft.holdsLicences,
    description: draft.description?.trim() || null,
    tagIds: draft.tags.map((t) => t.id),
});

export const devicesText = (n: number) =>
    n === 0
        ? "No device is of this type."
        : n === 1
          ? "1 device is of this type."
          : `${n} devices are of this type.`;
