import type { Page } from "../paging";
import type { AssetRow, LicenseRow, Section } from "./assets";
import { send, toQuery } from "./http";

export interface PersonItem {
    id: string;
    name: string;
    email: string;
    /** Every asset they hold: devices, furniture, licenses. */
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

export interface SeatRow {
    id: string;
    software: string;
    licenseId: string;
    license: string;
    licenseNumber: number | null;
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
    /** Where it is signed; empty leaves the line for a hand. */
    place: string;
    /** Who produced it: the signed-in person. */
    controller: string;
    /** Whether the server can print it. */
    pdf: boolean;
    assets: { number: number | null; name: string; description: string | null; type: string }[];
    /** Active seats, theirs by name or on a device they hold. */
    seats: {
        software: string;
        license: string;
        licenseNumber: number | null;
        /** The device it is on, for a seat not theirs by name. */
        device: string | null;
        deviceNumber: number | null;
        /** `YYYY-MM-DD`, the license's subscription end. */
        expires: string | null;
    }[];
}

const base = "/api/company/people";

export const listPeople = (q: PersonQuery) => send<Page<PersonItem>>(`${base}/?${toQuery(q)}`);

/** The spreadsheet of what a list query selects, every row. */
export const peopleExport = (q: Omit<PersonQuery, "page" | "pageSize">) => `${base}/export?${toQuery(q)}`;

export const getPerson = (id: string) => send<PersonDetail>(`${base}/${id}`);

export const getHoldings = (id: string) => send<Holdings>(`${base}/${id}/holdings`);

export const getHandover = (id: string) => send<Handover>(`${base}/${id}/handover`);

/** The handover sheet as the server prints it, dated in the browser's time zone. */
export const handoverPdf = (id: string) =>
    `${base}/${id}/handover.pdf?tz=${encodeURIComponent(Intl.DateTimeFormat().resolvedOptions().timeZone)}`;

export const createPerson = (form: PersonForm) =>
    send<PersonDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const updatePerson = (id: string, form: PersonForm) =>
    send<PersonDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });

export const deletePerson = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
