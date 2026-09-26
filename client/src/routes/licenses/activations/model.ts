import { createModel } from "cx/ui";

import type { ActivationItem, ActivationSort, Expiry, Option } from "../../../api/activations";
import { expiryText, formatDate } from "../../../licensing";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    software: string;
    license: string;
    assignee: string;
    /** "Device · #100893" under the name, for a device. */
    assigneeNote?: string;
    seats: string;
    activated: string;
    deactivated?: string;
    ended: boolean;
    expiry?: Expiry;
    expiryText?: string;
}

/** A picked option is its id and its text, as a single `LookupField` binds them. */
export interface Filters {
    softwareId?: string | null;
    softwareText?: string;
    licenseId?: string | null;
    licenseText?: string;
    status?: "active" | "deactivated" | null;
    expiry?: Expiry | "none" | null;
}

export type FilterKey = "software" | "license" | "status" | "expiry";

export interface Chip {
    key: FilterKey;
    text: string;
}

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: Chip[];
    software: Option[];
    licenses: Option[];
    sort: ActivationSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    totalText: string;
}

export interface Model {
    list: ListState;
    $row: Row;
    $chip: Chip;
}

export default createModel<Model>();

export const toRows = (items: ActivationItem[]): Row[] =>
    items.map((a) => ({
        id: a.id,
        software: a.software,
        license: a.license,
        assignee: a.assignee ?? "—",
        assigneeNote: a.forDevice ? (a.deviceNumber ? `Device · #${a.deviceNumber}` : "Device") : undefined,
        seats: a.quantity === 1 ? "1 seat" : `${a.quantity} seats`,
        activated: formatDate(a.activationDate)!,
        deactivated: formatDate(a.deactivationDate),
        ended: !!a.deactivationDate,
        expiry: a.expiry ?? undefined,
        expiryText: a.expiry ? `${expiryText[a.expiry]} · ${formatDate(a.expirationDate)}` : undefined,
    }));

const statusText = { active: "Active", deactivated: "Deactivated" } as const;
const expiryFilterText = { ...expiryText, none: "No expiry date" } as const;

export const toChips = (f: Filters): Chip[] => [
    ...(f.softwareId ? [{ key: "software" as const, text: f.softwareText ?? "Software" }] : []),
    ...(f.licenseId ? [{ key: "license" as const, text: `Licence: ${f.licenseText ?? "…"}` }] : []),
    ...(f.status ? [{ key: "status" as const, text: statusText[f.status] }] : []),
    ...(f.expiry ? [{ key: "expiry" as const, text: `Licence: ${expiryFilterText[f.expiry]}` }] : []),
];
