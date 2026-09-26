import { createModel } from "cx/ui";

import type { Expiry } from "../../../api/activations";
import type { LicenseDetail, LicenseForm, LicenseOptions, Option, Weighted } from "../../../api/licenses";
import { expiryText, formatDate } from "../../../licensing";

/** A volume as the form holds it: an existing one is kept or removed, a new one is filled in. */
export interface VolumeRow {
    /** The row's own key, stable while the form is open. */
    key: string;
    /** An existing volume's id; absent for one being added. */
    id?: string;
    softwareId?: string | null;
    softwareText?: string;
    typeId?: number | null;
    typeText?: string;
    quantity?: number | null;
    description?: string | null;
    /** An existing volume, as a line: "Office · Per user · 3 of 5 in use". */
    summary?: string;
    /** Why an existing volume cannot be removed, or absent. */
    held?: string;
    /** The activations list filtered to this volume's licence and software. */
    activationsHref?: string;
}

/** The form, as the fields bind it: text keys absent until typed, a pick as its id and text. */
export interface Draft {
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
    licenseTypeId?: string | null;
    licenseTypeText?: string;
    licenseModelId?: string | null;
    licenseModelText?: string;
    expirationModelId?: string | null;
    expirationModelText?: string;
    expirationDate?: string | null;
    subscriptionFee?: number | null;
    currencyId?: string | null;
    currencyText?: string;
    periodId?: string | null;
    periodText?: string;
    autoRenew: boolean;
    businessEntityId?: string | null;
    businessEntityText?: string;
    locationId?: string | null;
    locationText?: string;
    managementConsoleUrl?: string | null;
    registrationNumber?: string | null;
    keyIdentifier?: string | null;
    url?: string | null;
    volumes: VolumeRow[];
}

export interface EditorState {
    /** `null` while creating. */
    id: string | null;
    viewing: boolean;
    title: string;
    /** "#100893" beside the title, once there is one. */
    number?: string;
    draft: Draft;
    /** What the server said it held when loaded: echoed on save so an edit meanwhile is not overwritten. */
    lastModified?: string;
    /** Computed from the three weights as the server will; "—" until all three are chosen. */
    importance: string;
    expiry?: Expiry;
    expiryText?: string;
    options: LicenseOptions;
    loading: boolean;
    saving: boolean;
    error?: string;
    /** The save was refused because someone saved first. */
    stale: boolean;
    errors: Partial<Record<string, string>>;
    valid: boolean;
    visited: boolean;
}

export interface Model {
    license: EditorState;
    $route: { id: string };
    $volume: VolumeRow;
}

export default createModel<Model>();

export const emptyOptions: LicenseOptions = {
    vendors: [],
    people: [],
    confidentialities: [],
    integrities: [],
    availabilities: [],
    importances: [],
    licenseTypes: [],
    licenseModels: [],
    expirationModels: [],
    currencies: [],
    periods: [],
    businessEntities: [],
    locations: [],
    software: [],
    volumeTypes: [],
};

let next = 0;
export const rowKey = () => `v${++next}`;

/** The importance the server will compute: 3–4 Low, 5–7 Medium, 8–9 High, nothing unless all three. */
export function importanceFor(d: Draft, o: LicenseOptions): string {
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

const pick = (ref: { id: string; name: string } | null) => (ref ? { id: ref.id, text: ref.name } : undefined);

/** A loaded licence as the form: every pick as id and text, the volumes as kept rows. */
export function toDraft(l: LicenseDetail, duplicate: boolean): Draft {
    const d: Draft = {
        name: l.name,
        invoiceNumber: l.invoiceNumber,
        purchaseValue: l.purchaseValue,
        purchaseDate: l.purchaseDate,
        description: l.description,
        incomplete: l.incomplete,
        expirationDate: l.expirationDate,
        subscriptionFee: l.subscriptionFee,
        autoRenew: l.autoRenew,
        managementConsoleUrl: l.managementConsoleUrl,
        registrationNumber: l.registrationNumber,
        keyIdentifier: l.keyIdentifier,
        url: l.url,
        volumes: duplicate
            ? []
            : l.volumes.map((v) => ({
                  key: rowKey(),
                  id: v.id,
                  summary: `${v.software.name} · ${v.type.name} · ${v.inUse} of ${v.quantity} in use${v.description ? ` · ${v.description}` : ""}`,
                  held: v.held ?? undefined,
                  activationsHref:
                      v.activationCount > 0
                          ? `~/licenses/activations?licenseId=${l.id}&softwareId=${v.software.id}`
                          : undefined,
              })),
    };
    const picks: [string, { id: string; text: string } | undefined][] = [
        ["vendor", pick(l.vendor)],
        ["person", pick(l.person)],
        ["confidentiality", pick(l.confidentiality)],
        ["integrity", pick(l.integrity)],
        ["availability", pick(l.availability)],
        ["licenseType", pick(l.licenseType)],
        ["licenseModel", pick(l.licenseModel)],
        ["expirationModel", pick(l.expirationModel)],
        ["currency", pick(l.currency)],
        ["period", pick(l.period)],
        ["businessEntity", pick(l.businessEntity)],
        ["location", pick(l.location)],
    ];
    for (const [key, value] of picks)
        if (value) {
            (d as any)[`${key}Id`] = value.id;
            (d as any)[`${key}Text`] = value.text;
        }
    // Text keys stay absent rather than empty: '' is a value, and `required` would pass on it.
    for (const [key, value] of Object.entries(d)) if (value === null || value === "") delete (d as any)[key];
    return d;
}

const text = (v?: string | null) => v?.trim() || null;

/** What the server is sent. */
export const toForm = (d: Draft, lastModified?: string): LicenseForm => ({
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
    licenseTypeId: d.licenseTypeId ?? null,
    licenseModelId: d.licenseModelId ?? null,
    expirationModelId: d.expirationModelId ?? null,
    expirationDate: d.expirationDate ?? null,
    subscriptionFee: d.subscriptionFee ?? null,
    currencyId: d.currencyId ?? null,
    periodId: d.periodId ?? null,
    autoRenew: !!d.autoRenew,
    businessEntityId: d.businessEntityId ?? null,
    managementConsoleUrl: text(d.managementConsoleUrl),
    registrationNumber: text(d.registrationNumber),
    keyIdentifier: text(d.keyIdentifier),
    locationId: d.locationId ?? null,
    url: text(d.url),
    volumes: d.volumes.map((v) =>
        v.id
            ? { id: v.id }
            : {
                  softwareOrServiceId: v.softwareId ?? null,
                  volumeTypeId: v.typeId ?? null,
                  quantity: v.quantity ?? null,
                  description: text(v.description),
              },
    ),
    lastModified,
});

export const expiryLine = (l: LicenseDetail) =>
    l.expiry ? `${expiryText[l.expiry]} · ${formatDate(l.expirationDate)}` : undefined;

export type { Option };
