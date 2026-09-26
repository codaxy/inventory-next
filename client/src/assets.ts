import type { AssetDetail, AssetForm, AssetOptions, Ref, Weighted } from "./api/assets";

/** An asset's fields as a form binds them: text keys absent until typed, a pick as its id and text. */
export interface AssetDraft {
    name?: string | null;
    invoiceNumber?: string | null;
    vendorId?: string | null;
    vendorText?: string;
    purchaseValue?: number | null;
    purchaseDate?: string | null;
    description?: string | null;
    personId?: string | null;
    personText?: string;
    confidentialityId?: string | null;
    confidentialityText?: string;
    integrityId?: string | null;
    integrityText?: string;
    availabilityId?: string | null;
    availabilityText?: string;
    incomplete: boolean;
    businessEntityId?: string | null;
    businessEntityText?: string;
    locationId?: string | null;
    locationText?: string;
    url?: string | null;
}

export const emptyAssetOptions: AssetOptions = {
    vendors: [],
    people: [],
    confidentialities: [],
    integrities: [],
    availabilities: [],
    businessEntities: [],
    locations: [],
};

/** The importance the server will compute: 3–4 Low, 5–7 Medium, 8–9 High, nothing unless all three. */
export function importanceFor(d: AssetDraft, o: AssetOptions): string {
    const weight = (list: Weighted[], id?: string | null) => list.find((x) => x.id === id)?.weight;
    const w = [
        weight(o.confidentialities, d.confidentialityId),
        weight(o.integrities, d.integrityId),
        weight(o.availabilities, d.availabilityId),
    ];
    if (w.some((x) => x == null)) return "—";
    const sum = w.reduce((a, b) => a! + b!, 0)!;
    return sum <= 4 ? "Low" : sum <= 7 ? "Medium" : "High";
}

/**
 * A draft from a loaded record: the plain fields as they are, each named reference (`vendor`) as the
 * picker's `vendorId` and `vendorText`, and blank text left absent — '' is a value, and `required`
 * would pass on it.
 */
export function toDraftOf<D extends AssetDraft>(
    plain: Omit<D, `${string}Id` | `${string}Text`>,
    refs: Record<string, Ref | null>,
): D {
    const d: Record<string, unknown> = { ...plain };
    for (const [key, ref] of Object.entries(refs))
        if (ref) {
            d[`${key}Id`] = ref.id;
            d[`${key}Text`] = ref.name;
        }
    for (const [key, value] of Object.entries(d)) if (value === null || value === "") delete d[key];
    return d as D;
}

/** The asset's part of a loaded record, for `toDraftOf`. */
export const assetParts = (a: AssetDetail) => ({
    plain: {
        name: a.name,
        invoiceNumber: a.invoiceNumber,
        purchaseValue: a.purchaseValue,
        purchaseDate: a.purchaseDate,
        description: a.description,
        incomplete: a.incomplete,
        url: a.url,
    },
    refs: {
        vendor: a.vendor,
        person: a.person,
        confidentiality: a.confidentiality,
        integrity: a.integrity,
        availability: a.availability,
        businessEntity: a.businessEntity,
        location: a.location,
    } as Record<string, Ref | null>,
});

/** Trimmed, and empty as `null`. */
export const text = (v?: string | null) => v?.trim() || null;

/** The asset's part of what the server is sent. */
export const toAssetForm = (d: AssetDraft, lastModified?: string): AssetForm => ({
    name: (d.name ?? "").trim(),
    invoiceNumber: text(d.invoiceNumber),
    vendorId: d.vendorId ?? null,
    purchaseValue: d.purchaseValue ?? null,
    purchaseDate: d.purchaseDate ?? null,
    description: text(d.description),
    personId: d.personId ?? null,
    confidentialityId: d.confidentialityId ?? null,
    integrityId: d.integrityId ?? null,
    availabilityId: d.availabilityId ?? null,
    incomplete: !!d.incomplete,
    businessEntityId: d.businessEntityId ?? null,
    locationId: d.locationId ?? null,
    url: text(d.url),
    lastModified,
});
