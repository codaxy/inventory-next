import { createModel } from "cx/ui";

import type { MachineDetail, MachineForm } from "../../../../api/infrastructure";
import { text } from "../../../../assets";
import { informationKind, toSections } from "../../../../holdings";
import type { RecordState } from "../../../../recordController";

export interface Draft {
    name?: string | null;
    ipAddress?: string | null;
}

export interface Model {
    record: RecordState<Draft>;
    $route: { id: string };
}

export default createModel<Model>();

export const toDraft = (d: MachineDetail): Draft => ({ name: d.name, ipAddress: d.ipAddress });

export const toForm = (d: Draft): MachineForm => ({
    name: (d.name ?? "").trim(),
    ipAddress: text(d.ipAddress),
});

export const toAttached = (d: { id: string; information: MachineDetail["information"] }) =>
    toSections(
        [informationKind(d.information, "virtualMachineId", d.id)],
        "No information is kept on it.",
        (kinds) => `No ${kinds} kept on it.`,
    );
