import { createModel } from "cx/ui";

import type { ManufacturerItem, ManufacturerSort } from "../../../api/manufacturers";
import { counted } from "../../../components/searchList";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    url?: string;
    devices?: string;
    devicesWord: string;
    software?: string;
    softwareWord: string;
}

/** Searched, not filtered; the list keeps the shape every list does. */
export type Filters = Record<string, never>;

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: { key: string; text: string }[];
    sort: ManufacturerSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    /** The search that ran is one id: an empty answer then says no record has it. */
    idSearch?: boolean;
    exportHref?: string;
}

export interface Model {
    list: ListState;
    $row: Row;
}

export default createModel<Model>();

export const toRows = (items: ManufacturerItem[]): Row[] =>
    items.map((m) => {
        const devices = counted(m.devices, "device", "devices");
        const software = counted(m.software, "software or service", "software and services");
        return {
            id: m.id,
            name: m.name,
            url: m.url || undefined,
            devices: devices.value,
            devicesWord: devices.word,
            software: software.value,
            softwareWord: software.word,
        };
    });
