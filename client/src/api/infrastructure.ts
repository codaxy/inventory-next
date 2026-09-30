import type { Page } from "../paging";
import type { Option } from "./assets";
import { send, toQuery } from "./http";

/** A volume as it is named — its license and the license's number, then what tells it apart. */
export interface VolumeName {
    id: string;
    license: string;
    licenseNumber: number | null;
    designator: string;
}

/** A volume, and the license it is under. */
export interface VolumeRef extends VolumeName {
    licenseId: string;
}

interface Information {
    total: number;
    items: { id: string; name: string; type: string | null }[];
}

export interface MachineItem {
    id: string;
    name: string;
    ipAddress: string | null;
    information: number;
}

export interface MachineDetail {
    id: string;
    name: string;
    ipAddress: string | null;
    information: Information;
}

export interface MachineForm {
    name: string;
    ipAddress: string | null;
}

/** A cloud subscription or a software entry: bought under a volume. */
export interface OnVolumeItem {
    id: string;
    name: string;
    license: string;
    software: string;
    information: number;
}

export interface OnVolumeDetail {
    id: string;
    name: string;
    managementUrl?: string | null;
    volume: VolumeRef;
    information: Information;
}

export interface OnVolumeForm {
    name: string;
    volumeId: string | null;
    managementUrl?: string | null;
}

export type Sort = `${"" | "-"}${"name" | "information" | "license"}`;

const records = <Item, Detail, Form>(base: string) => ({
    list: (q: { q?: string; sort?: Sort; page: number; pageSize: number }) =>
        send<Page<Item>>(`${base}/?${toQuery(q)}`),
    /** The spreadsheet of what a list query selects, every row. */
    export: (q: { q?: string; sort?: Sort }) => `${base}/export?${toQuery(q)}`,
    get: (id: string) => send<Detail>(`${base}/${id}`),
    options: () => send<{ volumes: VolumeName[] }>(`${base}/options`),
    create: (form: Form) => send<Detail>(`${base}/`, { method: "POST", body: JSON.stringify(form) }),
    update: (id: string, form: Form) =>
        send<Detail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) }),
    remove: (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" }),
});

export const virtualMachines = records<MachineItem, MachineDetail, MachineForm>(
    "/api/infrastructure/virtual-machines",
);
export const cloudSubscriptions = records<OnVolumeItem, OnVolumeDetail, OnVolumeForm>(
    "/api/infrastructure/cloud-subscriptions",
);
export const software = records<OnVolumeItem, OnVolumeDetail, OnVolumeForm>("/api/infrastructure/software");
