import { createModel } from "cx/ui";

import type { Expiry } from "../../../api/activations";
import type { LicenseDetail, LicenseForm, LicenseOptions, Option } from "../../../api/licenses";
import {
    type AssetDraft,
    assetParts,
    emptyAssetOptions,
    text,
    toAssetForm,
    toDraftOf,
} from "../../../assets";
import { firstUrl } from "../../../components/externalLink";
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
    /** An existing volume, shown in parts: its software, then its type and description beneath. */
    software?: string;
    detail?: string;
    /** The web address in its description, for the open button. */
    url?: string;
    /** "3 / 5": the seats in use and bought, the figure a reader scans for. */
    seats?: string;
    /** How full, 0–100, for the meter; past 100 when more seats are in use than were bought. */
    fill?: number;
    /** `full` at the quantity, `over` past it: the meter's colour. */
    load?: "full" | "over";
    /** Why an existing volume cannot be removed, or absent. */
    held?: string;
    /** An existing volume marked to go when the licence is saved: struck through until then, and
     *  undone as easily. */
    removed?: boolean;
    /** The activations list filtered to exactly this volume; absent when it has none. */
    activationsHref?: string;
    /** "2 activations", deactivated ones included: what the link shows. */
    activationsText?: string;
    /** A new activation of this volume; absent when every seat is taken, as the shortcut would only
     *  lead past the quantity — still possible, deliberately, from the activations form. */
    activateHref?: string;
}

/** The form, as the fields bind it: the asset's fields, then the licence's own. */
export interface Draft extends AssetDraft {
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
    managementConsoleUrl?: string | null;
    registrationNumber?: string | null;
    keyIdentifier?: string | null;
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
    ...emptyAssetOptions,
    importances: [],
    licenseTypes: [],
    licenseModels: [],
    expirationModels: [],
    currencies: [],
    periods: [],
    software: [],
    volumeTypes: [],
};

let next = 0;
export const rowKey = () => `v${++next}`;

/** A loaded licence as the form: every pick as id and text, the volumes as kept rows. */
export function toDraft(l: LicenseDetail, duplicate: boolean): Draft {
    const asset = assetParts(l);
    return toDraftOf<Draft>(
        {
            ...asset.plain,
            expirationDate: l.expirationDate,
            subscriptionFee: l.subscriptionFee,
            autoRenew: l.autoRenew,
            managementConsoleUrl: l.managementConsoleUrl,
            registrationNumber: l.registrationNumber,
            keyIdentifier: l.keyIdentifier,
            volumes: duplicate ? [] : l.volumes.map(toVolumeRow),
        },
        {
            ...asset.refs,
            licenseType: l.licenseType,
            licenseModel: l.licenseModel,
            expirationModel: l.expirationModel,
            currency: l.currency,
            period: l.period,
        },
    );
}

const toVolumeRow = (v: LicenseDetail["volumes"][number]): VolumeRow => ({
    key: rowKey(),
    id: v.id,
    software: v.software.name,
    detail: v.description ? `${v.type.name} · ${v.description}` : v.type.name,
    url: firstUrl(v.description),
    seats: `${v.inUse} / ${v.quantity}`,
    fill: v.quantity > 0 ? Math.round((v.inUse / v.quantity) * 100) : 0,
    load: v.inUse > v.quantity ? "over" : v.inUse === v.quantity ? "full" : undefined,
    held: v.held ?? undefined,
    activationsHref: v.activationCount > 0 ? `~/licenses/activations?volumeId=${v.id}` : undefined,
    activationsText: v.activationCount === 1 ? "1 activation" : `${v.activationCount} activations`,
    activateHref:
        v.inUse < v.quantity ? `~/licenses/activations/new?volumeId=${v.id}&from=license` : undefined,
});

/** What the server is sent. */
export const toForm = (d: Draft, lastModified?: string): LicenseForm => ({
    ...toAssetForm(d, lastModified),
    licenseTypeId: d.licenseTypeId ?? null,
    licenseModelId: d.licenseModelId ?? null,
    expirationModelId: d.expirationModelId ?? null,
    expirationDate: d.expirationDate ?? null,
    subscriptionFee: d.subscriptionFee ?? null,
    currencyId: d.currencyId ?? null,
    periodId: d.periodId ?? null,
    autoRenew: !!d.autoRenew,
    managementConsoleUrl: text(d.managementConsoleUrl),
    registrationNumber: text(d.registrationNumber),
    keyIdentifier: text(d.keyIdentifier),
    volumes: d.volumes
        .filter((v) => !v.removed)
        .map((v) =>
            v.id
                ? { id: v.id }
                : {
                      softwareOrServiceId: v.softwareId ?? null,
                      volumeTypeId: v.typeId ?? null,
                      quantity: v.quantity ?? null,
                      description: text(v.description),
                  },
        ),
});

export const expiryLine = (l: LicenseDetail) =>
    l.expiry ? `${expiryText[l.expiry]} · ${formatDate(l.expirationDate)}` : undefined;

export type { Option };
