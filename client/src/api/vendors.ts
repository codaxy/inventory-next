import type { Page } from "../paging";
import type { AssetSections, Section } from "./assets";
import { send, toQuery } from "./http";

export interface VendorItem {
    id: string;
    name: string;
    contactPerson: string | null;
    email: string | null;
    /** The phone, or the mobile where there is none. */
    phone: string | null;
    assets: number;
}

export interface VendorFields {
    name: string;
    location: string | null;
    registrationNumber: string | null;
    vatNumber: string | null;
    web: string | null;
    contactPerson: string | null;
    mobilePhone: string | null;
    phone: string | null;
    email: string | null;
}

export interface VendorDetail extends VendorFields {
    id: string;
    assets: AssetSections;
    contracts: Section<{
        id: string;
        assetId: string | null;
        asset: string | null;
        assetNumber: number | null;
        contractNumber: string | null;
        expirationDate: string | null;
    }>;
}

export type VendorSort = `${"" | "-"}${"name" | "assets"}`;

const base = "/api/company/vendors";

export const listVendors = (q: { q?: string; sort?: VendorSort; page: number; pageSize: number }) =>
    send<Page<VendorItem>>(`${base}/?${toQuery(q)}`);
export const getVendor = (id: string) => send<VendorDetail>(`${base}/${id}`);
export const createVendor = (form: VendorFields) =>
    send<VendorDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });
export const updateVendor = (id: string, form: VendorFields) =>
    send<VendorDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });
export const deleteVendor = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
