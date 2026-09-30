import type { Page } from "../paging";
import type { AssetForm, AssetOptions, Option, Ref } from "./assets";
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
            licenseNumber: number | null;
            person: string | null;
            quantity: number;
            activationDate: string;
            deactivationDate: string | null;
        }[];
    };
    information: { total: number; items: { id: string; name: string; type: string | null }[] };
}

export interface DeviceOptions extends AssetOptions {
    types: Option[];
    tags: Option[];
    manufacturers: Option[];
    /** Each type's tags, by the type's id: what a device of it shows. */
    typeTags: Record<string, Option[]>;
}

/** An asset's fields and the device's own, as the server takes them; "warranty" is the entity's "guarantee". */
export interface DeviceForm extends AssetForm {
    typeId: string | null;
    manufacturerId: string | null;
    manufacturingDate: string | null;
    modelName: string | null;
    modelCode: string | null;
    serialNumber: string | null;
    warrantyNumber: string | null;
    warrantyExpirationDate: string | null;
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
export const createDevice = (form: DeviceForm) =>
    send<DeviceDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });
export const updateDevice = (id: string, form: DeviceForm) =>
    send<DeviceDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });
export const deleteDevice = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
