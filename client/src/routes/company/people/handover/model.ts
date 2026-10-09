import { createModel } from "cx/ui";

import type { Language } from "../../../../documents/languages";

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

/** Everything the sheet says in one language, but its dates, which `documentDate` writes. */
export interface HandoverText {
    /** "Interno", at its head; printed uppercase. */
    classification: string;
    /** The title's two lines. */
    title: [string, string];
    lead: string;
    terms: string[];
    assets: { index: string; number: string; name: string; description: string; type: string };
    /** The equipment table's one row when there is none. */
    noData: string;
    seats: {
        title: string;
        index: string;
        software: string;
        license: string;
        device: string;
        expires: string;
    };
    /** The line above the signatures: how many copies, and who keeps them. */
    closing: string;
    signatures: {
        place: string;
        name: string;
        date: string;
        signature: string;
        controller: string;
        responsible: string;
    };
}

export interface HandoverState {
    id: string;
    name: string;
    rows: SheetRow[];
    seats: SeatSheetRow[];
    /** The sheet's language: the address's, else the deployment's default; absent until known. */
    language?: Language;
    text?: HandoverText;
    /** The `@page` rule numbering the printed pages in the sheet's language. */
    pageNumbers?: string;
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
    $term: string;
}

export default createModel<Model>();
