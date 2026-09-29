import type { Page } from "../paging";
import type { AssetSections, Option, Ref, Section } from "./assets";
import { send, toQuery } from "./http";

export interface LocationItem {
    id: string;
    name: string;
    street: string | null;
    houseNumber: number | null;
    city: string;
    room: string | null;
    assets: number;
}

export interface LocationForm {
    name: string;
    description: string | null;
    countryCode: string | null;
    stateId: string | null;
    cityId: string | null;
    postalCode: string | null;
    street: string | null;
    houseNumber: number | null;
    floor: number | null;
    room: string | null;
}

export interface LocationDetail {
    id: string;
    name: string;
    description: string | null;
    country: { code: string; name: string };
    state: Ref | null;
    city: Ref;
    postalCode: string | null;
    street: string | null;
    houseNumber: number | null;
    floor: number | null;
    room: string | null;
    assets: AssetSections;
    information: Section<{ id: string; name: string; type: string | null }>;
}

/** A city or a state, and the country it is in: what the form narrows the list by. */
export interface InCountry extends Option {
    countryCode: string;
}

export interface LocationOptions {
    countries: Option[];
    cities: InCountry[];
    states: InCountry[];
}

export type LocationSort = `${"" | "-"}${"name" | "city" | "assets"}`;

const base = "/api/company/locations";

export const listLocations = (q: { q?: string; sort?: LocationSort; page: number; pageSize: number }) =>
    send<Page<LocationItem>>(`${base}/?${toQuery(q)}`);

/** The spreadsheet of what a list query selects, every row. */
export const locationsExport = (q: { q?: string; sort?: LocationSort }) => `${base}/export?${toQuery(q)}`;

export const getLocation = (id: string) => send<LocationDetail>(`${base}/${id}`);
export const getLocationOptions = () => send<LocationOptions>(`${base}/options`);
export const createLocation = (form: LocationForm) =>
    send<LocationDetail>(`${base}/`, { method: "POST", body: JSON.stringify(form) });
export const updateLocation = (id: string, form: LocationForm) =>
    send<LocationDetail>(`${base}/${id}`, { method: "PUT", body: JSON.stringify(form) });
export const deleteLocation = (id: string) => send<void>(`${base}/${id}`, { method: "DELETE" });
