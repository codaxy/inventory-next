import type { Page } from "../paging";
import { send, toQuery } from "./http";

export interface ClientItem {
    id: string;
    name: string;
    projects: number;
}

export interface ClientDetail {
    id: string;
    name: string;
    projectCount: number;
    /** The first by name. */
    projects: { id: string; name: string; owner: string | null }[];
}

export interface ClientForm {
    name: string;
}

export type ClientSort = `${"" | "-"}${"name" | "projects"}`;

export interface ClientQuery {
    q?: string;
    sort?: ClientSort;
    page: number;
    pageSize: number;
}

const base = "/api/company/clients";

export const listClients = (q: ClientQuery) => send<Page<ClientItem>>(`${base}/?${toQuery(q)}`);

export const getClient = (id: string) => send<ClientDetail>(`${base}/${id}`);

export const createClient = (form: ClientForm) =>
    send<ClientDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updateClient = (id: string, form: ClientForm) =>
    send<ClientDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deleteClient = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
