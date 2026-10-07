import { createModel } from "cx/ui";

export interface SheetRow {
    /** "1", as the sheet's first column counts. */
    index: string;
    number: string;
    name: string;
    description: string;
    type: string;
}

export interface SeatSheetRow {
    index: string;
    software: string;
    license: string;
    /** "#100655", muted beside the name; empty without one. */
    licenseNumber: string;
    /** "-" for a seat theirs by name. */
    device: string;
    deviceNumber: string;
    expires: string;
}

export interface HandoverState {
    id: string;
    name: string;
    rows: SheetRow[];
    seats: SeatSheetRow[];
    /** What the signature lines say; an empty one is left for a hand. */
    place: string;
    date: string;
    controller: string;
    /** The PDF's address, while the server can print it. */
    pdfHref?: string;
    /** `data-print`, which tells the server's printer when the sheet is whole. */
    print: "loading" | "ready" | "failed";
    error?: string;
}

export interface Model {
    handover: HandoverState;
    $route: { id: string };
    $row: SheetRow;
    $seat: SeatSheetRow;
}

export default createModel<Model>();
