import type { Page } from "../paging";
import { send, toQuery } from "./http";

export interface FurnitureTypeItem {
    id: string;
    name: string;
    description: string | null;
    furnitureCount: number;
}

export interface FurnitureTypeDetail extends FurnitureTypeItem {}

export interface FurnitureTypeForm {
    name: string;
    description: string | null;
}

export type FurnitureTypeSort = `${"" | "-"}${"name" | "furniture"}`;

export interface FurnitureTypeQuery {
    q?: string;
    sort?: FurnitureTypeSort;
    page: number;
    pageSize: number;
}

const base = "/api/furniture/types";

export const listFurnitureTypes = (q: FurnitureTypeQuery) =>
    send<Page<FurnitureTypeItem>>(`${base}/?${toQuery(q)}`);

export const getFurnitureType = (id: string) => send<FurnitureTypeDetail>(`${base}/${id}`);

export const createFurnitureType = (form: FurnitureTypeForm) =>
    send<FurnitureTypeDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updateFurnitureType = (id: string, form: FurnitureTypeForm) =>
    send<FurnitureTypeDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deleteFurnitureType = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
