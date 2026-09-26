import type { Page } from "../paging";
import type { AssetDetail, AssetForm, AssetOptions, Option, Ref } from "./assets";
import { send, toQuery } from "./http";

export interface FurnitureItem {
    id: string;
    number: number | null;
    name: string;
    incomplete: boolean;
    assignee: string;
    location: string | null;
    type: string | null;
    vendor: string;
    purchaseValue: number;
    lastModified: string;
}

export interface FurnitureDetail extends AssetDetail {
    type: Ref | null;
    model: string | null;
}

export interface FurnitureForm extends AssetForm {
    typeId: string | null;
    model: string | null;
}

export interface FurnitureOptions extends AssetOptions {
    types: Option[];
}

export type FurnitureSort =
    `${"" | "-"}${"number" | "name" | "assignee" | "location" | "type" | "vendor" | "value" | "modified"}`;

export interface FurnitureQuery {
    q?: string;
    typeId?: string;
    vendorId?: string;
    personId?: string;
    locationId?: string;
    purchasedFrom?: string;
    /** Exclusive. */
    purchasedTo?: string;
    incomplete?: boolean;
    sort?: FurnitureSort;
    page: number;
    pageSize: number;
}

const base = "/api/furniture";

export const listFurniture = (q: FurnitureQuery) => send<Page<FurnitureItem>>(`${base}/?${toQuery(q)}`);

/** The spreadsheet of what a list query selects, every row. */
export const furnitureExport = (q: Omit<FurnitureQuery, "page" | "pageSize">) =>
    `${base}/export?${toQuery(q)}`;

export const getFurniture = (id: string) => send<FurnitureDetail>(`${base}/${id}`);

export const getFurnitureOptions = () => send<FurnitureOptions>(`${base}/options`);

export const createFurniture = (form: FurnitureForm) =>
    send<FurnitureDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updateFurniture = (id: string, form: FurnitureForm) =>
    send<FurnitureDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deleteFurniture = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
