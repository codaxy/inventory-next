import { createModel } from "cx/ui";

import type { Option } from "../../../../api/assets";
import type { OnVolumeDetail, OnVolumeForm } from "../../../../api/infrastructure";
import { text } from "../../../../assets";
import { volumeText } from "../../../../inventoryNumbers";
import { informationKind, toSections } from "../../../../holdings";
import type { RecordState } from "../../../../recordController";

export interface Draft {
    name?: string | null;
    volumeId?: string | null;
    volumeText?: string;
    managementUrl?: string | null;
}

export interface State extends RecordState<Draft> {
    options: { volumes: Option[] };
    /** The license the volume is under, for its link, and the volume in view: the license's number muted. */
    licenseHref?: string;
    licenseText?: string;
    licenseNumber?: string;
    designator?: string;
}

export interface Model {
    record: State;
    $route: { id: string };
}

export default createModel<Model>();

export const toDraft = (d: OnVolumeDetail): Draft => ({
    name: d.name,
    volumeId: d.volume.id,
    volumeText: volumeText(d.volume),
    managementUrl: d.managementUrl,
});

export const toForm = (d: Draft): OnVolumeForm => ({
    name: (d.name ?? "").trim(),
    volumeId: d.volumeId ?? null,
});

export const toAttached = (d: { id: string; information: OnVolumeDetail["information"] }) =>
    toSections(
        [informationKind(d.information, "softwareId", d.id)],
        "No information is kept on it.",
        (kinds) => `No ${kinds} kept on it.`,
    );
