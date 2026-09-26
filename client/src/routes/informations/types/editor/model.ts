import { createModel } from "cx/ui";

import type { GroupDetail, GroupForm } from "../../../../api/informations";
import { text } from "../../../../assets";
import { informationKind, toSections } from "../../../../holdings";
import type { RecordState } from "../../../../recordController";

export type GroupDraft = { name?: string | null; description?: string | null };

export interface Model {
    record: RecordState<GroupDraft>;
    $route: { id: string };
}

export default createModel<Model>();

export const toDraft = (g: GroupDetail): GroupDraft => ({ name: g.name, description: g.description });

export const toForm = (d: GroupDraft): GroupForm => ({
    name: (d.name ?? "").trim(),
    description: text(d.description),
});

export const toAttached = (g: GroupDetail) =>
    toSections(
        [informationKind(g.information, "typeId", g.id)],
        "No information is of it.",
        (kinds) => `No ${kinds} is of it.`,
    );
