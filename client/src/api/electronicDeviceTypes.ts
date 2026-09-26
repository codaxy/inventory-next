import type { Page } from "../paging";
import { send } from "./http";

export interface TypeItem {
    id: string;
    name: string;
    holdsLicences: boolean;
    description: string | null;
    tagCount: number;
    /** The first three by name; `tagCount` says how many more. */
    firstTags: string[];
    deviceCount: number;
}

export interface TagRef {
    id: string;
    name: string;
}

export interface TypeDetail {
    id: string;
    name: string;
    holdsLicences: boolean;
    description: string | null;
    tags: TagRef[];
    deviceCount: number;
}

export interface TypeForm {
    name: string;
    holdsLicences: boolean;
    description: string | null;
    tagIds: string[];
}

export type TypeSort = "name" | "-name" | "tags" | "-tags" | "devices" | "-devices";

export interface TypeQuery {
    q?: string;
    /** A type must carry every one. */
    tagIds?: string[];
    holdsLicences?: boolean;
    sort?: TypeSort;
    page: number;
    pageSize: number;
}

const base = "/api/electronic-devices/types";

export function listTypes({ tagIds, ...query }: TypeQuery) {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(query))
        if (value !== undefined && value !== null && value !== "") params.set(key, String(value));
    for (const id of tagIds ?? []) params.append("tagId", id);
    return send<Page<TypeItem>>(`${base}/?${params}`);
}

export const getType = (id: string) => send<TypeDetail>(`${base}/${id}`);

export const getTypeOptions = () => send<{ tags: { id: string; text: string }[] }>(`${base}/options`);

export const createType = (form: TypeForm) =>
    send<TypeDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updateType = (id: string, form: TypeForm) =>
    send<TypeDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deleteType = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
