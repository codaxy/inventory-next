import type { Page } from "../paging";
import type { Option, Ref, Weighted } from "./assets";
import { send, toQuery } from "./http";

export interface InformationRow {
    id: string;
    name: string;
    type: string | null;
}

/** A type or a tag: the same shape, the information of it beneath. */
export interface GroupItem {
    id: string;
    name: string;
    description: string | null;
    information: number;
}

export interface GroupDetail {
    id: string;
    name: string;
    description: string | null;
    information: { total: number; items: InformationRow[] };
}

export interface GroupForm {
    name: string;
    description: string | null;
}

export type GroupSort = `${"" | "-"}${"name" | "information"}`;

const groups = (base: string) => ({
    list: (q: { q?: string; sort?: GroupSort; page: number; pageSize: number }) =>
        send<Page<GroupItem>>(`${base}/?${toQuery(q)}`),
    get: (id: string) => send<GroupDetail>(`${base}/${id}`),
    create: (form: GroupForm) =>
        send<GroupDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) }),
    update: (id: string, form: GroupForm) =>
        send<GroupDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) }),
    remove: (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" }),
});

export const informationTypes = groups("/api/informations/types");
export const informationTags = groups("/api/informations/tags");

export type LocationKind =
    "device" | "virtualMachine" | "software" | "cloudSubscription" | "location" | "url";

export interface InformationItem {
    id: string;
    name: string;
    incomplete: boolean;
    type: string;
    assignee: string;
    author: string | null;
    project: string | null;
}

export interface LocationRow {
    id: string;
    kind: LocationKind;
    targetId: string | null;
    /** The device's, machine's, software's, cloud subscription's or place's name, or the address. */
    target: string;
    number: number | null;
}

export interface InformationDetail {
    id: string;
    name: string;
    type: Ref;
    person: Ref;
    author: string | null;
    accessRights: string | null;
    personalInformation: boolean;
    clientsPersonalInformation: boolean;
    incomplete: boolean;
    description: string | null;
    note: string | null;
    confidentiality: Ref | null;
    integrity: Ref | null;
    availability: Ref | null;
    importance: Ref | null;
    project: Ref | null;
    tags: Ref[];
    locations: LocationRow[];
}

export interface LocationForm {
    id?: string;
    kind?: LocationKind | null;
    targetId?: string | null;
    url?: string | null;
}

export interface InformationForm {
    name: string;
    typeId: string | null;
    personId: string | null;
    author: string | null;
    accessRights: string | null;
    personalInformation: boolean;
    clientsPersonalInformation: boolean;
    incomplete: boolean;
    description: string | null;
    note: string | null;
    confidentialityId: string | null;
    integrityId: string | null;
    availabilityId: string | null;
    projectId: string | null;
    tagIds: string[];
    locations: LocationForm[];
}

export interface InformationOptions {
    types: Option[];
    people: Option[];
    projects: Option[];
    tags: Option[];
    confidentialities: Weighted[];
    integrities: Weighted[];
    availabilities: Weighted[];
    devices: Option[];
    virtualMachines: Option[];
    software: Option[];
    cloudSubscriptions: Option[];
    locations: Option[];
}

export type InformationSort = `${"" | "-"}${"name" | "type" | "assignee" | "author" | "project"}`;

export interface InformationQuery {
    q?: string;
    typeId?: string;
    personId?: string;
    projectId?: string;
    tagId?: string;
    locationId?: string;
    incomplete?: boolean;
    sort?: InformationSort;
    page: number;
    pageSize: number;
}

const base = "/api/informations";

export const listInformation = (q: InformationQuery) => send<Page<InformationItem>>(`${base}/?${toQuery(q)}`);
export const informationExport = (q: Omit<InformationQuery, "page" | "pageSize">) =>
    `${base}/export?${toQuery(q)}`;
export const getInformation = (id: string) => send<InformationDetail>(`${base}/${id}`);
export const getInformationOptions = () => send<InformationOptions>(`${base}/options`);
export const createInformation = (form: InformationForm) =>
    send<InformationDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });
export const updateInformation = (id: string, form: InformationForm) =>
    send<InformationDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });
export const deleteInformation = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
