import type { Page } from "../paging";
import type { Permission } from "./auth";
import { send, toQuery } from "./http";

export interface RoleItem {
    id: string;
    name: string;
    description: string;
    isBuiltIn: boolean;
    isSystemAdmin: boolean;
    users: number;
}

export interface RoleDetail {
    id: string;
    name: string;
    description: string;
    isBuiltIn: boolean;
    isSystemAdmin: boolean;
    permissions: Permission[];
    /** How many users hold it; one held by any is not deleted. */
    users: number;
    /** The reader holds it, so may not change it. */
    isOwn: boolean;
    /** The reader holds everything it grants. */
    canManage: boolean;
}

export interface RoleForm {
    name: string;
    description: string | null;
    permissions: Permission[];
}

/** A permission as the server's catalogue lists it, and whether the reader may grant it. */
export interface CatalogueEntry {
    key: Permission;
    section: string;
    /** "View", "Edit", "Manage". */
    name: string;
    description: string;
    /** The view an edit cannot be held without. */
    implies: Permission | null;
    grantable: boolean;
}

/** The order is authority order, fixed; `rank` is its only name. */
export type RoleSort = "rank";

const base = "/api/administration/roles";

export const listRoles = (q: { q?: string; page: number; pageSize: number }) =>
    send<Page<RoleItem>>(`${base}/?${toQuery(q)}`);

export const getRoleCatalogue = () => send<CatalogueEntry[]>(`${base}/catalogue`);

export const getRole = (id: string) => send<RoleDetail>(`${base}/${id}`);
export const createRole = (form: RoleForm) =>
    send<RoleDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });
export const updateRole = (id: string, form: RoleForm) =>
    send<RoleDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });
export const resetRole = (id: string) => send<RoleDetail>(`${base}/${id}/reset`, { method: "POST" });
export const deleteRole = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
