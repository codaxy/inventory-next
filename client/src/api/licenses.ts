import type { Page } from "../paging";
import type { Expiry } from "./activations";
import type { AssetDetail, AssetForm, AssetOptions, Option, Ref } from "./assets";
import { send, toQuery } from "./http";

export type { Option, Ref, Weighted } from "./assets";

export interface LicenseItem {
    id: string;
    number: number | null;
    name: string;
    incomplete: boolean;
    vendor: string;
    purchaseValue: number;
    purchaseDate: string;
    expirationDate: string | null;
    expiry: Expiry | null;
    lastModified: string;
}

export interface VolumeDetail {
    id: string;
    software: Ref;
    type: { id: number; name: string };
    quantity: number;
    description: string | null;
    inUse: number;
    activationCount: number;
    /** Why it cannot be removed — activations, a cloud or software on it — or null. */
    held: string | null;
}

export interface LicenseDetail extends AssetDetail {
    licenseType: Ref | null;
    licenseModel: Ref | null;
    expirationModel: Ref | null;
    expirationDate: string | null;
    expiry: Expiry | null;
    subscriptionFee: number | null;
    currency: Ref | null;
    period: Ref | null;
    autoRenew: boolean;
    managementConsoleUrl: string | null;
    registrationNumber: string | null;
    keyIdentifier: string | null;
    volumes: VolumeDetail[];
}

export interface VolumeForm {
    id?: string;
    softwareOrServiceId?: string | null;
    volumeTypeId?: number | null;
    quantity?: number | null;
    description?: string | null;
}

export interface LicenseForm extends AssetForm {
    licenseTypeId: string | null;
    licenseModelId: string | null;
    expirationModelId: string | null;
    expirationDate: string | null;
    subscriptionFee: number | null;
    currencyId: string | null;
    periodId: string | null;
    autoRenew: boolean;
    managementConsoleUrl: string | null;
    registrationNumber: string | null;
    keyIdentifier: string | null;
    volumes: VolumeForm[];
}

export type LicenseSort =
    `${"" | "-"}${"number" | "name" | "vendor" | "value" | "purchased" | "expires" | "modified"}`;

export interface LicenseQuery {
    q?: string;
    vendorId?: string;
    purchasedFrom?: string;
    purchasedTo?: string;
    expiry?: Expiry | "none";
    incomplete?: boolean;
    sort?: LicenseSort;
    page: number;
    pageSize: number;
}

export interface LicenseOptions extends AssetOptions {
    importances: Option[];
    licenseTypes: Option[];
    licenseModels: Option[];
    expirationModels: Option[];
    currencies: Option[];
    periods: Option[];
    software: Option[];
    volumeTypes: { id: number; text: string }[];
}

const base = "/api/licenses";

export const listLicenses = (q: LicenseQuery) => send<Page<LicenseItem>>(`${base}/?${toQuery(q)}`);

/** The spreadsheet of what a list query selects, every row. */
export const licensesExport = (q: Omit<LicenseQuery, "page" | "pageSize">) => `${base}/export?${toQuery(q)}`;

export const getLicense = (id: string) => send<LicenseDetail>(`${base}/${id}`);

export const getLicenseOptions = () => send<LicenseOptions>(`${base}/options`);

export const createLicense = (form: LicenseForm) =>
    send<LicenseDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updateLicense = (id: string, form: LicenseForm) =>
    send<LicenseDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deleteLicense = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
