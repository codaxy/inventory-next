import { createModel } from "cx/ui";

export interface SheetRow {
    /** "1.", as the sheet's first column counts. */
    index: string;
    number: string;
    name: string;
    description: string;
    type: string;
}

export interface HandoverState {
    id: string;
    name: string;
    rows: SheetRow[];
    loading: boolean;
    error?: string;
}

export interface Model {
    handover: HandoverState;
    $route: { id: string };
    $row: SheetRow;
}

export default createModel<Model>();
