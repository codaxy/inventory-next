import type { Page } from "../paging";
import { send, toQuery } from "./http";

export type Expiry = "expired" | "soon" | "regular";

export interface Named {
    id: string;
    name: string;
}

export interface ActivationItem {
    id: string;
    software: string;
    licenseId: string;
    license: string;
    assignee: string | null;
    deviceNumber: number | null;
    forDevice: boolean;
    quantity: number;
    activationDate: string;
    deactivationDate: string | null;
    expirationDate: string | null;
    expiry: Expiry | null;
}

export interface ActivationDetail {
    id: string;
    software: Named;
    license: {
        id: string;
        number: number | null;
        name: string;
        vendor: string | null;
        type: string | null;
        model: string | null;
        expirationModel: string | null;
        expirationDate: string | null;
        expiry: Expiry | null;
        location: string | null;
        url: string | null;
    };
    volume: { id: string; type: string; typeId: number; quantity: number; inUse: number };
    person: Named | null;
    device: { id: string; name: string; number: number | null } | null;
    quantity: number;
    activationDate: string;
    deactivationDate: string | null;
}

export interface ActivationForm {
    volumeId: string | null;
    personId: string | null;
    deviceId: string | null;
    activationDate: string | null;
    quantity: number;
}

export type ActivationSort =
    | "activated"
    | "-activated"
    | "software"
    | "-software"
    | "license"
    | "-license"
    | "assignee"
    | "-assignee"
    | "deactivated"
    | "-deactivated";

export interface ActivationQuery {
    q?: string;
    softwareId?: string;
    licenseId?: string;
    volumeId?: string;
    /** Theirs by name, and those on a device they hold. */
    personId?: string;
    status?: "active" | "deactivated";
    expiry?: Expiry | "none";
    sort?: ActivationSort;
    page: number;
    pageSize: number;
}

export interface Option {
    id: string;
    text: string;
}

export interface DeviceOption extends Option {
    number: number | null;
    holder: string | null;
}

/** A volume for a filter's chip and a form's preselection: what tells it apart, and its software. */
export interface VolumeRef {
    id: string;
    text: string;
    softwareId: string;
    software: string;
    licenseId: string;
}

export interface VolumeOption {
    id: string;
    licenseId: string;
    license: string;
    licenseNumber: number | null;
    type: string;
    typeId: number;
    quantity: number;
    inUse: number;
    /** What tells volumes of one licence and software apart. */
    description: string | null;
}

/** The seeded id of the per-user volume type: the one activated for a person. */
export const perUser = 1;

const base = "/api/licenses/activations";

export const listActivations = (q: ActivationQuery) => send<Page<ActivationItem>>(`${base}/?${toQuery(q)}`);

/** The spreadsheet of what a list query selects, every row. */
export const activationsExport = (q: Omit<ActivationQuery, "page" | "pageSize">) =>
    `${base}/export?${toQuery(q)}`;

export const getActivation = (id: string) => send<ActivationDetail>(`${base}/${id}`);

export const getActivationOptions = () =>
    send<{
        software: Option[];
        licenses: Option[];
        people: Option[];
        devices: DeviceOption[];
        volumes: VolumeRef[];
    }>(`${base}/options`);

export const getVolumes = (softwareId: string) =>
    send<VolumeOption[]>(`${base}/volumes?${toQuery({ softwareId })}`);

export const createActivation = (form: ActivationForm) =>
    send<ActivationDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });

export const deactivateActivation = (id: string, date: string) =>
    send<ActivationDetail>(`${base}/${id}/deactivate`, { method: "POST", body: JSON.stringify({ date }) });

export const reactivateActivation = (id: string) =>
    send<ActivationDetail>(`${base}/${id}/reactivate`, { method: "POST" });

export const deleteActivation = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
