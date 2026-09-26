import type { Page } from "../paging";
import type { Expiry } from "./activations";
import { send, toQuery } from "./http";

export interface Ref {
    id: string;
    name: string;
}

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

export interface LicenseDetail {
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
    licenseType: Ref | null;
    licenseModel: Ref | null;
    expirationModel: Ref | null;
    expirationDate: string | null;
    expiry: Expiry | null;
    subscriptionFee: number | null;
    currency: Ref | null;
    period: Ref | null;
    autoRenew: boolean;
    businessEntity: Ref | null;
    managementConsoleUrl: string | null;
    registrationNumber: string | null;
    keyIdentifier: string | null;
    location: Ref | null;
    url: string | null;
    lastModified: string;
    volumes: VolumeDetail[];
}

export interface VolumeForm {
    id?: string;
    softwareOrServiceId?: string | null;
    volumeTypeId?: number | null;
    quantity?: number | null;
    description?: string | null;
}

export interface LicenseForm {
    name: string;
    invoiceNumber: string | null;
    vendorId: string | null;
    purchaseValue: number | null;
    purchaseDate: string | null;
    description: string | null;
    personId: string | null;
    confidentialityId: string | null;
    integrityId: string | null;
    availabilityId: string | null;
    incomplete: boolean;
    licenseTypeId: string | null;
    licenseModelId: string | null;
    expirationModelId: string | null;
    expirationDate: string | null;
    subscriptionFee: number | null;
    currencyId: string | null;
    periodId: string | null;
    autoRenew: boolean;
    businessEntityId: string | null;
    managementConsoleUrl: string | null;
    registrationNumber: string | null;
    keyIdentifier: string | null;
    locationId: string | null;
    url: string | null;
    volumes: VolumeForm[];
    lastModified?: string;
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

export interface Option {
    id: string;
    text: string;
}

export interface Weighted extends Option {
    weight: number;
}

export interface LicenseOptions {
    vendors: Option[];
    people: Option[];
    confidentialities: Weighted[];
    integrities: Weighted[];
    availabilities: Weighted[];
    importances: Option[];
    licenseTypes: Option[];
    licenseModels: Option[];
    expirationModels: Option[];
    currencies: Option[];
    periods: Option[];
    businessEntities: Option[];
    locations: Option[];
    software: Option[];
    volumeTypes: { id: number; text: string }[];
}

const base = "/api/licenses";

export const listLicenses = (q: LicenseQuery) => send<Page<LicenseItem>>(`${base}/?${toQuery(q)}`);

export const getLicense = (id: string) => send<LicenseDetail>(`${base}/${id}`);

export const getLicenseOptions = () => send<LicenseOptions>(`${base}/options`);

export const createLicense = (form: LicenseForm) =>
    send<LicenseDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updateLicense = (id: string, form: LicenseForm) =>
    send<LicenseDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deleteLicense = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
