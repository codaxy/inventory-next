import type { Page } from "../paging";
import type { AssetRow, Section } from "./assets";
import { send, toQuery } from "./http";

export interface ManufacturerItem {
    id: string;
    name: string;
    url: string | null;
    devices: number;
    software: number;
}

export interface ManufacturerForm {
    name: string;
    url: string | null;
}

export interface ManufacturerDetail extends ManufacturerForm {
    id: string;
    devices: Section<AssetRow>;
    software: Section<{ id: string; name: string; category: string | null }>;
}

export type ManufacturerSort = `${"" | "-"}${"name" | "devices" | "software"}`;

const base = "/api/company/manufacturers";

export const listManufacturers = (q: {
    q?: string;
    sort?: ManufacturerSort;
    page: number;
    pageSize: number;
}) => send<Page<ManufacturerItem>>(`${base}/?${toQuery(q)}`);
export const getManufacturer = (id: string) => send<ManufacturerDetail>(`${base}/${id}`);
export const createManufacturer = (form: ManufacturerForm) =>
    send<ManufacturerDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });
export const updateManufacturer = (id: string, form: ManufacturerForm) =>
    send<ManufacturerDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });
export const deleteManufacturer = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
