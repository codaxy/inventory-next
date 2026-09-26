import { createModel } from "cx/ui";

import type { VendorDetail, VendorFields } from "../../../../api/vendors";
import { text } from "../../../../assets";
import { assetKinds, toSections } from "../../../../holdings";
import type { RecordState } from "../../../../recordController";

export type VendorDraft = { [K in keyof VendorFields]?: string | null };

export interface Model {
    record: RecordState<VendorDraft>;
    $route: { id: string };
}

export default createModel<Model>();

export const toDraft = (v: VendorDetail): VendorDraft => ({
    name: v.name,
    location: v.location,
    registrationNumber: v.registrationNumber,
    vatNumber: v.vatNumber,
    web: v.web,
    contactPerson: v.contactPerson,
    mobilePhone: v.mobilePhone,
    phone: v.phone,
    email: v.email,
});

export const toForm = (d: VendorDraft): VendorFields => ({
    name: (d.name ?? "").trim(),
    location: text(d.location),
    registrationNumber: text(d.registrationNumber),
    vatNumber: text(d.vatNumber),
    web: text(d.web),
    contactPerson: text(d.contactPerson),
    mobilePhone: text(d.mobilePhone),
    phone: text(d.phone),
    email: text(d.email),
});

/** What was bought from the vendor, and its maintenance contracts — each of which it would take with it. */
export const toAttached = (v: VendorDetail) =>
    toSections(
        [
            ...assetKinds(v.assets, "vendorId", v.id),
            {
                key: "contracts",
                title: "Maintenance contracts",
                none: "maintenance contracts",
                one: "maintenance contract",
                many: "maintenance contracts",
                total: v.contracts.total,
                rows: v.contracts.items.map((c) => ({
                    key: c.id,
                    title: c.asset ?? "—",
                    note: c.assetNumber ? `#${c.assetNumber}` : undefined,
                    meta: c.contractNumber ? `Contract ${c.contractNumber}` : undefined,
                })),
            },
        ],
        "Nothing was bought from them.",
        (kinds) => `No ${kinds} from them.`,
    );
