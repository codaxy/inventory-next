/** What every asset's API shares: the pickers, and the asset's own fields beside a subtype's. */

export interface Ref {
    id: string;
    name: string;
}

export interface Option {
    id: string;
    text: string;
}

export interface Weighted extends Option {
    weight: number;
}

/** The pickers every asset form shows. */
export interface AssetOptions {
    vendors: Option[];
    people: Option[];
    confidentialities: Weighted[];
    integrities: Weighted[];
    availabilities: Weighted[];
    businessEntities: Option[];
    locations: Option[];
}

/** An asset as a detail carries it, flat beside the subtype's own fields. */
export interface AssetDetail {
    id: string;
    number: number | null;
    name: string;
    invoiceNumber: string | null;
    vendor: Ref;
    purchaseValue: number;
    purchaseDate: string;
    description: string | null;
    person: Ref;
    confidentiality: Ref | null;
    integrity: Ref | null;
    availability: Ref | null;
    importance: Ref | null;
    incomplete: boolean;
    businessEntity: Ref | null;
    location: Ref | null;
    url: string | null;
    lastModified: string;
}

/** An asset as a form sends it; `lastModified` echoed on an update. */
export interface AssetForm {
    name: string;
    invoiceNumber: string | null;
    vendorId: string | null;
    purchaseValue: number | null;
    purchaseDate: string | null;
    description: string | null;
    personId: string | null;
    confidentialityId: string | null;
    integrityId: string | null;
    availabilityId: string | null;
    incomplete: boolean;
    businessEntityId: string | null;
    locationId: string | null;
    url: string | null;
    lastModified?: string;
}
