import { createModel } from "cx/ui";

import type { Option } from "../../../api/assets";
import type { DeviceDetail, DeviceForm, DeviceOptions } from "../../../api/electronicDevices";
import {
    type AssetDraft,
    assetParts,
    emptyAssetOptions,
    text,
    toAssetForm,
    toDraftOf,
} from "../../../assets";
import { informationKind, joinAll, plural, toSections } from "../../../holdings";
import { formatDate } from "../../../licensing";
import type { RecordState } from "../../../recordController";

/** A device as its form binds it: the asset's fields, then its own, each pick as id and text. */
export interface DeviceDraft extends AssetDraft {
    typeId?: string | null;
    typeText?: string;
    manufacturerId?: string | null;
    manufacturerText?: string;
    manufacturingDate?: string | null;
    modelName?: string | null;
    modelCode?: string | null;
    serialNumber?: string | null;
    warrantyNumber?: string | null;
    warrantyExpirationDate?: string | null;
}

export interface DeviceState extends RecordState<DeviceDraft> {
    options: DeviceOptions;
    /** Computed from the three weights as the server will; "—" until all three are chosen. */
    importance: string;
    /** The chosen type's tags, as chips. */
    tags: Option[];
    /** How many maintenance contracts go with it when it is deleted. */
    contracts: number;
}

export interface Model {
    device: DeviceState;
    $route: { id: string };
    $tag: Option;
}

export default createModel<Model>();

export const emptyOptions: DeviceOptions = {
    ...emptyAssetOptions,
    types: [],
    tags: [],
    manufacturers: [],
    typeTags: {},
};

export const blankDraft = (): DeviceDraft => ({ incomplete: false });

export const toDraft = (d: DeviceDetail): DeviceDraft => {
    const asset = assetParts(d);
    return toDraftOf<DeviceDraft>(
        {
            ...asset.plain,
            manufacturingDate: d.manufacturingDate,
            modelName: d.modelName,
            modelCode: d.modelCode,
            serialNumber: d.serialNumber,
            warrantyNumber: d.warrantyNumber,
            warrantyExpirationDate: d.warrantyExpirationDate,
        },
        { ...asset.refs, type: d.type, manufacturer: d.manufacturer },
    );
};

/** A copy: every field but what no two devices share — the serial number and the warranty's. */
export const toCopy = (d: DeviceDetail): DeviceDraft => {
    const { serialNumber: _, warrantyNumber: __, warrantyExpirationDate: ___, ...copy } = toDraft(d);
    return copy;
};

export const toForm = (d: DeviceDraft, lastModified?: string): DeviceForm => ({
    ...toAssetForm(d, lastModified),
    typeId: d.typeId ?? null,
    manufacturerId: d.manufacturerId ?? null,
    manufacturingDate: d.manufacturingDate ?? null,
    modelName: text(d.modelName),
    modelCode: text(d.modelCode),
    serialNumber: text(d.serialNumber),
    warrantyNumber: text(d.warrantyNumber),
    warrantyExpirationDate: d.warrantyExpirationDate ?? null,
});

const joined = (...parts: (string | null | undefined)[]) => parts.filter(Boolean).join(" · ") || undefined;

/**
 * Its maintenance contracts, the seats activated on it and the information kept on it. What keeps it
 * from being deleted is only the seats and the information: its contracts go with it.
 */
export function toAttached(d: DeviceDetail) {
    const attached = toSections(
        [
            {
                key: "contracts",
                title: "Maintenance contracts",
                none: "maintenance contracts",
                one: "maintenance contract",
                many: "maintenance contracts",
                total: d.contracts.length,
                rows: d.contracts.map((c) => ({
                    key: c.id,
                    title: c.vendor.name,
                    note: c.type ?? undefined,
                    meta: joined(
                        c.contractNumber ? `Contract ${c.contractNumber}` : undefined,
                        c.expirationDate ? `Expires ${formatDate(c.expirationDate)}` : undefined,
                        c.serviceDueDate ? `Service due ${formatDate(c.serviceDueDate)}` : undefined,
                        c.contactName,
                        c.contactNumber,
                        c.contactEmail,
                    ),
                })),
            },
            {
                key: "seats",
                title: "Seats",
                none: "seats",
                one: "seat",
                many: "seats",
                total: d.seats.total,
                rows: d.seats.items.map((s) => ({
                    key: s.id,
                    title: s.software,
                    note: s.person ? `For ${s.person}` : s.quantity > 1 ? `${s.quantity} seats` : undefined,
                    meta: s.license,
                    href: `~/licenses/activations/${s.id}`,
                    ended: s.deactivationDate ? `Deactivated ${formatDate(s.deactivationDate)}` : undefined,
                })),
            },
            informationKind(d.information),
        ],
        "Nothing is attached to it.",
        (kinds) => `No ${kinds} on it.`,
    );
    const held = [
        ...(d.seats.total ? [plural(d.seats.total, "seat", "seats")] : []),
        ...(d.information.total
            ? [plural(d.information.total, "piece of information", "pieces of information")]
            : []),
    ];
    return { ...attached, holds: held.length ? joinAll(held, "and") : undefined };
}
