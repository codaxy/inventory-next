import { createModel } from "cx/ui";

import type { Option } from "../../../api/assets";
import type { ProjectItem, ProjectSort } from "../../../api/projects";
import { counted } from "../../../components/searchList";
import type { PagerState } from "../../../paging";

export interface Row {
    id: string;
    name: string;
    client: string;
    owner: string;
    information?: string;
    informationWord: string;
}

export interface Filters {
    clientId?: string | null;
    clientText?: string;
    personId?: string | null;
    personText?: string;
}

export type FilterKey = "client" | "person";

export interface Chip {
    key: FilterKey;
    text: string;
}

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    chips: Chip[];
    clients: Option[];
    people: Option[];
    sort: ProjectSort;
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
    $chip: Chip;
}

export default createModel<Model>();

export const toRows = (items: ProjectItem[]): Row[] =>
    items.map((p) => {
        const information = counted(p.information, "piece of information", "pieces of information");
        return {
            id: p.id,
            name: p.name,
            client: p.client,
            owner: p.owner,
            information: information.value,
            informationWord: information.word,
        };
    });

export const toChips = (f: Filters): Chip[] => [
    ...(f.clientId ? [{ key: "client" as const, text: `Client: ${f.clientText ?? "…"}` }] : []),
    ...(f.personId ? [{ key: "person" as const, text: `Led by: ${f.personText ?? "…"}` }] : []),
];
