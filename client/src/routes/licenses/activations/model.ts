import { createModel } from "cx/ui";

import type {
    ActivationItem,
    ActivationSort,
    Expiry,
    NumberedOption,
    Option,
    VolumeRef,
} from "../../../api/activations";
import { numberText } from "../../../inventoryNumbers";
import { expiryText, formatDate } from "../../../licensing";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    software: string;
    license: string;
    assignee: string;
    /** The assignee is a device, not a person: its icon says which. */
    forDevice: boolean;
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
    /** One volume of a license: what a license page's volume links to. */
    volumeId?: string | null;
    volumeText?: string;
    /** The seats a person answers for: theirs by name, and those on a device they hold. */
    personId?: string | null;
    personText?: string;
    status?: "active" | "deactivated" | null;
    expiry?: Expiry | "none" | null;
}

export type FilterKey = "software" | "license" | "volume" | "person" | "status" | "expiry";

export interface Chip {
    key: FilterKey;
    text: string;
    /** A license's inventory number, muted after its name, and a volume's designator after that. */
    number?: string;
    rest?: string;
}

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: Chip[];
    software: Option[];
    licenses: NumberedOption[];
    volumes: VolumeRef[];
    people: Option[];
    sort: ActivationSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    /** The search that ran is one id: an empty answer then says no record has it. */
    idSearch?: boolean;
    /** The spreadsheet of what the list selects — every row, not the page. */
    exportHref?: string;
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
        forDevice: a.forDevice,
        seats: a.quantity === 1 ? "1 seat" : `${a.quantity} seats`,
        activated: formatDate(a.activationDate)!,
        deactivated: formatDate(a.deactivationDate),
        ended: !!a.deactivationDate,
        expiry: a.expiry ?? undefined,
        expiryText: a.expiry ? `${expiryText[a.expiry]} · ${formatDate(a.expirationDate)}` : undefined,
    }));

const licenseChip = (l: NumberedOption | undefined): Chip => ({
    key: "license",
    text: `License: ${l?.name ?? "…"}`,
    number: numberText(l?.number),
});

const volumeChip = (v: VolumeRef | undefined): Chip => ({
    key: "volume",
    text: `Volume: ${v?.license ?? "…"}`,
    number: numberText(v?.licenseNumber),
    rest: v && ` · ${v.designator}`,
});

const statusText = { active: "Active", deactivated: "Deactivated" } as const;
const expiryFilterText = { ...expiryText, none: "No expiry date" } as const;

/** A license's and a volume's chips name them from the options, so the number is apart from the name. */
export const toChips = (f: Filters, licenses: NumberedOption[] = [], volumes: VolumeRef[] = []): Chip[] => [
    ...(f.softwareId ? [{ key: "software" as const, text: f.softwareText ?? "Software" }] : []),
    ...(f.licenseId ? [licenseChip(licenses.find((l) => l.id === f.licenseId))] : []),
    ...(f.volumeId ? [volumeChip(volumes.find((v) => v.id === f.volumeId))] : []),
    ...(f.personId ? [{ key: "person" as const, text: `Held by: ${f.personText ?? "…"}` }] : []),
    ...(f.status ? [{ key: "status" as const, text: statusText[f.status] }] : []),
    ...(f.expiry ? [{ key: "expiry" as const, text: `License: ${expiryFilterText[f.expiry]}` }] : []),
];
