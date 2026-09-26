import type { Page } from "../paging";
import { send } from "./http";

export interface TagItem {
    id: string;
    name: string;
    description: string | null;
    typeCount: number;
    /** The first three by name; `typeCount` says how many more. */
    firstTypes: string[];
}

export interface TypeRef {
    id: string;
    name: string;
}

export interface TagDetail {
    id: string;
    name: string;
    description: string | null;
    types: TypeRef[];
}

export interface TagForm {
    name: string;
    description: string | null;
    typeIds: string[];
}

export interface TagQuery {
    q?: string;
    sort?: "name" | "-name" | "types" | "-types";
    page: number;
    pageSize: number;
}

const base = "/api/electronic-devices/tags";

export function listTags(query: TagQuery) {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(query))
        if (value !== undefined && value !== null && value !== "") params.set(key, String(value));
    return send<Page<TagItem>>(`${base}/?${params}`);
}

export const getTag = (id: string) => send<TagDetail>(`${base}/${id}`);

export const getTagOptions = () => send<{ types: { id: string; text: string }[] }>(`${base}/options`);

export const createTag = (form: TagForm) =>
    send<TagDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updateTag = (id: string, form: TagForm) =>
    send<TagDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deleteTag = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
