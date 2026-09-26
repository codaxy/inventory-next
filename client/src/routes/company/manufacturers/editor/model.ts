import { createModel } from "cx/ui";

import type { ManufacturerDetail, ManufacturerForm } from "../../../../api/manufacturers";
import { text } from "../../../../assets";
import { assetRow, toSections } from "../../../../holdings";
import type { RecordState } from "../../../../recordController";

export type ManufacturerDraft = { name?: string | null; url?: string | null };

export interface Model {
    record: RecordState<ManufacturerDraft>;
    $route: { id: string };
}

export default createModel<Model>();

export const toDraft = (d: ManufacturerDetail): ManufacturerDraft => ({ name: d.name, url: d.url });

export const toForm = (d: ManufacturerDraft): ManufacturerForm => ({
    name: (d.name ?? "").trim(),
    url: text(d.url),
});

/** Their devices and their software and services — the software goes with them, the database cascading. */
export const toAttached = (d: ManufacturerDetail) =>
    toSections(
        [
            {
                key: "devices",
                title: "Electronic devices",
                none: "electronic devices",
                one: "electronic device",
                many: "electronic devices",
                total: d.devices.total,
                rows: d.devices.items.map((x) => assetRow(x, `~/electronic-devices/${x.id}`)),
                moreHref: `~/electronic-devices?manufacturerId=${d.id}`,
            },
            {
                key: "software",
                title: "Software and services",
                none: "software or services",
                one: "software or service",
                many: "software and services",
                total: d.software.total,
                rows: d.software.items.map((s) => ({
                    key: s.id,
                    title: s.name,
                    meta: s.category ?? undefined,
                    href: `~/licenses/software-services/${s.id}`,
                })),
                moreHref: `~/licenses/software-services?manufacturerId=${d.id}`,
            },
        ],
        "Nothing of theirs is recorded.",
        (kinds) => `No ${kinds} of theirs.`,
    );
