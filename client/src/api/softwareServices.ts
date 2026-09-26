import type { Page } from "../paging";
import { send, toQuery } from "./http";

export interface Ref {
    id: string;
    name: string;
}

export interface SoftwareServiceItem {
    id: string;
    name: string;
    category: string;
    manufacturer: string;
    url: string | null;
    volumeCount: number;
}

export interface SoftwareServiceDetail {
    id: string;
    name: string;
    category: Ref;
    manufacturer: Ref;
    url: string | null;
    volumeCount: number;
}

export interface SoftwareServiceForm {
    name: string;
    categoryId: string | null;
    manufacturerId: string | null;
    url: string | null;
}

export type SoftwareServiceSort =
    "name" | "-name" | "category" | "-category" | "manufacturer" | "-manufacturer" | "volumes" | "-volumes";

export interface SoftwareServiceQuery {
    q?: string;
    categoryId?: string;
    manufacturerId?: string;
    sort?: SoftwareServiceSort;
    page: number;
    pageSize: number;
}

export interface Option {
    id: string;
    text: string;
}

const base = "/api/licenses/software-services";

export const listSoftwareServices = (q: SoftwareServiceQuery) =>
    send<Page<SoftwareServiceItem>>(`${base}/?${toQuery(q)}`);

export const getSoftwareService = (id: string) => send<SoftwareServiceDetail>(`${base}/${id}`);

export const getSoftwareServiceOptions = () =>
    send<{ categories: Option[]; manufacturers: Option[] }>(`${base}/options`);

export const createSoftwareService = (form: SoftwareServiceForm) =>
    send<SoftwareServiceDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updateSoftwareService = (id: string, form: SoftwareServiceForm) =>
    send<SoftwareServiceDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deleteSoftwareService = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
