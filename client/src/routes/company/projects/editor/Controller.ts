import {
    createProject,
    deleteProject,
    getProject,
    getProjectOptions,
    type ProjectDetail,
    type ProjectForm,
    updateProject,
} from "../../../../api/projects";
import { RecordController } from "../../../../recordController";
import m, { type ProjectDraft, toAttached, toDraft, toForm } from "./model";

export default class extends RecordController<ProjectDraft, ProjectDetail, ProjectForm> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/company/projects";
    protected readonly noun = "project";
    protected readonly newTitle = "New project";

    protected load = getProject;
    protected create = createProject;
    protected update = updateProject;
    protected destroy = deleteProject;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (p: ProjectDetail) => p.name;
    protected attached = toAttached;

    protected refusal(holds: string) {
        return `${holds} ${holds.startsWith("1 ") ? "is" : "are"} of it. Give ${holds.startsWith("1 ") ? "it" : "them"} another project first, or keep this one.`;
    }

    onInit() {
        super.onInit();
        this.store.set(m.record.options, { clients: [], people: [] });
        getProjectOptions()
            .then((o) => this.store.set(m.record.options, o))
            .catch(() => {});
    }
}
