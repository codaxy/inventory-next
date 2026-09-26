import { createModel } from "cx/ui";

import type { Option } from "../../../api/assets";
import type { DeviceDetail } from "../../../api/electronicDevices";
import { type AssetDraft, assetParts, toDraftOf } from "../../../assets";
import { type HoldingSection, informationKind, toSections } from "../../../holdings";
import { formatDate } from "../../../licensing";

/** A device as its read-only page binds it: the asset's fields and its own, each pick as id and text. */
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

export interface DeviceState {
    id: string;
    /** Always: the page is read-only until editing lands. */
    viewing: boolean;
    title: string;
    number?: string;
    draft: DeviceDraft;
    importance: string;
    tags: Option[];
    sections: HoldingSection[];
    none?: string;
    loading: boolean;
    error?: string;
    errors: Record<string, string | undefined>;
}

export interface Model {
    device: DeviceState;
    $route: { id: string };
    $tag: Option;
}

export default createModel<Model>();

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

const joined = (...parts: (string | null | undefined)[]) => parts.filter(Boolean).join(" · ") || undefined;

/** Its maintenance contracts, the seats activated on it and the information kept on it. */
export const toAttached = (d: DeviceDetail) =>
    toSections(
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
