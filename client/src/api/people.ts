import type { Page } from "../paging";
import type { Expiry } from "./activations";
import { send, toQuery } from "./http";

export interface PersonItem {
    id: string;
    name: string;
    email: string;
    /** Every asset they hold: devices, furniture, licences. */
    assets: number;
    /** Active seats assigned to them by name. */
    seats: number;
}

export interface PersonDetail {
    id: string;
    name: string;
    email: string;
}

export interface PersonForm {
    name: string;
    email: string;
}

export type PersonSort = `${"" | "-"}${"name" | "email" | "assets"}`;

export interface PersonQuery {
    q?: string;
    sort?: PersonSort;
    page: number;
    pageSize: number;
}

export interface Section<T> {
    total: number;
    items: T[];
}

export interface AssetRow {
    id: string;
    number: number | null;
    name: string;
    type: string | null;
    model: string | null;
}

export interface LicenseRow {
    id: string;
    number: number | null;
    name: string;
    vendor: string;
    expirationDate: string | null;
    expiry: Expiry | null;
}

export interface SeatRow {
    id: string;
    software: string;
    licenseId: string;
    license: string;
    quantity: number;
    activationDate: string;
    deactivationDate: string | null;
    /** The device the seat is on, where it is on one they hold rather than theirs by name. */
    device: string | null;
    deviceNumber: number | null;
}

export interface Holdings {
    devices: Section<AssetRow>;
    furniture: Section<AssetRow>;
    licenses: Section<LicenseRow>;
    /** `total` counts the deactivated too. */
    seats: Section<SeatRow> & { active: number };
    information: Section<{ id: string; name: string; type: string | null }>;
    projects: Section<{ id: string; name: string; client: string | null }>;
}

export interface Handover {
    name: string;
    assets: { number: number | null; name: string; description: string | null; type: string }[];
}

const base = "/api/directory/people";

export const listPeople = (q: PersonQuery) => send<Page<PersonItem>>(`${base}/?${toQuery(q)}`);

export const getPerson = (id: string) => send<PersonDetail>(`${base}/${id}`);

export const getHoldings = (id: string) => send<Holdings>(`${base}/${id}/holdings`);

export const getHandover = (id: string) => send<Handover>(`${base}/${id}/handover`);

export const createPerson = (form: PersonForm) =>
    send<PersonDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updatePerson = (id: string, form: PersonForm) =>
    send<PersonDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deletePerson = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
