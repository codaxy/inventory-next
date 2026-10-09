import { send } from "./http";

/** What every printed document takes from the deployment. */
export interface DocumentSettings {
    /** The language a document opens in, unless its address names another. */
    defaultLanguage: string;
    /** Where documents are signed; empty leaves the line for a hand. */
    place: string;
}

export const getDocumentSettings = () => send<DocumentSettings>("/api/documents/settings");
