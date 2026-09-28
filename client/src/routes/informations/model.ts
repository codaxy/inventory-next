import { createModel } from "cx/ui";

import type { Option } from "../../api/assets";
import type { InformationItem, InformationSort } from "../../api/informations";
import type { PagerState } from "../../paging";

export interface Row {
    id: string;
    name: string;
    /** Present only where the record is marked incomplete: a flag, not a column. */
    incomplete?: string;
    type: string;
    assignee: string;
    author?: string;
    project?: string;
}

export interface Filters {
    typeId?: string | null;
    typeText?: string;
    personId?: string | null;
    personText?: string;
    projectId?: string | null;
    projectText?: string;
    tagId?: string | null;
    tagText?: string;
    locationId?: string | null;
    locationText?: string;
    virtualMachineId?: string | null;
    virtualMachineText?: string;
    cloudSubscriptionId?: string | null;
    cloudSubscriptionText?: string;
    softwareId?: string | null;
    softwareText?: string;
    incomplete?: boolean | null;
}

export type FilterKey =
    | "type"
    | "person"
    | "project"
    | "tag"
    | "location"
    | "virtualMachine"
    | "cloudSubscription"
    | "software"
    | "incomplete";

export interface Chip {
    key: FilterKey;
    text: string;
}

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: Chip[];
    types: Option[];
    people: Option[];
    projects: Option[];
    tags: Option[];
    locations: Option[];
    sort: InformationSort;
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

export const toRows = (items: InformationItem[]): Row[] =>
    items.map((i) => ({
        id: i.id,
        name: i.name,
        incomplete: i.incomplete ? "Incomplete" : undefined,
        type: i.type,
        assignee: i.assignee,
        author: i.author || undefined,
        project: i.project ?? undefined,
    }));

const pickChip = (key: FilterKey, label: string, id?: string | null, text?: string): Chip[] =>
    id ? [{ key, text: `${label}: ${text ?? "…"}` }] : [];

export const toChips = (f: Filters): Chip[] => [
    ...pickChip("type", "Type", f.typeId, f.typeText),
    ...pickChip("person", "Assignee", f.personId, f.personText),
    ...pickChip("project", "Project", f.projectId, f.projectText),
    ...pickChip("tag", "Tag", f.tagId, f.tagText),
    ...pickChip("location", "Kept at", f.locationId, f.locationText),
    ...pickChip("virtualMachine", "Kept on", f.virtualMachineId, f.virtualMachineText),
    ...pickChip("cloudSubscription", "Kept on", f.cloudSubscriptionId, f.cloudSubscriptionText),
    ...pickChip("software", "Kept on", f.softwareId, f.softwareText),
    ...(f.incomplete == null
        ? []
        : [{ key: "incomplete" as const, text: f.incomplete ? "Incomplete" : "Complete" }]),
];
