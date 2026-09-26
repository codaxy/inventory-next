import type { Page } from "../paging";
import type { Option, Ref } from "./assets";
import { send, toQuery } from "./http";

export interface DeviceItem {
    id: string;
    number: number | null;
    name: string;
    incomplete: boolean;
    modelName: string | null;
    assignee: string;
    location: string | null;
    type: string | null;
    manufacturer: string | null;
    modelCode: string | null;
    serialNumber: string | null;
    lastModified: string;
}

export interface ContractRow {
    id: string;
    vendor: Ref;
    /** "AdHoc" or "Contract". */
    type: string | null;
    serviceDueDate: string | null;
    expirationDate: string | null;
    contactName: string | null;
    contactNumber: string | null;
    contactEmail: string | null;
    contractNumber: string | null;
    description: string | null;
}

export interface DeviceDetail {
    id: string;
    number: number | null;
    name: string;
    invoiceNumber: string | null;
    vendor: Ref;
    purchaseValue: number;
    purchaseDate: string;
    description: string | null;
    person: Ref;
    confidentiality: Ref | null;
    integrity: Ref | null;
    availability: Ref | null;
    importance: Ref | null;
    incomplete: boolean;
    businessEntity: Ref | null;
    location: Ref | null;
    url: string | null;
    type: Ref | null;
    tags: Ref[];
    manufacturer: Ref | null;
    manufacturingDate: string | null;
    modelName: string | null;
    modelCode: string | null;
    serialNumber: string | null;
    warrantyNumber: string | null;
    warrantyExpirationDate: string | null;
    lastModified: string;
    contracts: ContractRow[];
    seats: {
        total: number;
        items: {
            id: string;
            software: string;
            licenseId: string;
            license: string;
            person: string | null;
            quantity: number;
            activationDate: string;
            deactivationDate: string | null;
        }[];
    };
    information: { total: number; items: { id: string; name: string; type: string | null }[] };
}

export interface DeviceOptions {
    types: Option[];
    tags: Option[];
    people: Option[];
    vendors: Option[];
    locations: Option[];
    manufacturers: Option[];
}

export type DeviceSort =
    `${"" | "-"}${"number" | "name" | "model" | "assignee" | "location" | "type" | "manufacturer" | "modified"}`;

export interface DeviceQuery {
    q?: string;
    typeId?: string;
    tagId?: string;
    personId?: string;
    vendorId?: string;
    locationId?: string;
    manufacturerId?: string;
    purchasedFrom?: string;
    /** Exclusive. */
    purchasedTo?: string;
    incomplete?: boolean;
    sort?: DeviceSort;
    page: number;
    pageSize: number;
}

const base = "/api/electronic-devices";

export const listDevices = (q: DeviceQuery) => send<Page<DeviceItem>>(`${base}/?${toQuery(q)}`);
export const devicesExport = (q: Omit<DeviceQuery, "page" | "pageSize">) => `${base}/export?${toQuery(q)}`;
export const getDevice = (id: string) => send<DeviceDetail>(`${base}/${id}`);
export const getDeviceOptions = () => send<DeviceOptions>(`${base}/options`);
