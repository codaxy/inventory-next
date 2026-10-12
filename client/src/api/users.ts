import type { Page } from "../paging";
import { send, toQuery } from "./http";

export interface UserItem {
    id: string;
    name: string;
    email: string;
    role: string;
    isActive: boolean;
    createdAt: string;
}

export interface UserDetail {
    id: string;
    email: string;
    name: string;
    roleId: string;
    role: string;
    isActive: boolean;
    createdAt: string;
    /** The reader's own user: only its name may change. */
    isSelf: boolean;
    /** Whether the reader may change it at all: its role grants nothing the reader lacks. */
    canManage: boolean;
}

/** Adding takes the email; once added it is fixed, and the server ignores it. */
export interface UserForm {
    email?: string;
    name: string;
    roleId: string | null;
}

export interface RoleOption {
    id: string;
    name: string;
    description: string;
}

export type UserSort = `${"" | "-"}${"name" | "email" | "role" | "created"}`;

const base = "/api/administration/users";

export const listUsers = (q: { q?: string; sort?: UserSort; page: number; pageSize: number }) =>
    send<Page<UserItem>>(`${base}/?${toQuery(q)}`);

/** The addresses `Auth:AllowedUsers` lets in that are not users yet. */
export const listConfiguredUsers = () => send<string[]>(`${base}/configured`);

/** The roles the reader may give, in authority order. */
export const getUserOptions = () => send<{ roles: RoleOption[] }>(`${base}/options`);

export const getUser = (id: string) => send<UserDetail>(`${base}/${id}`);
export const createUser = (form: UserForm) =>
    send<UserDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });
export const updateUser = (id: string, form: UserForm) =>
    send<UserDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });
export const deactivateUser = (id: string) =>
    send<UserDetail>(`${base}/${id}/deactivate`, { method: "POST" });
export const reactivateUser = (id: string) =>
    send<UserDetail>(`${base}/${id}/reactivate`, { method: "POST" });
