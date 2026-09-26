import { createModel } from "cx/ui";

import type { Option } from "../../../../api/assets";
import type { ProjectDetail, ProjectForm } from "../../../../api/projects";
import { informationKind, toSections } from "../../../../holdings";
import type { RecordState } from "../../../../recordController";

export interface ProjectDraft {
    name?: string | null;
    clientId?: string | null;
    clientText?: string;
    ownerId?: string | null;
    ownerText?: string;
}

export interface ProjectState extends RecordState<ProjectDraft> {
    options: { clients: Option[]; people: Option[] };
}

export interface Model {
    record: ProjectState;
    $route: { id: string };
}

export default createModel<Model>();

export const toDraft = (p: ProjectDetail): ProjectDraft => ({
    name: p.name,
    clientId: p.client.id,
    clientText: p.client.name,
    ownerId: p.owner.id,
    ownerText: p.owner.name,
});

export const toForm = (d: ProjectDraft): ProjectForm => ({
    name: (d.name ?? "").trim(),
    clientId: d.clientId ?? null,
    ownerId: d.ownerId ?? null,
});

export const toAttached = (p: ProjectDetail) =>
    toSections(
        [informationKind(p.information, "projectId", p.id)],
        "No information is of it.",
        (kinds) => `No ${kinds} is of it.`,
    );
